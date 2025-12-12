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

    // --- UI ELEMENTS ---
    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _shopContainer, _inventoryContainer, _craftContainer, _battleContainer, _baseContainer;
    private ScrollView _shopScroll, _invScroll, _craftScroll;

    private Label _goldLabel, _gemLabel, _playerLevelLabel;
    private ProgressBar _hpBar, _staminaBar, _expBar;

    private Label _pageLabel;
    private int _currentPage = 1;
    private int _pageSize = 8;

    private ProgressBar _monsterHpBar;
    private Button _btnAttack;
    private MonsterDto _currentMonster;

    // --- DATA CACHE ---
    private CharacterDto _currentProfile;
    private List<InventoryDto> _fullInventory = new List<InventoryDto>();

    // --- STATE MANAGEMENT ---
    private Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
    private Dictionary<string, Button> _filterButtons = new Dictionary<string, Button>();
    private string _currentFilter = "All";

    // --- INITIALIZATION ---
    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null)
        {
            Debug.LogError("[ShopManager] Missing UIDocument!");
            return;
        }
        _root = _uiDoc.rootVisualElement;

        // 1. Containers
        _shopContainer = _root.Q<VisualElement>("ShopContainer");
        _inventoryContainer = _root.Q<VisualElement>("InventoryContainer");
        _craftContainer = _root.Q<VisualElement>("CraftContainer");
        _battleContainer = _root.Q<VisualElement>("BattleContainer");
        _baseContainer = _root.Q<VisualElement>("BaseContainer");

        _shopScroll = _root.Q<ScrollView>("ShopScrollView");
        _invScroll = _root.Q<ScrollView>("InventoryScrollView");
        _craftScroll = _root.Q<ScrollView>("CraftScrollView");

        // 2. Stats
        _goldLabel = _root.Q<Label>("ShopGold");
        _gemLabel = _root.Q<Label>("ShopGem");
        _hpBar = _root.Q<ProgressBar>("HpBar");
        _staminaBar = _root.Q<ProgressBar>("StaminaBar");
        _expBar = _root.Q<ProgressBar>("ExpBar");
        _playerLevelLabel = _root.Q<Label>("LevelLabel");

        // 3. Tabs (Setup và lưu cache nút)
        SetupTabButton("TabShop", "Shop");
        SetupTabButton("TabInventory", "Inventory");
        SetupTabButton("TabCraft", "Craft");
        SetupTabButton("TabBattle", "Battle");
        SetupTabButton("TabBase", "Base");

        // 4. Shop Pagination
        var btnPrev = _root.Q<Button>("BtnPrev");
        var btnNext = _root.Q<Button>("BtnNext");
        _pageLabel = _root.Q<Label>("PageLabel");

        if (btnPrev != null) btnPrev.clicked += () => ChangePage(-1);
        if (btnNext != null) btnNext.clicked += () => ChangePage(1);

        // 5. Battle
        _monsterHpBar = _root.Q<ProgressBar>("MonsterHpBar");
        _btnAttack = _root.Q<Button>("BtnAttack");
        if (_btnAttack != null) _btnAttack.clicked += () => StartCoroutine(AttackProcess());

        // 6. Filter Chips (Inventory)
        SetupInvFilter("BtnFilterAll", "All");
        SetupInvFilter("BtnFilterWep", "Weapon");
        SetupInvFilter("BtnFilterArm", "Armor");
        SetupInvFilter("BtnFilterCon", "Consumable");

        // 7. Notification Logs
        var btnLogs = _root.Q<Button>("BtnNotiLog");
        if (btnLogs != null) btnLogs.clicked += () => StartCoroutine(LoadTransactionHistory());

        // --- REGISTER EVENTS ---
        GameEvents.OnCurrencyChanged += RefreshAllData;
        GameEvents.OnEquipRequest += HandleEquipRequest;

        // Init
        StartCoroutine(LoadProfile());
        SwitchTab("Shop"); // Mặc định mở Shop trước
    }

    void OnDisable()
    {
        GameEvents.OnCurrencyChanged -= RefreshAllData;
        GameEvents.OnEquipRequest -= HandleEquipRequest;
    }

    void RefreshAllData()
    {
        StartCoroutine(LoadProfile());
        // Nếu đang mở Inventory thì reload luôn Inventory để cập nhật số lượng
        if (_inventoryContainer.style.display == DisplayStyle.Flex) StartCoroutine(LoadInventory());
    }

    // --- EVENT HANDLERS ---

    void HandleEquipRequest(string itemId)
    {
        StartCoroutine(EquipItem(itemId));
    }

    public void UseItemFromHotbar(string itemId)
    {
        var item = _fullInventory.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            if (item.Type == "Consumable") StartCoroutine(UseItem(itemId));
            else if (item.Type == "Weapon" || item.Type == "Armor") StartCoroutine(EquipItem(itemId));
        }
        else
        {
            ToastManager.Instance.Show("Hết vật phẩm này!", false);
        }
    }

    // --- TAB SYSTEM ---

    void SetupTabButton(string btnName, string tabName)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null)
        {
            _tabButtons[tabName] = btn;
            btn.clicked += () => SwitchTab(tabName);
        }
    }

    void SetupInvFilter(string btnName, string type)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null)
        {
            _filterButtons[type] = btn;
            btn.clicked += () => {
                _currentFilter = type;
                FilterInventory(type);
            };
        }
    }

    void SwitchTab(string tabName)
    {
        // 1. Ẩn tất cả nội dung
        _shopContainer.style.display = DisplayStyle.None;
        _inventoryContainer.style.display = DisplayStyle.None;
        _craftContainer.style.display = DisplayStyle.None;
        _battleContainer.style.display = DisplayStyle.None;
        if (_baseContainer != null) _baseContainer.style.display = DisplayStyle.None;

        // 2. Reset Active State cho TẤT CẢ nút Tab
        foreach (var btn in _tabButtons.Values) btn.RemoveFromClassList("tab-active");

        // 3. Active nút Tab hiện tại
        if (_tabButtons.ContainsKey(tabName))
        {
            _tabButtons[tabName].AddToClassList("tab-active");
        }

        // 4. Hiển thị nội dung & Load dữ liệu
        if (tabName == "Shop")
        {
            _shopContainer.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadShopItems(_currentPage));
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

    // --- API CALLS: PROFILE ---

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

    // --- SHOP LOGIC ---
    void ChangePage(int dir)
    {
        _currentPage += dir;
        if (_currentPage < 1) _currentPage = 1;
        StartCoroutine(LoadShopItems(_currentPage));
    }

    IEnumerator LoadShopItems(int page)
    {
        _shopScroll.Clear();
        _shopScroll.Add(new Label("Loading Shop...") { style = { color = Color.gray, alignSelf = Align.Center } });

        yield return NetworkManager.Instance.SendRequest<List<ShopItemDto>>($"game/shop?page={page}&pageSize={_pageSize}", "GET", null,
            (items) => {
                _shopScroll.Clear();
                if (items.Count == 0 && page > 1) { _currentPage--; return; }
                if (_pageLabel != null) _pageLabel.text = $"Page {_currentPage}";

                foreach (var item in items)
                {
                    var card = CreateItemCard(item);
                    _shopScroll.Add(card);
                }
            },
            (err) => {
                _shopScroll.Clear();
                ToastManager.Instance.Show("Lỗi tải Shop: " + err, false);
            }
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

        if (item.PriceCurrency == "RES_GOLD")
        {
            btnGem.style.display = DisplayStyle.None;
            btnGold.Q<Label>("PriceGoldLabel").text = item.PriceAmount.ToString();
            btnGold.clicked += () => ShowDetailPopup(item);
        }
        else
        {
            btnGold.style.display = DisplayStyle.None;
            btnGem.Q<Label>("PriceGemLabel").text = item.PriceAmount.ToString();
            btnGem.clicked += () => ShowDetailPopup(item);
        }
        return template;
    }

    void ShowDetailPopup(ShopItemDto item)
    {
        var popup = PopupTemplate.Instantiate();
        var overlay = popup.Q<VisualElement>("DetailOverlay");
        _root.Add(overlay);

        popup.Q<Label>("DetailName").text = item.Name;
        popup.Q<Label>("DetailDesc").text = item.Description;
        StartCoroutine(popup.Q<Image>("DetailImage").LoadImage(item.ImageURL));

        int qty = 1;
        var lblQty = popup.Q<Label>("LblQuantity");
        var lblTotal = popup.Q<Label>("LblTotalPrice");

        Action UpdatePrice = () => {
            lblQty.text = qty.ToString();
            int total = item.PriceAmount * qty;
            lblTotal.text = $"Total: {total:N0} {(item.PriceCurrency == "RES_GOLD" ? "G" : "💎")}";
        };
        popup.Q<Button>("BtnPlus").clicked += () => { qty++; UpdatePrice(); };
        popup.Q<Button>("BtnMinus").clicked += () => { if (qty > 1) qty--; UpdatePrice(); };
        popup.Q<Button>("BtnConfirmBuy").clicked += () => {
            StartCoroutine(BuyProcess(item.ProductID, qty));
            _root.Remove(overlay);
        };
        popup.Q<Button>("BtnCloseDetail").clicked += () => _root.Remove(overlay);
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

    // --- INVENTORY (PHẦN ĐÃ ĐƯỢC SỬA LỖI) ---
    [cite_start]// [cite: 603] Đã thêm callback báo lỗi để không bị màn hình trắng
    IEnumerator LoadInventory()
    {
        // 1. Hiển thị trạng thái đang tải
        _invScroll.Clear();
        _invScroll.Add(new Label("Đang tải túi đồ...") { 
            style = { color = Color.gray, alignSelf = Align.Center, paddingTop = 50, fontSize = 16 } 
        });

        // 2. Gọi API lấy Inventory
        yield return NetworkManager.Instance.SendRequest<List<InventoryDto>>("game/inventory", "GET", null,
            (items) => {
                _fullInventory = items ?? new List<InventoryDto>();

                // Cập nhật sức chứa
                var capLabel = _root.Q<Label>("CapacityLabel");
                if (capLabel != null)
                {
                    capLabel.text = $"Bag: {_fullInventory.Count}/{GameConfig.MAX_INVENTORY_SLOTS_BASE}";
                    capLabel.style.color = (_fullInventory.Count >= GameConfig.MAX_INVENTORY_SLOTS_BASE) ? Color.red : Color.white;
                }

                // Render lại UI với bộ lọc hiện tại
                FilterInventory(_currentFilter);
            },
            // [QUAN TRỌNG] Callback xử lý lỗi
            (err) => {
                ToastManager.Instance.Show("Lỗi tải túi đồ: " + err, false);
                _invScroll.Clear();
                _invScroll.Add(new Label("Lỗi kết nối.\nKhông thể tải dữ liệu.") { 
                    style = { color = new Color(1f, 0.4f, 0.4f), alignSelf = Align.Center, paddingTop = 50, whiteSpace = WhiteSpace.Normal, unityTextAlign = TextAnchor.MiddleCenter } 
                });
            }
        );
    }

    void FilterInventory(string type)
    {
        // 1. Cập nhật Visual Active cho nút Filter
        foreach (var kvp in _filterButtons)
        {
            if (kvp.Key == type) kvp.Value.AddToClassList("filter-active");
            else kvp.Value.RemoveFromClassList("filter-active");
        }

        _invScroll.Clear();

        // 2. Lọc Item từ danh sách cache _fullInventory
        var list = (type == "All") ? _fullInventory : _fullInventory.Where(i => i.Type == type).ToList();

        // 3. Hiển thị thông báo nếu trống
        if (list.Count == 0)
        {
            var emptyLabel = new Label("Túi đồ trống.");
            emptyLabel.style.color = Color.gray;
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.paddingTop = 50;
            emptyLabel.style.fontSize = 18;
            _invScroll.Add(emptyLabel);
            return;
        }

        // 4. Render Item
        foreach (var inv in list)
        {
            var ui = ItemTemplate.Instantiate();
            var root = ui.Q<VisualElement>("ItemContainer");
            ui.Q<Label>("ItemName").text = inv.Name;
            StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(inv.ImageUrl));

            if (!string.IsNullOrEmpty(inv.Rarity)) root.AddToClassList($"rarity-{inv.Rarity.ToLower()}");
            
            // Ẩn giá tiền vì đây là Inventory
            var priceRow = ui.Q<VisualElement>("PriceRow");
            if(priceRow != null) priceRow.style.display = DisplayStyle.None;

            // Hiển thị số lượng
            var qtyLabel = ui.Q<Label>("QtyLabel");
            if (qtyLabel != null)
            {
                qtyLabel.text = $"x{inv.Quantity}";
                qtyLabel.style.display = DisplayStyle.Flex;
            }

            // Hiển thị trạng thái trang bị
            var equipLabel = ui.Q<Label>("EquippedLabel");
            if (equipLabel != null)
            {
                equipLabel.style.display = inv.IsEquipped ? DisplayStyle.Flex : DisplayStyle.None;
            }

            root.userData = inv.ItemId; // Lưu ID vào userData để DragManipulator dùng

            // Context Menu (Chuột phải)
            root.RegisterCallback<ClickEvent>(e => {
                if (e.button == 1) ShowContextMenu(inv, e.position);
            });

            // Kéo thả
            root.AddManipulator(new DragManipulator(root, _root));

            _invScroll.Add(ui);
        }
    }

    void ShowContextMenu(InventoryDto inv, Vector2 mousePos)
    {
        var old = _root.Q("ContextMenu");
        if (old != null) _root.Remove(old);

        var menu = ContextMenuTemplate.Instantiate();
        var menuRoot = menu.Q<VisualElement>("ContextMenu");

        // Chỉnh vị trí menu không tràn màn hình
        float x = mousePos.x;
        float y = mousePos.y;
        if (x + 160 > _root.resolvedStyle.width) x -= 160;
        if (y + 150 > _root.resolvedStyle.height) y -= 150;

        menuRoot.style.left = x;
        menuRoot.style.top = y;

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

    // --- ACTIONS ---

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

    // --- HISTORY LOGS ---
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
                if (logs.Count == 0) list.Add(new Label("Chưa có giao dịch nào."));

                foreach (var log in logs)
                {
                    string currencySymbol = (log.Currency == "RES_GEM") ? "💎" : "G";
                    var row = new Label($"[{log.Date}] {log.Action} ({log.Amount} {currencySymbol})");
                    row.style.color = log.Amount >= 0 ? Color.green : new Color(1f, 0.4f, 0.4f);
                    if (log.Currency == "RES_GEM") row.style.unityFontStyleAndWeight = FontStyle.Bold;
                    row.style.borderBottomWidth = 1;
                    row.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
                    list.Add(row);
                }
            },
            (err) => { list.Clear(); list.Add(new Label("Lỗi tải: " + err)); }
        );
    }

    // --- CRAFT & BATTLE ---
    IEnumerator LoadRecipes()
    {
        yield return NetworkManager.Instance.SendRequest<List<RecipeDto>>("game/recipes", "GET", null, (recipes) => {
            _craftScroll.Clear();
            foreach (var r in recipes)
            {
                var ui = ItemTemplate.Instantiate();
                var root = ui.Q<VisualElement>("ItemContainer");
                ui.Q<Label>("ItemName").text = r.ResultItemName;
                StartCoroutine(ui.Q<Image>("ItemImage").LoadImage(r.ResultItemImage));
                
                var btn = new Button { text = $"CRAFT ({r.CraftingTime}s)" };
                btn.AddToClassList("btn-buy");
                btn.clicked += () => StartCoroutine(CraftProcess(r));
                
                var priceRow = ui.Q<VisualElement>("PriceRow");
                if (priceRow != null) priceRow.style.display = DisplayStyle.None;
                
                root.Add(btn);
                _craftScroll.Add(ui);
            }
        }, null);
    }

    IEnumerator CraftProcess(RecipeDto r)
    {
        ToastManager.Instance.Show($"Đang chế tạo {r.ResultItemName}...", true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("craft");
        yield return new WaitForSeconds(r.CraftingTime);
        yield return NetworkManager.Instance.SendRequest<object>($"game/craft/{r.RecipeId}", "POST", null,
            (res) => { ToastManager.Instance.Show("Chế tạo hoàn tất!", true); GameEvents.TriggerCurrencyChanged(); },
            (err) => ToastManager.Instance.Show(err, false)
        );
    }

    IEnumerator SpawnMonster()
    {
        // Mock Spawn
        _currentMonster = new MonsterDto { Name = "Zombie", HP = 100, MaxHp = 100 };
        _root.Q<Label>("MonsterName").text = _currentMonster.Name;
        _monsterHpBar.value = 100;
        yield break;
    }

    IEnumerator AttackProcess()
    {
        yield return NetworkManager.Instance.SendRequest<HuntResponse>("game/hunt", "POST", null,
            (res) => {
                ToastManager.Instance.Show($"Damage dealt! +{res.GoldEarned}G", true);
                if (res.LevelUp) ToastManager.Instance.Show("LEVEL UP!", true);
                GameEvents.TriggerCurrencyChanged();
            }, null
        );
    }
}