using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : MonoBehaviour
{
    [Header("UI Templates")]
    public VisualTreeAsset ItemTemplate;
    public VisualTreeAsset PopupTemplate;
    public VisualTreeAsset ContextMenuTemplate;
    
    [Header("Effects")]
    public GameObject ConfettiPrefab;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    private VisualElement _shopContainer, _inventoryContainer, _craftContainer, _battleContainer;
    private ScrollView _shopScroll, _invScroll, _craftScroll;
    private Label _goldLabel, _gemLabel, _playerLevelLabel;
    private ProgressBar _hpBar, _staminaBar, _expBar;
    private Label _pageLabel;
    private int _currentPage = 1;
    private int _pageSize = 8;
    private ProgressBar _monsterHpBar;
    private Button _btnAttack;
    private MonsterDto _currentMonster;
    private CharacterDto _currentProfile;
    private List<InventoryDto> _fullInventory = new List<InventoryDto>();

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        
        _shopContainer = _root.Q<VisualElement>("ShopContainer");
        _inventoryContainer = _root.Q<VisualElement>("InventoryContainer");
        _craftContainer = _root.Q<VisualElement>("CraftContainer");
        _battleContainer = _root.Q<VisualElement>("BattleContainer");

        _shopScroll = _root.Q<ScrollView>("ShopScrollView");
        _invScroll = _root.Q<ScrollView>("InventoryScrollView");
        _craftScroll = _root.Q<ScrollView>("CraftScrollView");

        
        _goldLabel = _root.Q<Label>("ShopGold");
        _gemLabel = _root.Q<Label>("ShopGem");
        _hpBar = _root.Q<ProgressBar>("HpBar");
        _staminaBar = _root.Q<ProgressBar>("StaminaBar");
        _expBar = _root.Q<ProgressBar>("ExpBar");
        _playerLevelLabel = _root.Q<Label>("LevelLabel");

        
        SetupTabButton("TabShop", "Shop");
        SetupTabButton("TabInventory", "Inventory");
        SetupTabButton("TabCraft", "Craft");
        SetupTabButton("TabBattle", "Battle");

        
        var btnPrev = _root.Q<Button>("BtnPrev");
        var btnNext = _root.Q<Button>("BtnNext");
        _pageLabel = _root.Q<Label>("PageLabel");
        
        if (btnPrev != null) btnPrev.clicked += () => ChangePage(-1);
        if (btnNext != null) btnNext.clicked += () => ChangePage(1);

        _monsterHpBar = _root.Q<ProgressBar>("MonsterHpBar");
        _btnAttack = _root.Q<Button>("BtnAttack");
        if (_btnAttack != null) _btnAttack.clicked += () => StartCoroutine(AttackProcess());

        SetupInvFilter("BtnFilterAll", "All");
        SetupInvFilter("BtnFilterWep", "Weapon");
        SetupInvFilter("BtnFilterCon", "Consumable");

        var btnLogs = _root.Q<Button>("BtnNotiLog");
        if (btnLogs != null) btnLogs.clicked += () => StartCoroutine(LoadTransactionHistory());

        GameEvents.OnCurrencyChanged += RefreshAllData;
        GameEvents.OnEquipRequest += HandleEquipRequest;

        StartCoroutine(LoadProfile());
        SwitchTab("Shop");
    }

    void OnDisable()
    {
        GameEvents.OnCurrencyChanged -= RefreshAllData;
        GameEvents.OnEquipRequest -= HandleEquipRequest;
    }

    void RefreshAllData()
    {
        StartCoroutine(LoadProfile());
        if (_inventoryContainer != null && _inventoryContainer.style.display == DisplayStyle.Flex) StartCoroutine(LoadInventory());
    }

    void HandleEquipRequest(string itemId) { StartCoroutine(EquipItem(itemId)); }

    public void UseItemFromHotbar(string itemId)
    {
        var item = _fullInventory.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            if (item.Type == "Consumable") StartCoroutine(UseItem(itemId));
            else StartCoroutine(EquipItem(itemId));
        }
    }

    void SetupTabButton(string btnName, string tabName)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null) 
        {
            btn.clicked -= () => SwitchTab(tabName);
            btn.clicked += () => SwitchTab(tabName);
        }
    }

    void SetupInvFilter(string btnName, string type)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null) btn.clicked += () => FilterInventory(type);
    }

    void SwitchTab(string tabName)
    {
        if (_shopContainer != null) _shopContainer.style.display = DisplayStyle.None;
        if (_inventoryContainer != null) _inventoryContainer.style.display = DisplayStyle.None;
        if (_craftContainer != null) _craftContainer.style.display = DisplayStyle.None;
        if (_battleContainer != null) _battleContainer.style.display = DisplayStyle.None;

        UpdateTabVisual("TabShop", tabName == "Shop");
        UpdateTabVisual("TabInventory", tabName == "Inventory");
        UpdateTabVisual("TabCraft", tabName == "Craft");
        UpdateTabVisual("TabBattle", tabName == "Battle");

        if (tabName == "Shop") { _shopContainer.style.display = DisplayStyle.Flex; StartCoroutine(LoadShopItems(_currentPage)); }
        else if (tabName == "Inventory") { _inventoryContainer.style.display = DisplayStyle.Flex; StartCoroutine(LoadInventory()); }
        else if (tabName == "Craft") { _craftContainer.style.display = DisplayStyle.Flex; StartCoroutine(LoadRecipes()); }
        else if (tabName == "Battle") { _battleContainer.style.display = DisplayStyle.Flex; StartCoroutine(SpawnMonster()); }
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    void UpdateTabVisual(string btnName, bool isActive)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null)
        {
            if (isActive) btn.AddToClassList("active-tab");
            else btn.RemoveFromClassList("active-tab");
        }
    }

    IEnumerator LoadProfile()
    {
        yield return NetworkManager.Instance.SendRequest<CharacterDto>("game/profile/me", "GET", null,
            (res) => {
                _currentProfile = res;
                if (_goldLabel != null) _goldLabel.text = $"{res.Gold:N0} G";
                if (_gemLabel != null) _gemLabel.text = $"{res.Gem:N0} 💎";
                if (_hpBar != null) { _hpBar.value = res.Health; _hpBar.highValue = res.MaxHealth; _hpBar.title = $"{res.Health}/{res.MaxHealth}"; }
                if (_staminaBar != null) _staminaBar.value = res.Hunger;
                if (_playerLevelLabel != null) _playerLevelLabel.text = $"Lv.{res.Level}";
            }, null
        );
    }

    void ChangePage(int dir)
    {
        _currentPage += dir;
        if (_currentPage < 1) _currentPage = 1;
        StartCoroutine(LoadShopItems(_currentPage));
    }

    IEnumerator LoadShopItems(int page)
    {
        if (_shopScroll == null) yield break;
        _shopScroll.Clear();
        
        yield return NetworkManager.Instance.SendRequest<List<ShopItemDto>>($"game/shop?page={page}&pageSize={_pageSize}", "GET", null,
            (items) => {
                if (_shopScroll == null) return;
                _shopScroll.Clear();
                if (items.Count == 0 && page > 1) { _currentPage--; ChangePage(0); return; }
                if (_pageLabel != null) _pageLabel.text = $"Page {_currentPage}";
                
                foreach (var item in items) {
                    var card = CreateItemCard(item);
                    _shopScroll.Add(card);
                }
            },
            (err) => ToastManager.Instance.Show("Lỗi tải Shop: " + err, false)
        );
    }

    VisualElement CreateItemCard(ShopItemDto item)
    {
        var template = ItemTemplate.Instantiate();
        var root = template.Q<VisualElement>("ItemContainer");
        template.Q<Label>("ItemName").text = item.Name;
        StartCoroutine(template.Q<Image>("ItemImage").LoadImage(item.ImageURL));
        
        if (!string.IsNullOrEmpty(item.Rarity)) root.AddToClassList($"rarity-{item.Rarity.ToLower()}");
        
        root.RegisterCallback<ClickEvent>(evt => ShowDetailPopup(item));

        var btnGold = template.Q<Button>("BtnBuyGold");
        var btnGem = template.Q<Button>("BtnBuyGem");
        
        if (item.PriceCurrency == "RES_GOLD") {
            btnGem.style.display = DisplayStyle.None;
            btnGold.Q<Label>("PriceGoldLabel").text = item.PriceAmount.ToString();
            btnGold.clicked += () => ShowDetailPopup(item);
        } else {
            btnGold.style.display = DisplayStyle.None;
            btnGem.Q<Label>("PriceGemLabel").text = item.PriceAmount.ToString();
            btnGem.clicked += () => ShowDetailPopup(item);
        }
        return template;
    }

    void ShowDetailPopup(ShopItemDto item)
    {
        if (PopupTemplate == null) { Debug.LogError("Chưa gán Popup Template!"); return; }

        var popup = PopupTemplate.Instantiate();
        var overlay = popup.Q<VisualElement>("DetailOverlay");
        
        if (overlay == null) { Debug.LogError("Không tìm thấy 'DetailOverlay'!"); return; }

        overlay.style.position = Position.Absolute;
        overlay.style.width = Length.Percent(100);
        overlay.style.height = Length.Percent(100);
        
        _root.Add(overlay);

        var lblName = overlay.Q<Label>("DetailName");
        if (lblName != null) lblName.text = item.Name;

        var lblDesc = overlay.Q<Label>("DetailDesc");
        if (lblDesc != null) lblDesc.text = item.Description;

        var img = overlay.Q<Image>("DetailImage");
        if (img != null) StartCoroutine(img.LoadImage(item.ImageURL));

        int qty = 1;
        var lblQty = overlay.Q<Label>("LblQuantity");
        var lblTotal = overlay.Q<Label>("LblTotalPrice");

        Action UpdatePrice = () => {
            if (lblQty != null) lblQty.text = qty.ToString();
            if (lblTotal != null) {
                int total = item.PriceAmount * qty;
                lblTotal.text = $"Total: {total:N0} {(item.PriceCurrency == "RES_GOLD" ? "G" : "💎")}";
            }
        };

        var btnPlus = overlay.Q<Button>("BtnPlus");
        if (btnPlus != null) btnPlus.clicked += () => { qty++; UpdatePrice(); };

        var btnMinus = overlay.Q<Button>("BtnMinus");
        if (btnMinus != null) btnMinus.clicked += () => { if (qty > 1) qty--; UpdatePrice(); };

        var btnConfirm = overlay.Q<Button>("BtnConfirmBuy");
        if (btnConfirm != null) {
            btnConfirm.clicked += () => {
                StartCoroutine(BuyProcess(item.ProductID, qty));
                if (_root.Contains(overlay)) _root.Remove(overlay);
            };
        }

        var btnClose = overlay.Q<Button>("BtnCloseDetail");
        if (btnClose != null) {
            btnClose.clicked += () => {
                if (_root.Contains(overlay)) _root.Remove(overlay);
            };
        }

        UpdatePrice();
    }

    IEnumerator BuyProcess(string prodId, int qty)
    {
        var body = new BuyRequest { ProductId = prodId, Quantity = qty };
        yield return NetworkManager.Instance.SendRequest<object>("game/buy", "POST", body,
            (res) => {
                ToastManager.Instance.Show("Mua thành công!", true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("success");
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator LoadInventory()
    {
        if (_invScroll == null) yield break;
        _invScroll.Clear();
        _invScroll.Add(new Label("Loading Inventory...") { style = { color = Color.gray, alignSelf = Align.Center } });

        yield return NetworkManager.Instance.SendRequest<List<InventoryDto>>("game/inventory", "GET", null,
            (items) => { 
                _fullInventory = items; 
                FilterInventory("All"); 
            }, 
            (err) => {
                if (_invScroll != null) {
                    _invScroll.Clear();
                    _invScroll.Add(new Label("Failed to load.") { style = { color = Color.red } });
                }
            }
        );
    }

    void FilterInventory(string type)
    {
        if (_invScroll == null) return;
        _invScroll.Clear();
        var list = (type == "All") ? _fullInventory : _fullInventory.Where(i => i.Type == type).ToList();
        
        if (list.Count == 0)
        {
            _invScroll.Add(new Label("Túi đồ trống.") { style = { color = Color.white, marginTop = 20, alignSelf = Align.Center } });
            return;
        }

        foreach (var inv in list)
        {
            var ui = ItemTemplate.Instantiate();
            var root = ui.Q<VisualElement>("ItemContainer");
            ui.Q<Label>("ItemName").text = inv.Name;
            StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(inv.ImageUrl));
            
            if (!string.IsNullOrEmpty(inv.Rarity)) root.AddToClassList($"rarity-{inv.Rarity.ToLower()}");
            ui.Q<VisualElement>("price-row").style.display = DisplayStyle.None;
            root.Add(new Label($"x{inv.Quantity}") { style = { position = Position.Absolute, bottom = 2, right = 5, fontSize = 12 } });
            if (inv.IsEquipped) root.Add(new Label("E") { style = { position = Position.Absolute, top = 2, left = 2, backgroundColor = Color.green, fontSize = 10 } });
            root.userData = inv.ItemId;
            
            root.RegisterCallback<ClickEvent>(e => {
                if (e.button == 1) ShowContextMenu(inv, e.position); 
            });
            root.AddManipulator(new DragManipulator(root, _root));
            _invScroll.Add(ui);
        }
    }

    void ShowContextMenu(InventoryDto inv, Vector2 mousePos)
    {
        var old = _root.Q("ContextMenu");
        if (old != null) old.style.display = DisplayStyle.None;

        var menu = ContextMenuTemplate.Instantiate();
        var menuRoot = menu.Q<VisualElement>("ContextMenu");
        
        float x = mousePos.x;
        float y = mousePos.y;
        if (x + 120 > _root.resolvedStyle.width) x -= 120;
        if (y + 150 > _root.resolvedStyle.height) y -= 150;

        menuRoot.style.left = x;
        menuRoot.style.top = y;
        menuRoot.style.display = DisplayStyle.Flex;

        menu.Q<Button>("BtnCtxUse").clicked += () => { 
            if (inv.Type == "Consumable") StartCoroutine(UseItem(inv.ItemId));
            else StartCoroutine(EquipItem(inv.ItemId));
            _root.Remove(menuRoot);
        };

        menu.Q<Button>("BtnCtxSell").clicked += () => {
            StartCoroutine(SellItem(inv.ItemId, 1));
            _root.Remove(menuRoot);
        };

        menu.Q<Button>("BtnCtxSellAll").clicked += () => {
            StartCoroutine(SellItem(inv.ItemId, inv.Quantity));
            _root.Remove(menuRoot);
        };

        menu.Q<Button>("BtnCtxCancel").clicked += () => _root.Remove(menuRoot);
        _root.Add(menuRoot);
    }

    IEnumerator SellItem(string itemId, int qty)
    {
        var body = new BuyRequest { ProductId = itemId, Quantity = qty };
        yield return NetworkManager.Instance.SendRequest<object>("game/sell", "POST", body,
            (res) => {
                ToastManager.Instance.Show("Đã bán thành công!", true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("coins");
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => ToastManager.Instance.Show("Lỗi bán hàng: " + err, false)
        );
    }

    IEnumerator UseItem(string itemId)
    {
        yield return NetworkManager.Instance.SendRequest<object>($"game/use-item/{itemId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Đã sử dụng!", true); RefreshAllData(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator EquipItem(string itemId)
    {
        yield return NetworkManager.Instance.SendRequest<object>($"game/equip/{itemId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Đã thay đổi trang bị!", true); RefreshAllData(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator LoadTransactionHistory()
    {
        var panel = _root.Q<VisualElement>("NotiLogPanel");
        if (panel == null) yield break;
        panel.style.display = DisplayStyle.Flex;
        
        var list = panel.Q<ScrollView>("NotiLogList");
        list.Clear();
        list.Add(new Label("Đang tải lịch sử...") { style = { color = Color.gray } });
        yield return NetworkManager.Instance.SendRequest<List<TransactionDto>>("game/transactions/my", "GET", null,
            (logs) => {
                list.Clear();
                if (logs.Count == 0) list.Add(new Label("Chưa có giao dịch nào.") { style = { color = Color.white } });
                
                foreach(var log in logs) {
                    string currencySymbol = (log.Currency == "RES_GEM") ? "💎" : "G";
                    var row = new Label($"[{log.Date}] {log.Action} ({log.Amount} {currencySymbol})");
                    row.style.color = log.Amount >= 0 ? Color.green : new Color(1f, 0.4f, 0.4f);
                    row.style.borderBottomWidth = 1;
                    row.style.borderBottomColor = new Color(1,1,1,0.1f);
                    list.Add(row);
                }
            },
            (err) => { list.Clear(); list.Add(new Label("Lỗi tải: " + err)); }
        );
    }

    IEnumerator LoadRecipes() { 
        _craftScroll.Clear();
        _craftScroll.Add(new Label("Loading Recipes...") { style = { color = Color.gray } });

        yield return NetworkManager.Instance.SendRequest<List<RecipeDto>>("game/recipes", "GET", null, (recipes) => {
             _craftScroll.Clear();
             if (recipes.Count == 0) _craftScroll.Add(new Label("No Recipes Available.") { style = { color = Color.white } });

             foreach(var r in recipes) {
                 var ui = ItemTemplate.Instantiate();
                 var root = ui.Q<VisualElement>("ItemContainer");
                 ui.Q<Label>("ItemName").text = r.ResultItemName;
                 StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(r.ResultItemImage));
                 var btn = new Button { text = $"CRAFT ({r.CraftingTime}s)" };
                 btn.AddToClassList("btn-buy");
                 btn.clicked += () => StartCoroutine(CraftProcess(r));
                 ui.Q<VisualElement>("price-row").style.display = DisplayStyle.None;
                 root.Add(btn);
                 _craftScroll.Add(ui);
             }
        }, null);
    }

    IEnumerator CraftProcess(RecipeDto r) {
        ToastManager.Instance.Show($"Đang chế tạo {r.ResultItemName}...", true);
        if(AudioManager.Instance!=null) AudioManager.Instance.PlaySFX("craft");
        yield return new WaitForSeconds(r.CraftingTime);
        yield return NetworkManager.Instance.SendRequest<object>($"game/craft/{r.RecipeId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Chế tạo hoàn tất!", true); GameEvents.TriggerCurrencyChanged(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator SpawnMonster() { 
        _currentMonster = new MonsterDto { Name = "Zombie", HP = 100, MaxHp = 100 };
        _root.Q<Label>("MonsterName").text = _currentMonster.Name;
        _monsterHpBar.value = 100;
        yield break; 
    }

    
    IEnumerator AttackProcess() {
        if (_currentMonster != null) {
            if (_monsterHpBar != null) _monsterHpBar.value -= 10; 
            
            
            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.ShowDamage(_btnAttack.worldBound.center, 10, false);
            }
        }

        yield return NetworkManager.Instance.SendRequest<HuntResponse>("game/hunt", "POST", null,
            (res) => {
                ToastManager.Instance.Show($"Damage dealt! +{res.GoldEarned}G", true);
                if(res.LevelUp) ToastManager.Instance.Show("LEVEL UP!", true);
                GameEvents.TriggerCurrencyChanged();
            }, null
        );
    }
}