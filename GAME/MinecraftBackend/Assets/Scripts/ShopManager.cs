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
    private VisualElement _shopWrapper;
    private ScrollView _shopScroll, _invScroll, _craftScroll;
    private Label _goldLabel, _gemLabel, _playerLevelLabel;
    private ProgressBar _hpBar, _staminaBar;

    // UI Elements
    private Label _pageLabel; // Trang Shop
    private Label _invPageLabel; // Trang Inventory

    private Button _btnTabShop, _btnTabInv, _btnTabCraft, _btnTabBattle;
    private Button _btnFilterAll, _btnFilterWep, _btnFilterCon;

    // Logic Variables
    private int _currentPage = 1; // Shop Page (API side)
    private int _currentInvPage = 1; // Inv Page (Client side)

    private int _pageSize = 10;
    
    // --- AUTO-DETECT UPDATE ---
    // Không dùng const nữa. Mặc định là 100, nhưng sẽ tự cập nhật khi chạy.
    private float _itemHeight = 100f; 
    private bool _isHeightCalculated = false; // Cờ kiểm tra đã đo xong chưa

    private string _currentFilterType = "All"; // Lưu loại lọc hiện tại
    private List<InventoryDto> _fullInventory = new List<InventoryDto>(); 
    private List<InventoryDto> _filteredInventory = new List<InventoryDto>(); 

    private ProgressBar _monsterHpBar;
    private Button _btnAttack;
    private MonsterDto _currentMonster;
    private CharacterDto _currentProfile;

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

        // --- Containers & Scrolls ---
        _shopContainer = _root.Q<VisualElement>("ShopContainer");
        _inventoryContainer = _root.Q<VisualElement>("InventoryContainer");
        _craftContainer = _root.Q<VisualElement>("CraftContainer");
        _battleContainer = _root.Q<VisualElement>("BattleContainer");

        _shopScroll = _root.Q<ScrollView>("ShopScrollView");
        _invScroll = _root.Q<ScrollView>("InventoryScrollView");
        _craftScroll = _root.Q<ScrollView>("CraftScrollView");

        // --- Stats ---
        _goldLabel = _root.Q<Label>("ShopGold");
        _gemLabel = _root.Q<Label>("ShopGem");
        _hpBar = _root.Q<ProgressBar>("HpBar");
        _staminaBar = _root.Q<ProgressBar>("StaminaBar");
        _playerLevelLabel = _root.Q<Label>("LevelLabel");

        // --- Tabs ---
        _btnTabShop = SetupTabButton("TabShop", "Shop");
        _btnTabInv = SetupTabButton("TabInventory", "Inventory");
        _btnTabCraft = SetupTabButton("TabCraft", "Craft");
        _btnTabBattle = SetupTabButton("TabBattle", "Battle");

        // --- Pagination Controls ---
        var btnPrev = _root.Q<Button>("BtnPrev");
        var btnNext = _root.Q<Button>("BtnNext");
        _pageLabel = _root.Q<Label>("PageLabel");
        if (btnPrev != null) btnPrev.clicked += () => ChangePage(-1);
        if (btnNext != null) btnNext.clicked += () => ChangePage(1);

        var btnInvPrev = _root.Q<Button>("BtnInvPrev");
        var btnInvNext = _root.Q<Button>("BtnInvNext");
        _invPageLabel = _root.Q<Label>("InvPageLabel");
        if (btnInvPrev != null) btnInvPrev.clicked += () => ChangeInventoryPage(-1);
        if (btnInvNext != null) btnInvNext.clicked += () => ChangeInventoryPage(1);

        // --- Battle & Filters ---
        _monsterHpBar = _root.Q<ProgressBar>("MonsterHpBar");
        _btnAttack = _root.Q<Button>("BtnAttack");
        if (_btnAttack != null) _btnAttack.clicked += () => StartCoroutine(AttackProcess());

        _btnFilterAll = SetupInvFilter("BtnFilterAll", "All");
        _btnFilterWep = SetupInvFilter("BtnFilterWep", "Weapon");
        _btnFilterCon = SetupInvFilter("BtnFilterCon", "Consumable");

        var btnLogs = _root.Q<Button>("BtnNotiLog");
        if (btnLogs != null) btnLogs.clicked += () => StartCoroutine(LoadTransactionHistory());

        // --- Events ---
        GameEvents.OnCurrencyChanged += RefreshAllData;
        GameEvents.OnEquipRequest += HandleEquipRequest;

        // --- AUTO-DETECT LOGIC ---
        // Thay vì SwitchTab ngay, ta đo chiều cao trước
        StartCoroutine(InitializeLayoutAndLoad());
    }

    void OnDisable()
    {
        GameEvents.OnCurrencyChanged -= RefreshAllData;
        GameEvents.OnEquipRequest -= HandleEquipRequest;
        if (_shopWrapper != null)
            _shopWrapper.UnregisterCallback<GeometryChangedEvent>(OnShopWrapperLayoutChange);
    }

    // --- COROUTINE KHỞI TẠO THÔNG MINH ---
    IEnumerator InitializeLayoutAndLoad()
    {
        // 1. Tạo một item giả để đo
        if (ItemTemplate != null && _shopScroll != null)
        {
            var ghostItem = ItemTemplate.Instantiate();
            var ghostRoot = ghostItem.Q<VisualElement>("ItemContainer");
            
            // Làm cho nó tàng hình và không ảnh hưởng layout chính
            ghostRoot.style.visibility = Visibility.Hidden; 
            ghostRoot.style.position = Position.Absolute;
            
            _shopScroll.Add(ghostRoot);

            // 2. Đợi đến cuối frame để Unity tính toán layout xong
            yield return new WaitForEndOfFrame(); 

            // 3. Lấy chiều cao thực tế
            if (ghostRoot.layout.height > 0)
            {
                _itemHeight = ghostRoot.layout.height;
                Debug.Log($"[ShopManager] Auto-detected Item Height: {_itemHeight}px");
            }
            else
            {
                Debug.LogWarning("[ShopManager] Could not detect height, using default 100px");
                _itemHeight = 100f; // Fallback nếu lỗi
            }

            // 4. Xóa item giả
            _shopScroll.Remove(ghostRoot);
            _isHeightCalculated = true;
        }

        // --- Sau khi đo xong mới gán sự kiện Resize ---
        if (_shopContainer != null)
        {
            _shopWrapper = _shopContainer.Q(className: "list-wrapper");
            if (_shopWrapper != null)
            {
                // Gọi thủ công lần đầu để tính _pageSize ngay lập tức
                CalculatePageSize(_shopWrapper.resolvedStyle.height);
                // Sau đó mới đăng ký sự kiện cho các lần resize sau
                _shopWrapper.RegisterCallback<GeometryChangedEvent>(OnShopWrapperLayoutChange);
            }
        }

        // Load dữ liệu
        StartCoroutine(LoadProfile());
        SwitchTab("Shop");
    }

    private void OnShopWrapperLayoutChange(GeometryChangedEvent evt)
    {
        CalculatePageSize(evt.newRect.height);
    }

    // Hàm tính toán số lượng item hiển thị
    void CalculatePageSize(float wrapperHeight)
    {
        if (!_isHeightCalculated) return; // Chưa đo xong thì chưa tính
        if (wrapperHeight < _itemHeight) return;

        // Tính số lượng item vừa khít
        int fitCount = Mathf.FloorToInt(wrapperHeight / _itemHeight);
        if (fitCount < 1) fitCount = 1;

        // Chỉ reload khi số lượng thay đổi
        if (fitCount != _pageSize)
        {
            _pageSize = fitCount;
            Debug.Log($"[ShopManager] Page Size updated to: {_pageSize} items");
            
            if (_shopContainer.style.display == DisplayStyle.Flex) StartCoroutine(LoadShopItems(_currentPage));
            if (_inventoryContainer.style.display == DisplayStyle.Flex) RenderInventoryCurrentPage();
        }
    }

    // --- CÁC HÀM LOGIC CŨ GIỮ NGUYÊN ---

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
            _invPageLabel.text = "1";
            return;
        }

        int maxPage = Mathf.CeilToInt((float)_filteredInventory.Count / _pageSize);
        _invPageLabel.text = $"{_currentInvPage}/{maxPage}";

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
            equipLabel.style.paddingLeft = 5;
            equipLabel.style.paddingRight = 5;
            priceRow.Add(equipLabel);
        }

        root.RegisterCallback<ClickEvent>(e => {
            if (e.button == 1) ShowContextMenu(inv, e.position);
        });

        return ui;
    }

    void FilterInventory(string type)
    {
        _currentFilterType = type;
        UpdateFilterVisual(type);
        _filteredInventory = (type == "All") ? _fullInventory : _fullInventory.Where(i => i.Type == type).ToList();
        _currentInvPage = 1;
        RenderInventoryCurrentPage();
    }

    IEnumerator LoadInventory()
    {
        if (_invScroll == null) yield break;
        _invScroll.Clear();
        _invScroll.Add(new Label("Loading...") { style = { color = Color.gray, alignSelf = Align.Center, paddingTop = 20, fontSize = 20 } });

        yield return NetworkManager.Instance.SendRequest<List<InventoryDto>>("game/inventory", "GET", null,
            (items) => {
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

    public void UseItemFromHotbar(string itemId)
    {
        var item = _fullInventory.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            if (item.Type == "Consumable") StartCoroutine(UseItem(itemId));
            else StartCoroutine(EquipItem(itemId));
        }
        else ToastManager.Instance.Show("Item không tồn tại!", false);
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
            // Chỉ load nếu đã tính được kích thước trang
            if (_pageSize > 0 && _isHeightCalculated) StartCoroutine(LoadShopItems(_currentPage));
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
                _pageLabel.text = $"{_currentPage}";
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
        root.RegisterCallback<ClickEvent>(evt => ShowDetailPopup(item));

        var priceRow = template.Q<VisualElement>("PriceRow");
        priceRow.Clear();
        
        // --- TẠO NÚT MUA TO HƠN ---
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
        
        // KÍCH THƯỚC MỚI: Cao 55px (cũ 40)
        btn.style.height = 55; 

        var lbl = new Label(priceText);
        lbl.AddToClassList("fw-bold");
        // FONT GIÁ MỚI: 22px (cũ 16)
        lbl.style.fontSize = 22; 
        
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
        if (x + 180 > _root.resolvedStyle.width) x -= 180;
        if (y + 180 > _root.resolvedStyle.height) y -= 180;
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
        list.Add(new Label("Loading...") { style = { color = Color.gray, fontSize = 18 } });
        yield return NetworkManager.Instance.SendRequest<List<TransactionDto>>("game/transactions/my", "GET", null,
            (logs) => {
                list.Clear();
                if (logs.Count == 0) list.Add(new Label("No history.") { style = { color = Color.white, fontSize = 18 } });
                foreach (var log in logs)
                {
                    string currencySymbol = (log.Currency == "RES_GEM") ? "💎" : "G";
                    var row = new Label($"[{log.Date}] {log.Action} ({log.Amount} {currencySymbol})");
                    row.style.color = log.Amount >= 0 ? Color.green : new Color(1f, 0.4f, 0.4f);
                    row.style.fontSize = 18;
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
        _craftScroll.Add(new Label("Loading Recipes...") { style = { color = Color.gray, fontSize = 24 } }); // Font loading to hơn
        
        yield return NetworkManager.Instance.SendRequest<List<RecipeDto>>("game/recipes", "GET", null, (recipes) => {
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
                
                // --- TẠO NÚT CRAFT TO HƠN ---
                var btn = new Button { text = "CRAFT" };
                btn.AddToClassList("btn-success");
                btn.AddToClassList("btn");
                
                // KÍCH THƯỚC MỚI: Cao 55px, Font 20px (tự nhận từ class btn)
                btn.style.height = 55;
                btn.style.fontSize = 20;
                btn.style.width = 120; // Rộng hơn chút cho đẹp

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