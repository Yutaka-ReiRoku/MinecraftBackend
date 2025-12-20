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
    public VisualTreeAsset ItemTemplate;      // Dùng cho Shop/Inventory/Craft
    public VisualTreeAsset PopupTemplate;     // Popup mua hàng
    public VisualTreeAsset ContextMenuTemplate; // Menu chuột phải

    [Header("Effects")]
    public GameObject ConfettiPrefab;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // --- CONTAINERS ---
    private VisualElement _shopContainer;
    private VisualElement _inventoryContainer;
    private VisualElement _craftContainer;
    private VisualElement _battleContainer;
    private VisualElement _historyContainer;
    private VisualElement _shopWrapper;

    // --- SCROLL VIEWS ---
    private ScrollView _shopScroll;
    private ScrollView _invScroll;
    private ScrollView _craftScroll;
    private ScrollView _historyScroll;

    // --- HEADER STATS ---
    private Label _goldLabel;
    private Label _gemLabel;
    private Label _playerLevelLabel;
    private ProgressBar _hpBar;
    private ProgressBar _staminaBar;

    // --- TABS ---
    private Button _btnTabShop;
    private Button _btnTabInv;
    private Button _btnTabCraft;
    private Button _btnTabBattle;
    private Button _btnTabHistory;

    // --- FILTERS ---
    private Button _btnFilterAll, _btnFilterWep, _btnFilterCon;

    // --- PAGINATION ---
    private Label _pageLabel;       // Shop Page
    private Label _invPageLabel;    // Inv Page
    private Label _histPageLabel;   // History Page

    // --- LOGIC VARIABLES ---
    private int _currentPage = 1;
    private int _currentInvPage = 1;
    private int _currentHistPage = 1;
    private int _pageSize = 10;
    private float _itemHeight = 100f;
    private bool _isHeightCalculated = false;

    // --- ANTI-SPAM ---
    private bool _isBusy = false; 
    private float _lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.3f;

    // --- DATA CACHE ---
    private string _currentFilterType = "All";
    private List<InventoryDto> _fullInventory = new List<InventoryDto>();
    private List<InventoryDto> _filteredInventory = new List<InventoryDto>();
    private MonsterDto _currentMonster;
    private CharacterDto _currentProfile;

    // --- BATTLE UI ---
    private ProgressBar _monsterHpBar;
    private Button _btnAttack;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null)
        {
            Debug.LogError("[ShopManager] Missing UIDocument!");
            return;
        }
        _root = _uiDoc.rootVisualElement;

        // 1. SETUP CONTAINERS
        _shopContainer = _root.Q<VisualElement>("ShopContainer");
        _inventoryContainer = _root.Q<VisualElement>("InventoryContainer");
        _craftContainer = _root.Q<VisualElement>("CraftContainer");
        _battleContainer = _root.Q<VisualElement>("BattleContainer");
        _historyContainer = _root.Q<VisualElement>("HistoryContainer");

        // 2. SETUP SCROLL VIEWS
        _shopScroll = _root.Q<ScrollView>("ShopScrollView");
        _invScroll = _root.Q<ScrollView>("InventoryScrollView");
        _craftScroll = _root.Q<ScrollView>("CraftScrollView");
        _historyScroll = _root.Q<ScrollView>("HistoryScrollView");

        // 3. SETUP HEADER STATS
        _goldLabel = _root.Q<Label>("ShopGold");
        _gemLabel = _root.Q<Label>("ShopGem");
        _hpBar = _root.Q<ProgressBar>("HpBar");
        _staminaBar = _root.Q<ProgressBar>("StaminaBar");
        _playerLevelLabel = _root.Q<Label>("LevelLabel");

        // 4. SETUP SETTINGS BUTTON
        var btnSettings = _root.Q<Button>("BtnSettings");
        if (btnSettings != null)
        {
            btnSettings.clicked -= OnSettingsClicked;
            btnSettings.clicked += OnSettingsClicked;
        }

        // 5. SETUP TABS
        _btnTabShop = SetupTabButton("TabShop", "Shop");
        _btnTabInv = SetupTabButton("TabInventory", "Inventory");
        _btnTabCraft = SetupTabButton("TabCraft", "Craft");
        _btnTabBattle = SetupTabButton("TabBattle", "Battle");
        _btnTabHistory = SetupTabButton("TabHistory", "History");

        // 6. PAGINATION
        var btnPrev = _root.Q<Button>("BtnPrev");
        var btnNext = _root.Q<Button>("BtnNext");
        _pageLabel = _root.Q<Label>("PageLabel");
        if (btnPrev != null) btnPrev.clicked += () => { if (CanClick()) ChangePage(-1); };
        if (btnNext != null) btnNext.clicked += () => { if (CanClick()) ChangePage(1); };

        var btnInvPrev = _root.Q<Button>("BtnInvPrev");
        var btnInvNext = _root.Q<Button>("BtnInvNext");
        _invPageLabel = _root.Q<Label>("InvPageLabel");
        if (btnInvPrev != null) btnInvPrev.clicked += () => { if (CanClick()) ChangeInventoryPage(-1); };
        if (btnInvNext != null) btnInvNext.clicked += () => { if (CanClick()) ChangeInventoryPage(1); };

        var btnHistPrev = _root.Q<Button>("BtnHistPrev");
        var btnHistNext = _root.Q<Button>("BtnHistNext");
        _histPageLabel = _root.Q<Label>("HistPageLabel");
        if (btnHistPrev != null) btnHistPrev.clicked += () => { if (CanClick()) ChangeHistoryPage(-1); };
        if (btnHistNext != null) btnHistNext.clicked += () => { if (CanClick()) ChangeHistoryPage(1); };

        // 7. BATTLE
        _monsterHpBar = _root.Q<ProgressBar>("MonsterHpBar");
        _btnAttack = _root.Q<Button>("BtnAttack");
        if (_btnAttack != null) _btnAttack.clicked += () => {
            if (CanClick()) StartCoroutine(AttackProcess());
        };

        // 8. FILTERS
        _btnFilterAll = SetupInvFilter("BtnFilterAll", "All");
        _btnFilterWep = SetupInvFilter("BtnFilterWep", "Weapon");
        _btnFilterCon = SetupInvFilter("BtnFilterCon", "Consumable");

        var btnLogs = _root.Q<Button>("BtnNotiLog");
        if (btnLogs != null) btnLogs.clicked += () => { if (CanClick()) SwitchTab("History"); };

        // 9. EVENTS
        GameEvents.OnCurrencyChanged += RefreshAllData;
        GameEvents.OnEquipRequest += HandleEquipRequest;

        _isBusy = false;
        StartCoroutine(InitializeLayoutAndLoad());
    }

    void OnDisable()
    {
        GameEvents.OnCurrencyChanged -= RefreshAllData;
        GameEvents.OnEquipRequest -= HandleEquipRequest;
        if (_shopWrapper != null) _shopWrapper.UnregisterCallback<GeometryChangedEvent>(OnShopWrapperLayoutChange);
    }

    private void OnSettingsClicked()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleSettings();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
        }
    }

    private bool CanClick()
    {
        if (_isBusy) return false;
        if (SettingsManager.Instance != null && SettingsManager.Instance.IsSettingsOpen) return false;
        if (Time.time - _lastClickTime < CLICK_COOLDOWN) return false;

        _lastClickTime = Time.time;
        return true;
    }

    void SwitchTab(string tabName)
    {
        if (_shopContainer != null) _shopContainer.style.display = DisplayStyle.None;
        if (_inventoryContainer != null) _inventoryContainer.style.display = DisplayStyle.None;
        if (_craftContainer != null) _craftContainer.style.display = DisplayStyle.None;
        if (_battleContainer != null) _battleContainer.style.display = DisplayStyle.None;
        if (_historyContainer != null) _historyContainer.style.display = DisplayStyle.None;

        SetTabActive(_btnTabShop, tabName == "Shop");
        SetTabActive(_btnTabInv, tabName == "Inventory");
        SetTabActive(_btnTabCraft, tabName == "Craft");
        SetTabActive(_btnTabBattle, tabName == "Battle");
        SetTabActive(_btnTabHistory, tabName == "History");

        if (tabName == "Shop")
        {
            if (_shopContainer != null) _shopContainer.style.display = DisplayStyle.Flex;
            if (_pageSize > 0 && _isHeightCalculated) StartCoroutine(LoadShopItems(_currentPage));
        }
        else if (tabName == "Inventory")
        {
            if (_inventoryContainer != null) _inventoryContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadInventory());
        }
        else if (tabName == "Craft")
        {
            if (_craftContainer != null) _craftContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadRecipes());
        }
        else if (tabName == "Battle")
        {
            if (_battleContainer != null) _battleContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(SpawnMonster());
        }
        else if (tabName == "History")
        {
            if (_historyContainer != null) _historyContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadHistoryLogs(_currentHistPage));
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    void SetTabActive(Button btn, bool isActive)
    {
        if (btn == null) return;
        btn.RemoveFromClassList("active");
        if (isActive) btn.AddToClassList("active");
    }

    IEnumerator InitializeLayoutAndLoad()
    {
        if (ItemTemplate != null && _shopScroll != null)
        {
            var ghostItem = ItemTemplate.Instantiate();
            var ghostRoot = ghostItem.Q<VisualElement>("ItemContainer");
            ghostRoot.style.visibility = Visibility.Hidden;
            ghostRoot.style.position = Position.Absolute;
            _shopScroll.Add(ghostRoot);
            yield return new WaitForEndOfFrame();

            if (ghostRoot.layout.height > 0) _itemHeight = ghostRoot.layout.height;
            else _itemHeight = 100f;
            _shopScroll.Remove(ghostRoot);
            _isHeightCalculated = true;
        }

        if (_shopContainer != null)
        {
            _shopWrapper = _shopContainer.Q(className: "list-wrapper");
            if (_shopWrapper != null)
            {
                CalculatePageSize(_shopWrapper.resolvedStyle.height);
                _shopWrapper.RegisterCallback<GeometryChangedEvent>(OnShopWrapperLayoutChange);
            }
        }

        StartCoroutine(LoadProfile());
        SwitchTab("Shop");
    }

    private void OnShopWrapperLayoutChange(GeometryChangedEvent evt)
    {
        CalculatePageSize(evt.newRect.height);
    }

    void CalculatePageSize(float wrapperHeight)
    {
        if (!_isHeightCalculated) return;
        if (wrapperHeight < _itemHeight) return;

        int fitCount = Mathf.FloorToInt(wrapperHeight / _itemHeight);
        if (fitCount < 1) fitCount = 1;

        if (fitCount != _pageSize)
        {
            _pageSize = fitCount;
            if (_shopContainer.style.display == DisplayStyle.Flex) StartCoroutine(LoadShopItems(_currentPage));
            if (_inventoryContainer.style.display == DisplayStyle.Flex) RenderInventoryCurrentPage();
        }
    }

    void RefreshAllData()
    {
        StartCoroutine(LoadProfile());
        if (_inventoryContainer != null && _inventoryContainer.style.display == DisplayStyle.Flex) StartCoroutine(LoadInventory());
    }

    IEnumerator LoadProfile()
    {
        yield return NetworkManager.Instance.SendRequest<CharacterDto>("game/profile/me", "GET", null,
            (res) => {
                _currentProfile = res;
                if (_goldLabel != null) _goldLabel.text = $"{res.Gold:N0}";
                if (_gemLabel != null) _gemLabel.text = $"{res.Gem:N0}";
                if (_hpBar != null)
                {
                    _hpBar.value = res.Health;
                    _hpBar.highValue = res.MaxHealth;
                    _hpBar.title = $"{res.Health}/{res.MaxHealth}";
                }
                if (_staminaBar != null) _staminaBar.value = res.Hunger;
                if (_playerLevelLabel != null) _playerLevelLabel.text = $"{res.Level}";
            },
            null
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
                
                if (items.Count == 0 && page > 1)
                {
                    _currentPage--;
                    ChangePage(0);
                    return;
                }

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

        if (index % 2 == 0) root.AddToClassList("row-even");
        else root.AddToClassList("row-odd");

        template.Q<Label>("ItemName").text = item.Name;
        template.Q<Label>("ItemRarity").text = $"{item.Type} | {item.Rarity}";
        StartCoroutine(template.Q<Image>("ItemImage").LoadImage(item.ImageURL));

        root.RegisterCallback<ClickEvent>(evt => {
            if (CanClick()) ShowDetailPopup(item);
        });

        var priceRow = template.Q<VisualElement>("PriceRow");
        priceRow.Clear();

        var btn = new Button();
        btn.AddToClassList("btn");
        btn.AddToClassList("btn-outline-secondary");
        btn.style.flexDirection = FlexDirection.Row;

        string priceText;
        Color color;
        if (item.PriceCurrency == "RES_GOLD")
        {
            priceText = $"{item.PriceAmount:N0} G";
            color = new Color(1f, 0.75f, 0f);
        }
        else
        {
            priceText = $"{item.PriceAmount:N0} 💎";
            color = new Color(0f, 0.82f, 1f);
        }
        
        btn.style.color = color;
        
        // FIX ERROR CS1061: Viết rõ từng cạnh thay vì shorthand 'borderColor'
        Color borderCol = new Color(color.r, color.g, color.b, 0.3f);
        btn.style.borderTopColor = borderCol;
        btn.style.borderBottomColor = borderCol;
        btn.style.borderLeftColor = borderCol;
        btn.style.borderRightColor = borderCol;

        btn.style.height = 55;

        var lbl = new Label(priceText);
        lbl.AddToClassList("fw-bold");
        lbl.style.fontSize = 22;
        btn.Add(lbl);

        btn.clicked += () => { if (CanClick()) ShowDetailPopup(item); };
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
                string curr = item.PriceCurrency == "RES_GOLD" ? "G" : "💎";
                lblTotal.text = $"Total: {total:N0} {curr}";
            }
        };

        var btnPlus = overlay.Q<Button>("BtnPlus");
        if (btnPlus != null) btnPlus.clicked += () => { qty++; UpdatePrice(); };

        var btnMinus = overlay.Q<Button>("BtnMinus");
        if (btnMinus != null) btnMinus.clicked += () => { if (qty > 1) qty--; UpdatePrice(); };

        var btnConfirm = overlay.Q<Button>("BtnConfirmBuy");
        if (btnConfirm != null) {
            btnConfirm.clicked += () => {
                if (CanClick())
                {
                    StartCoroutine(BuyProcess(item.ProductID, qty));
                    if (_root.Contains(overlay)) _root.Remove(overlay);
                }
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
        _isBusy = true;
        var body = new BuyRequest { ProductId = prodId, Quantity = qty };
        yield return NetworkManager.Instance.SendRequest<object>("game/buy", "POST", body,
            (res) => {
                _isBusy = false; 
                ToastManager.Instance.Show("Purchased successfully!", true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("success");
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => {
                _isBusy = false; 
                ToastManager.Instance.Show(err, false);
            }
        );
    }

    void ChangeInventoryPage(int dir)
    {
        if (_filteredInventory.Count == 0) return;
        int maxPage = Mathf.CeilToInt((float)_filteredInventory.Count / _pageSize);
        if (maxPage < 1) maxPage = 1;

        _currentInvPage += dir;
        if (_currentInvPage < 1) _currentInvPage = 1;
        if (_currentInvPage > maxPage) _currentInvPage = maxPage;

        RenderInventoryCurrentPage();
    }

    void RenderInventoryCurrentPage()
    {
        if (_invScroll == null) return;
        _invScroll.Clear();

        if (_filteredInventory.Count == 0)
        {
            _invScroll.Add(new Label("Empty.") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 20, fontSize = 20 } });
            if (_invPageLabel != null) _invPageLabel.text = "1";
            return;
        }

        int maxPage = Mathf.CeilToInt((float)_filteredInventory.Count / _pageSize);
        if (_invPageLabel != null) _invPageLabel.text = $"{_currentInvPage}/{maxPage}";

        var pageItems = _filteredInventory
            .Skip((_currentInvPage - 1) * _pageSize)
            .Take(_pageSize)
            .ToList();

        int index = 0;
        foreach (var inv in pageItems)
        {
            var ui = CreateInventoryItem(inv, index);
            _invScroll.Add(ui);
            index++;
        }
    }

    VisualElement CreateInventoryItem(InventoryDto inv, int index)
    {
        var ui = ItemTemplate.Instantiate();
        var root = ui.Q<VisualElement>("ItemContainer");

        if (index % 2 == 0) root.AddToClassList("row-even");
        else root.AddToClassList("row-odd");

        ui.Q<Label>("ItemName").text = inv.Name;
        ui.Q<Label>("ItemRarity").text = $"{inv.Type} | {inv.Rarity}";
        StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(inv.ImageUrl));

        var priceRow = ui.Q<VisualElement>("PriceRow");
        priceRow.Clear();

        var qtyLabel = new Label($"x{inv.Quantity}");
        qtyLabel.style.fontSize = 20;
        qtyLabel.style.color = Color.white;
        qtyLabel.style.marginRight = 10;
        priceRow.Add(qtyLabel);

        if (inv.IsEquipped)
        {
            var equipLabel = new Label("EQUIPPED");
            equipLabel.AddToClassList("badge");
            equipLabel.style.backgroundColor = new Color(0, 0.7f, 0);
            equipLabel.style.fontSize = 14;
            equipLabel.style.paddingLeft = 5; equipLabel.style.paddingRight = 5;
            priceRow.Add(equipLabel);
        }

        root.RegisterCallback<ClickEvent>(e =>
        {
            if (e.button == 1) ShowContextMenu(inv, e.position);
        });

        return ui;
    }

    public void UseItemFromHotbar(string itemId)
    {
        var item = _fullInventory.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            if (!CanClick()) return;
            if (item.Type == "Consumable") StartCoroutine(UseItem(itemId));
            else StartCoroutine(EquipItem(itemId));
        }
    }

    void FilterInventory(string type)
    {
        _currentFilterType = type;
        UpdateFilterVisual(type);

        _filteredInventory = (type == "All") ? 
            _fullInventory : 
            _fullInventory.Where(i => i.Type == type).ToList();

        _currentInvPage = 1;
        RenderInventoryCurrentPage();
    }

    IEnumerator LoadInventory()
    {
        if (_invScroll == null) yield break;
        _invScroll.Clear();
        _invScroll.Add(new Label("Loading...") { style = { color = Color.gray, alignSelf = Align.Center, paddingTop = 20, fontSize = 20 } });

        yield return NetworkManager.Instance.SendRequest<List<InventoryDto>>("game/inventory", "GET", null,
            (items) =>
            {
                _fullInventory = items;
                FilterInventory(_currentFilterType);
            },
            (err) => {
                if (_invScroll != null)
                {
                    _invScroll.Clear();
                    _invScroll.Add(new Label("Failed to load.") { style = { color = Color.red, fontSize = 20 } });
                }
            }
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
        if (x + 180 > _root.resolvedStyle.width) x -= 180;
        if (y + 180 > _root.resolvedStyle.height) y -= 180;

        menuRoot.style.left = x;
        menuRoot.style.top = y;
        menuRoot.style.display = DisplayStyle.Flex;

        menu.Q<Button>("BtnCtxUse").clicked += () => {
            if (CanClick())
            {
                if (inv.Type == "Consumable") StartCoroutine(UseItem(inv.ItemId));
                else StartCoroutine(EquipItem(inv.ItemId));
                _root.Remove(menuRoot);
            }
        };
        menu.Q<Button>("BtnCtxSell").clicked += () => {
            if (CanClick())
            {
                StartCoroutine(SellItem(inv.ItemId, 1));
                _root.Remove(menuRoot);
            }
        };
        menu.Q<Button>("BtnCtxCancel").clicked += () => _root.Remove(menuRoot);

        _root.Add(menuRoot);
    }

    IEnumerator SellItem(string itemId, int qty)
    {
        _isBusy = true;
        var body = new BuyRequest { ProductId = itemId, Quantity = qty };
        yield return NetworkManager.Instance.SendRequest<object>("game/sell", "POST", body,
            (res) => {
                _isBusy = false;
                ToastManager.Instance.Show("Sold successfully!", true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("coins");
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => {
                _isBusy = false;
                ToastManager.Instance.Show("Error selling: " + err, false);
            }
        );
    }

    IEnumerator UseItem(string itemId)
    {
        _isBusy = true;
        yield return NetworkManager.Instance.SendRequest<object>($"game/use-item/{itemId}", "POST", null,
            (res) => {
                _isBusy = false;
                ToastManager.Instance.Show("Item used!", true);
                RefreshAllData();
            },
            (err) => {
                _isBusy = false;
                ToastManager.Instance.Show(err, false);
            }
        );
    }

    IEnumerator EquipItem(string itemId)
    {
        _isBusy = true;
        yield return NetworkManager.Instance.SendRequest<object>($"game/equip/{itemId}", "POST", null,
            (res) => {
                _isBusy = false;
                ToastManager.Instance.Show("Equipped!", true);
                RefreshAllData();
            },
            (err) => {
                _isBusy = false;
                ToastManager.Instance.Show(err, false);
            }
        );
    }

    void HandleEquipRequest(string itemId)
    {
        if (!CanClick()) return;
        StartCoroutine(EquipItem(itemId));
    }

    IEnumerator LoadRecipes()
    {
        _craftScroll.Clear();
        _craftScroll.Add(new Label("Loading Recipes...") { style = { color = Color.gray, fontSize = 24 } });

        yield return NetworkManager.Instance.SendRequest<List<RecipeDto>>("game/recipes", "GET", null,
            (recipes) => {
                _craftScroll.Clear();
                if (recipes.Count == 0) _craftScroll.Add(new Label("No Recipes Available.") { style = { color = Color.white, fontSize = 24 } });
                
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
                    btn.AddToClassList("btn");
                    btn.style.height = 55;
                    btn.style.fontSize = 20;
                    btn.style.width = 120;
                    btn.clicked += () => { if (CanClick()) StartCoroutine(CraftProcess(r)); };
                    
                    priceRow.Add(btn);
                    _craftScroll.Add(ui);
                }
            },
            null);
    }

    IEnumerator CraftProcess(RecipeDto r)
    {
        _isBusy = true;
        ToastManager.Instance.Show($"Crafting {r.ResultItemName}...", true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("craft");
        
        yield return new WaitForSeconds(r.CraftingTime);
        
        yield return NetworkManager.Instance.SendRequest<object>($"game/craft/{r.RecipeId}", "POST", null,
            (res) => {
                _isBusy = false;
                ToastManager.Instance.Show("Crafting Complete!", true);
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => {
                _isBusy = false;
                ToastManager.Instance.Show(err, false);
            }
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
        _isBusy = true;
        if (_currentMonster != null)
        {
            if (_monsterHpBar != null) _monsterHpBar.value -= 10;
            if (EffectsManager.Instance != null) EffectsManager.Instance.ShowDamage(_btnAttack.worldBound.center, 10, false);
        }

        yield return NetworkManager.Instance.SendRequest<HuntResponse>("game/hunt", "POST", null,
            (res) => {
                _isBusy = false;
                ToastManager.Instance.Show($"Hit! +{res.GoldEarned}G", true);
                if (res.LevelUp) ToastManager.Instance.Show("LEVEL UP!", true);
                GameEvents.TriggerCurrencyChanged();
            },
            (err) => _isBusy = false
        );
    }

    Button SetupTabButton(string btnName, string tabName)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null)
        {
            btn.clicked -= () => SwitchTab(tabName);
            btn.clicked += () => { if (CanClick()) SwitchTab(tabName); };
        }
        return btn;
    }

    Button SetupInvFilter(string btnName, string type)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null) btn.clicked += () => { if (CanClick()) FilterInventory(type); };
        return btn;
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

    // --- HISTORY / TRANSACTION LOGS LOGIC ---

    void ChangeHistoryPage(int dir)
    {
        _currentHistPage += dir;
        if (_currentHistPage < 1) _currentHistPage = 1;
        StartCoroutine(LoadHistoryLogs(_currentHistPage));
    }

    IEnumerator LoadHistoryLogs(int page)
    {
        if (_historyScroll == null) yield break;
        _historyScroll.Clear();
        _historyScroll.Add(new Label("Loading history...") { style = { color = Color.gray, alignSelf = Align.Center, paddingTop = 20, fontSize = 20 } });

        string url = $"game/transactions/my?page={page}&pageSize={_pageSize}";
        
        yield return NetworkManager.Instance.SendRequest<List<TransactionDto>>(url, "GET", null,
            (logs) => {
                if (_historyScroll == null) return;
                _historyScroll.Clear();

                if (logs.Count == 0)
                {
                    if (page > 1) { _currentHistPage--; ChangeHistoryPage(0); return; }
                    _historyScroll.Add(new Label("No activity recorded.") { style = { color = Color.white, alignSelf = Align.Center, fontSize = 20, paddingTop = 20 } });
                    return;
                }

                if (_histPageLabel != null) _histPageLabel.text = $"{_currentHistPage}";

                int index = 0;
                foreach (var log in logs)
                {
                    var row = CreateHistoryRow(log, index);
                    _historyScroll.Add(row);
                    index++;
                }
            },
            (err) => {
                if (_historyScroll != null)
                {
                    _historyScroll.Clear();
                    _historyScroll.Add(new Label("Error loading history: " + err) { style = { color = Color.red, alignSelf = Align.Center } });
                }
            }
        );
    }

    VisualElement CreateHistoryRow(TransactionDto log, int index)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.height = 65;
        row.style.alignItems = Align.Center;
        row.style.paddingLeft = 20; row.style.paddingRight = 20;
        
        // Fix Error: Sử dụng từng cạnh thay vì shorthand borderColor
        row.style.borderBottomWidth = 1;
        row.style.borderBottomColor = new Color(1, 1, 1, 0.05f);
        
        if (index % 2 == 0) row.style.backgroundColor = new Color(0, 0, 0, 0.2f);
        else row.style.backgroundColor = new Color(0, 0, 0, 0.1f);

        var leftCol = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.Center } };
        var lblAction = new Label(log.Action) { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold } };
        var lblDate = new Label(log.Date) { style = { color = new Color(0.7f, 0.7f, 0.7f), fontSize = 14, marginTop = 2 } };
        
        leftCol.Add(lblAction);
        leftCol.Add(lblDate);

        string currencySymbol = (log.Currency == "RES_GEM") ? "💎" : "G";
        string sign = log.Amount >= 0 ? "+" : "";
        Color amountColor = log.Amount >= 0 ? new Color(0.2f, 1f, 0.2f) : new Color(1f, 0.4f, 0.4f);

        var lblAmount = new Label($"{sign}{log.Amount:N0} {currencySymbol}");
        lblAmount.style.fontSize = 20;
        lblAmount.style.unityFontStyleAndWeight = FontStyle.Bold;
        lblAmount.style.color = amountColor;
        lblAmount.style.minWidth = 120;
        lblAmount.style.unityTextAlign = TextAnchor.MiddleRight;

        row.Add(leftCol);
        row.Add(lblAmount);

        return row;
    }
}