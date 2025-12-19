using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI Templates")]
    public VisualTreeAsset ItemTemplate;
    public VisualTreeAsset PopupTemplate;
    public VisualTreeAsset ContextMenuTemplate;

    [Header("Effects")]
    public GameObject ConfettiPrefab;

    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _shopContainer, _inventoryContainer, _craftContainer, _battleContainer;

    // [QUAN TRỌNG] Biến lưu Wrapper để đo kích thước chuẩn
    private VisualElement _shopWrapper;

    private ScrollView _shopScroll, _invScroll, _craftScroll;
    private Label _goldLabel, _gemLabel, _playerLevelLabel;
    private ProgressBar _hpBar, _staminaBar;
    private Label _pageLabel;

    private Button _btnTabShop, _btnTabInv, _btnTabCraft, _btnTabBattle;
    private Button _btnFilterAll, _btnFilterWep, _btnFilterCon;

    private int _currentPage = 1;
    private int _pageSize = 10;

    // Chiều cao item trong CSS (.table-row) là 70px + 1px border = 71px. 
    // Ta để 72px cho dư dả, đảm bảo làm tròn xuống an toàn.
    private const float ITEM_HEIGHT = 72f;

    private ProgressBar _monsterHpBar;
    private Button _btnAttack;
    private MonsterDto _currentMonster;
    private CharacterDto _currentProfile;
    private List<InventoryDto> _fullInventory = new List<InventoryDto>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        // --- Lấy các Container chính ---
        _shopContainer = _root.Q<VisualElement>("ShopContainer");
        _inventoryContainer = _root.Q<VisualElement>("InventoryContainer");
        _craftContainer = _root.Q<VisualElement>("CraftContainer");
        _battleContainer = _root.Q<VisualElement>("BattleContainer");

        // --- Lấy ScrollViews ---
        _shopScroll = _root.Q<ScrollView>("ShopScrollView");
        _invScroll = _root.Q<ScrollView>("InventoryScrollView");
        _craftScroll = _root.Q<ScrollView>("CraftScrollView");

        // --- [QUAN TRỌNG] Lấy Wrapper của Shop để đo chiều cao ---
        // Class "list-wrapper" là cái khung cha có flex-grow: 1 mà ta đã thêm trong UXML
        if (_shopContainer != null)
        {
            _shopWrapper = _shopContainer.Q(className: "list-wrapper");

            // Đăng ký sự kiện thay đổi kích thước trên Wrapper (thay vì ScrollView)
            // Điều này tránh lỗi deadlock vì Wrapper luôn có chiều cao cố định
            if (_shopWrapper != null)
            {
                _shopWrapper.RegisterCallback<GeometryChangedEvent>(OnShopWrapperLayoutChange);
            }
        }

        // --- Lấy Stats UI ---
        _goldLabel = _root.Q<Label>("ShopGold");
        _gemLabel = _root.Q<Label>("ShopGem");
        _hpBar = _root.Q<ProgressBar>("HpBar");
        _staminaBar = _root.Q<ProgressBar>("StaminaBar");
        _playerLevelLabel = _root.Q<Label>("LevelLabel");

        // --- Setup Main Tabs ---
        _btnTabShop = SetupTabButton("TabShop", "Shop");
        _btnTabInv = SetupTabButton("TabInventory", "Inventory");
        _btnTabCraft = SetupTabButton("TabCraft", "Craft");
        _btnTabBattle = SetupTabButton("TabBattle", "Battle");

        // --- Pagination ---
        var btnPrev = _root.Q<Button>("BtnPrev");
        var btnNext = _root.Q<Button>("BtnNext");
        _pageLabel = _root.Q<Label>("PageLabel");

        if (btnPrev != null) btnPrev.clicked += () => ChangePage(-1);
        if (btnNext != null) btnNext.clicked += () => ChangePage(1);

        // --- Battle Zone ---
        _monsterHpBar = _root.Q<ProgressBar>("MonsterHpBar");
        _btnAttack = _root.Q<Button>("BtnAttack");
        if (_btnAttack != null) _btnAttack.clicked += () => StartCoroutine(AttackProcess());

        // --- Setup Inventory Filter ---
        _btnFilterAll = SetupInvFilter("BtnFilterAll", "All");
        _btnFilterWep = SetupInvFilter("BtnFilterWep", "Weapon");
        _btnFilterCon = SetupInvFilter("BtnFilterCon", "Consumable");

        var btnLogs = _root.Q<Button>("BtnNotiLog");
        if (btnLogs != null) btnLogs.clicked += () => StartCoroutine(LoadTransactionHistory());

        // --- Events ---
        GameEvents.OnCurrencyChanged += RefreshAllData;
        GameEvents.OnEquipRequest += HandleEquipRequest;

        // --- Init ---
        StartCoroutine(LoadProfile());
        SwitchTab("Shop");
    }

    void OnDisable()
    {
        GameEvents.OnCurrencyChanged -= RefreshAllData;
        GameEvents.OnEquipRequest -= HandleEquipRequest;

        // Hủy đăng ký sự kiện để tránh lỗi bộ nhớ
        if (_shopWrapper != null)
            _shopWrapper.UnregisterCallback<GeometryChangedEvent>(OnShopWrapperLayoutChange);
    }

    // --- [THUẬT TOÁN MỚI] TÍNH TOÁN SỐ LƯỢNG ITEM ---
    private void OnShopWrapperLayoutChange(GeometryChangedEvent evt)
    {
        float wrapperHeight = evt.newRect.height;

        // Nếu khung quá nhỏ (chưa load xong), bỏ qua
        if (wrapperHeight < ITEM_HEIGHT) return;

        // Tính số lượng item nhét vừa
        // Mathf.FloorToInt: Tự động làm tròn xuống (ví dụ 10.9 -> 10)
        // Điều này đảm bảo tổng chiều cao items luôn < chiều cao khung -> Không hiện Scrollbar
        int fitCount = Mathf.FloorToInt(wrapperHeight / ITEM_HEIGHT);

        // Giới hạn tối thiểu là 1 item
        if (fitCount < 1) fitCount = 1;

        // Chỉ load lại nếu số lượng thay đổi để tối ưu hiệu năng
        if (fitCount != _pageSize)
        {
            _pageSize = fitCount;
            // Gọi load lại dữ liệu ngay
            if (_shopContainer.style.display == DisplayStyle.Flex)
            {
                StartCoroutine(LoadShopItems(_currentPage));
            }
        }
    }

    public void UseItemFromHotbar(string itemId)
    {
        var item = _fullInventory.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            if (item.Type == "Consumable") StartCoroutine(UseItem(itemId));
            else StartCoroutine(EquipItem(itemId));
        }
        else
        {
            ToastManager.Instance.Show("Item không tồn tại!", false);
        }
    }

    void RefreshAllData()
    {
        StartCoroutine(LoadProfile());
        if (_inventoryContainer != null && _inventoryContainer.style.display == DisplayStyle.Flex)
            StartCoroutine(LoadInventory());
    }

    void HandleEquipRequest(string itemId) { StartCoroutine(EquipItem(itemId)); }

    Button SetupTabButton(string btnName, string tabName)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null)
        {
            btn.clicked -= () => SwitchTab(tabName);
            btn.clicked += () => SwitchTab(tabName);
        }
        return btn;
    }

    Button SetupInvFilter(string btnName, string type)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null) btn.clicked += () => FilterInventory(type);
        return btn;
    }

    void SwitchTab(string tabName)
    {
        if (_shopContainer != null) _shopContainer.style.display = DisplayStyle.None;
        if (_inventoryContainer != null) _inventoryContainer.style.display = DisplayStyle.None;
        if (_craftContainer != null) _craftContainer.style.display = DisplayStyle.None;
        if (_battleContainer != null) _battleContainer.style.display = DisplayStyle.None;

        SetTabActive(_btnTabShop, tabName == "Shop");
        SetTabActive(_btnTabInv, tabName == "Inventory");
        SetTabActive(_btnTabCraft, tabName == "Craft");
        SetTabActive(_btnTabBattle, tabName == "Battle");

        if (tabName == "Shop")
        {
            _shopContainer.style.display = DisplayStyle.Flex;
            // Chỉ load nếu pageSize đã được tính toán hợp lý
            if (_pageSize > 0) StartCoroutine(LoadShopItems(_currentPage));
        }
        else if (tabName == "Inventory")
        {
            _inventoryContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadInventory());
        }
        else if (tabName == "Craft")
        {
            _craftContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadRecipes());
        }
        else if (tabName == "Battle")
        {
            _battleContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(SpawnMonster());
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    void SetTabActive(Button btn, bool isActive)
    {
        if (btn == null) return;
        btn.RemoveFromClassList("active");
        if (isActive) btn.AddToClassList("active");
    }

    void UpdateFilterVisual(string activeType)
    {
        SetFilterStyle(_btnFilterAll, activeType == "All");
        SetFilterStyle(_btnFilterWep, activeType == "Weapon");
        SetFilterStyle(_btnFilterCon, activeType == "Consumable");
    }

    void SetFilterStyle(Button btn, bool isActive)
    {
        if (btn == null) return;
        btn.RemoveFromClassList("active");
        if (isActive) btn.AddToClassList("active");
    }

    void FilterInventory(string type)
    {
        UpdateFilterVisual(type);
        if (_invScroll == null) return;
        _invScroll.Clear();

        var list = (type == "All") ? _fullInventory : _fullInventory.Where(i => i.Type == type).ToList();

        if (list.Count == 0)
        {
            _invScroll.Add(new Label("No items in bag.") { style = { color = Color.gray, marginTop = 20, alignSelf = Align.Center } });
            return;
        }

        int index = 0;
        foreach (var inv in list)
        {
            var ui = ItemTemplate.Instantiate();
            var root = ui.Q<VisualElement>("ItemContainer");

            if (index % 2 == 0) root.AddToClassList("row-even");
            else root.AddToClassList("row-odd");
            index++;

            ui.Q<Label>("ItemName").text = inv.Name;
            ui.Q<Label>("ItemRarity").text = $"{inv.Type} | {inv.Rarity}";
            StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(inv.ImageUrl));

            var priceRow = ui.Q<VisualElement>("PriceRow");
            priceRow.Clear();

            var qtyLabel = new Label($"x{inv.Quantity}");
            qtyLabel.style.fontSize = 14;
            qtyLabel.style.color = Color.white;
            qtyLabel.style.marginRight = 10;
            priceRow.Add(qtyLabel);

            if (inv.IsEquipped)
            {
                var equipLabel = new Label("EQUIPPED");
                equipLabel.AddToClassList("badge");
                equipLabel.style.backgroundColor = new Color(0, 0.7f, 0);
                priceRow.Add(equipLabel);
            }

            root.RegisterCallback<ClickEvent>(e => {
                if (e.button == 1) ShowContextMenu(inv, e.position);
            });
            _invScroll.Add(ui);
        }
    }

    IEnumerator LoadInventory()
    {
        if (_invScroll == null) yield break;
        _invScroll.Clear();
        _invScroll.Add(new Label("Loading...") { style = { color = Color.gray, alignSelf = Align.Center, paddingTop = 20 } });

        yield return NetworkManager.Instance.SendRequest<List<InventoryDto>>("game/inventory", "GET", null,
            (items) => {
                _fullInventory = items;
                FilterInventory("All");
            },
            (err) => {
                if (_invScroll != null)
                {
                    _invScroll.Clear();
                    _invScroll.Add(new Label("Failed to load.") { style = { color = Color.red } });
                }
            }
        );
    }

    IEnumerator LoadProfile()
    {
        yield return NetworkManager.Instance.SendRequest<CharacterDto>("game/profile/me", "GET", null,
            (res) => {
                _currentProfile = res;
                if (_goldLabel != null) _goldLabel.text = $"{res.Gold:N0}";
                if (_gemLabel != null) _gemLabel.text = $"{res.Gem:N0}";
                if (_hpBar != null) { _hpBar.value = res.Health; _hpBar.highValue = res.MaxHealth; _hpBar.title = $"{res.Health}/{res.MaxHealth}"; }
                if (_staminaBar != null) _staminaBar.value = res.Hunger;
                if (_playerLevelLabel != null) _playerLevelLabel.text = $"{res.Level}";
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

        yield return NetworkManager.Instance.SendRequest<List<ShopItemDto>>($"game/shop?page={page}&pageSize={_pageSize}", "GET", null,
            (items) => {
                if (_shopScroll == null) return;
                _shopScroll.Clear();
                if (items.Count == 0 && page > 1) { _currentPage--; ChangePage(0); return; }
                if (_pageLabel != null) _pageLabel.text = $"{_currentPage}";

                int index = 0;
                foreach (var item in items)
                {
                    var card = CreateItemCard(item, index);
                    _shopScroll.Add(card);
                    index++;
                }
            },
            (err) => ToastManager.Instance.Show("Error loading Shop: " + err, false)
        );
    }

    VisualElement CreateItemCard(ShopItemDto item, int index)
    {
        var template = ItemTemplate.Instantiate();
        var root = template.Q<VisualElement>("ItemContainer");

        // CSS Zebra Striping
        if (index % 2 == 0) root.AddToClassList("row-even");
        else root.AddToClassList("row-odd");

        template.Q<Label>("ItemName").text = item.Name;
        template.Q<Label>("ItemRarity").text = $"{item.Type} | {item.Rarity}";

        StartCoroutine(template.Q<Image>("ItemImage").LoadImage(item.ImageURL));

        root.RegisterCallback<ClickEvent>(evt => ShowDetailPopup(item));

        var priceRow = template.Q<VisualElement>("PriceRow");
        priceRow.Clear();

        var btn = new Button();
        btn.AddToClassList("btn");
        btn.AddToClassList("btn-outline-secondary");
        btn.style.flexDirection = FlexDirection.Row;

        Color borderColor;
        string priceText;

        if (item.PriceCurrency == "RES_GOLD")
        {
            borderColor = new Color(1f, 0.75f, 0f, 0.3f);
            btn.style.color = new Color(1f, 0.75f, 0f);
            priceText = $"{item.PriceAmount:N0} G";
        }
        else
        {
            borderColor = new Color(0f, 0.82f, 1f, 0.3f);
            btn.style.color = new Color(0f, 0.82f, 1f);
            priceText = $"{item.PriceAmount:N0} 💎";
        }

        btn.style.borderTopColor = borderColor;
        btn.style.borderBottomColor = borderColor;
        btn.style.borderLeftColor = borderColor;
        btn.style.borderRightColor = borderColor;

        var lbl = new Label(priceText);
        lbl.AddToClassList("fw-bold");
        btn.Add(lbl);

        btn.clicked += () => ShowDetailPopup(item);
        priceRow.Add(btn);

        return template;
    }

    void ShowDetailPopup(ShopItemDto item)
    {
        if (PopupTemplate == null) return;
        var popup = PopupTemplate.Instantiate();
        var overlay = popup.Q<VisualElement>("DetailOverlay");
        if (overlay == null) return;

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
            if (lblTotal != null)
            {
                int total = item.PriceAmount * qty;
                lblTotal.text = $"Total: {total:N0} {(item.PriceCurrency == "RES_GOLD" ? "G" : "💎")}";
            }
        };

        var btnPlus = overlay.Q<Button>("BtnPlus");
        if (btnPlus != null) btnPlus.clicked += () => { qty++; UpdatePrice(); };

        var btnMinus = overlay.Q<Button>("BtnMinus");
        if (btnMinus != null) btnMinus.clicked += () => { if (qty > 1) qty--; UpdatePrice(); };

        var btnConfirm = overlay.Q<Button>("BtnConfirmBuy");
        if (btnConfirm != null)
        {
            btnConfirm.clicked += () => {
                StartCoroutine(BuyProcess(item.ProductID, qty));
                if (_root.Contains(overlay)) _root.Remove(overlay);
            };
        }

        var btnClose = overlay.Q<Button>("BtnCloseDetail");
        if (btnClose != null)
        {
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
                ToastManager.Instance.Show("Purchased successfully!", true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("success");
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    void ShowContextMenu(InventoryDto inv, Vector2 mousePos)
    {
        var old = _root.Q("ContextMenu");
        if (old != null) old.style.display = DisplayStyle.None;

        var menu = ContextMenuTemplate.Instantiate();
        var menuRoot = menu.Q<VisualElement>("ContextMenu");

        float x = mousePos.x;
        float y = mousePos.y;
        if (x + 150 > _root.resolvedStyle.width) x -= 150;
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

        menu.Q<Button>("BtnCtxCancel").clicked += () => _root.Remove(menuRoot);
        _root.Add(menuRoot);
    }

    IEnumerator SellItem(string itemId, int qty)
    {
        var body = new BuyRequest { ProductId = itemId, Quantity = qty };
        yield return NetworkManager.Instance.SendRequest<object>("game/sell", "POST", body,
            (res) => {
                ToastManager.Instance.Show("Sold successfully!", true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("coins");
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => ToastManager.Instance.Show("Error selling: " + err, false)
        );
    }

    IEnumerator UseItem(string itemId)
    {
        yield return NetworkManager.Instance.SendRequest<object>($"game/use-item/{itemId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Item used!", true); RefreshAllData(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator EquipItem(string itemId)
    {
        yield return NetworkManager.Instance.SendRequest<object>($"game/equip/{itemId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Equipped!", true); RefreshAllData(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator LoadTransactionHistory()
    {
        var panel = _root.Q<VisualElement>("NotiLogPanel");
        if (panel == null) yield break;
        panel.style.display = (panel.style.display == DisplayStyle.None) ? DisplayStyle.Flex : DisplayStyle.None;

        var list = panel.Q<ScrollView>("NotiLogList");
        list.Clear();
        list.Add(new Label("Loading...") { style = { color = Color.gray } });

        yield return NetworkManager.Instance.SendRequest<List<TransactionDto>>("game/transactions/my", "GET", null,
            (logs) => {
                list.Clear();
                if (logs.Count == 0) list.Add(new Label("No history.") { style = { color = Color.white } });

                foreach (var log in logs)
                {
                    string currencySymbol = (log.Currency == "RES_GEM") ? "💎" : "G";
                    var row = new Label($"[{log.Date}] {log.Action} ({log.Amount} {currencySymbol})");
                    row.style.color = log.Amount >= 0 ? Color.green : new Color(1f, 0.4f, 0.4f);
                    row.style.fontSize = 12;
                    row.style.borderBottomWidth = 1;
                    row.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
                    list.Add(row);
                }
            },
            (err) => { list.Clear(); list.Add(new Label("Error: " + err)); }
        );
    }

    IEnumerator LoadRecipes()
    {
        _craftScroll.Clear();
        _craftScroll.Add(new Label("Loading Recipes...") { style = { color = Color.gray } });

        yield return NetworkManager.Instance.SendRequest<List<RecipeDto>>("game/recipes", "GET", null, (recipes) => {
            _craftScroll.Clear();
            if (recipes.Count == 0) _craftScroll.Add(new Label("No Recipes Available.") { style = { color = Color.white } });

            int index = 0;
            foreach (var r in recipes)
            {
                var ui = ItemTemplate.Instantiate();
                var root = ui.Q<VisualElement>("ItemContainer");

                if (index % 2 == 0) root.AddToClassList("row-even");
                else root.AddToClassList("row-odd");
                index++;

                ui.Q<Label>("ItemName").text = r.ResultItemName;
                ui.Q<Label>("ItemRarity").text = $"Time: {r.CraftingTime}s";
                StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(r.ResultItemImage));

                var priceRow = ui.Q<VisualElement>("PriceRow");
                priceRow.Clear();

                var btn = new Button { text = "CRAFT" };
                btn.AddToClassList("btn-success");
                btn.style.height = 30;
                btn.clicked += () => StartCoroutine(CraftProcess(r));
                priceRow.Add(btn);

                _craftScroll.Add(ui);
            }
        }, null);
    }

    IEnumerator CraftProcess(RecipeDto r)
    {
        ToastManager.Instance.Show($"Crafting {r.ResultItemName}...", true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("craft");
        yield return new WaitForSeconds(r.CraftingTime);
        yield return NetworkManager.Instance.SendRequest<object>($"game/craft/{r.RecipeId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Crafting Complete!", true); GameEvents.TriggerCurrencyChanged(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator SpawnMonster()
    {
        _currentMonster = new MonsterDto { Name = "Zombie", HP = 100, MaxHp = 100 };
        _root.Q<Label>("MonsterName").text = _currentMonster.Name;
        _monsterHpBar.value = 100;
        yield break;
    }

    IEnumerator AttackProcess()
    {
        if (_currentMonster != null)
        {
            if (_monsterHpBar != null) _monsterHpBar.value -= 10;
            if (EffectsManager.Instance != null)
                EffectsManager.Instance.ShowDamage(_btnAttack.worldBound.center, 10, false);
        }

        yield return NetworkManager.Instance.SendRequest<HuntResponse>("game/hunt", "POST", null,
            (res) => {
                ToastManager.Instance.Show($"Hit! +{res.GoldEarned}G", true);
                if (res.LevelUp) ToastManager.Instance.Show("LEVEL UP!", true);
                GameEvents.TriggerCurrencyChanged();
            }, null
        );
    }
}