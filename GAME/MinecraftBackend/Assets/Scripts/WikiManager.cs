using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class WikiManager : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;

    // UI Elements
    private VisualElement _wikiPopup;
    private ScrollView _wikiGrid;
    private Button _btnClose;
    private Button _btnOpenWiki; // Nút mở trên HUD

    // Tabs
    private Button _tabItems;
    private Button _tabMobs;

    // Data Cache
    private List<WikiEntryDto> _allEntries = new List<WikiEntryDto>();
    private string _currentTab = "ITEM"; // "ITEM" hoặc "MONSTER"

    // DTO nhận từ API
    [System.Serializable]
    public class WikiEntryDto
    {
        public string Name;
        public string ProductImage;
        public string Type; // Resource, Weapon, Monster...
        public bool IsUnlocked; // True nếu user đã từng sở hữu/gặp
    }

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        // Query UI
        _wikiPopup = _root.Q<VisualElement>("WikiPopup");
        _wikiGrid = _root.Q<ScrollView>("WikiGrid");
        
        _btnClose = _root.Q<Button>("BtnCloseWiki");
        if (_btnClose != null) _btnClose.clicked += CloseWiki;

        _btnOpenWiki = _root.Q<Button>("BtnWiki");
        if (_btnOpenWiki != null) _btnOpenWiki.clicked += OpenWiki;

        // Tabs
        _tabItems = _root.Q<Button>("TabWikiItems");
        _tabMobs = _root.Q<Button>("TabWikiMobs");

        if (_tabItems != null) _tabItems.clicked += () => SwitchTab("ITEM");
        if (_tabMobs != null) _tabMobs.clicked += () => SwitchTab("MONSTER");
    }

    void OpenWiki()
    {
        if (_wikiPopup == null) return;
        
        _wikiPopup.style.display = DisplayStyle.Flex;
        
        // Animation
        _wikiPopup.style.opacity = 0;
        _wikiPopup.style.scale = new Scale(Vector3.one * 0.8f);
        _wikiPopup.experimental.animation.Start(
            new StyleValues { opacity = 0, scale = new Scale(Vector3.one * 0.8f) }, 
            new StyleValues { opacity = 1, scale = new Scale(Vector3.one) }, 
            300).Ease(Easing.OutBack);

        StartCoroutine(LoadWikiData());
    }

    void CloseWiki()
    {
        if (_wikiPopup != null) _wikiPopup.style.display = DisplayStyle.None;
    }

    void SwitchTab(string type)
    {
        _currentTab = type;
        
        // Update UI Tabs
        if (_tabItems != null) _tabItems.EnableInClassList("active-tab", type == "ITEM");
        if (_tabMobs != null) _tabMobs.EnableInClassList("active-tab", type == "MONSTER");

        RenderGrid();
    }

    IEnumerator LoadWikiData()
    {
        _wikiGrid.Clear();
        _wikiGrid.Add(new Label("Đang tải dữ liệu...") { style = { color = Color.white, alignSelf = Align.Center, marginTop = 50 } });

        // Gọi API lấy toàn bộ dữ liệu Wiki
        // Cần đảm bảo Backend có API GET /api/game/wiki
        yield return NetworkManager.Instance.SendRequest<List<WikiEntryDto>>("game/wiki", "GET", null,
            (data) => {
                _allEntries = data;
                SwitchTab("ITEM"); // Mặc định hiển thị Item trước
            },
            (err) => {
                _wikiGrid.Clear();
                _wikiGrid.Add(new Label("Lỗi tải Wiki.") { style = { color = Color.red, alignSelf = Align.Center } });
            }
        );
    }

    void RenderGrid()
    {
        _wikiGrid.Clear();

        // Lọc dữ liệu theo Tab
        var filteredList = _currentTab == "MONSTER" 
            ? _allEntries.Where(x => x.Type == "MONSTER" || x.Type == "Monster").ToList()
            : _allEntries.Where(x => x.Type != "MONSTER" && x.Type != "Monster").ToList();

        if (filteredList.Count == 0)
        {
            _wikiGrid.Add(new Label("Chưa có dữ liệu.") { style = { color = Color.gray, alignSelf = Align.Center } });
            return;
        }

        foreach (var entry in filteredList)
        {
            // Tạo Card
            var card = new VisualElement();
            card.style.width = 100;
            card.style.height = 130;
            card.style.marginRight = 10;
            card.style.marginBottom = 10;
            card.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            card.style.borderWidth = 1;
            card.style.borderColor = new Color(0.5f, 0.5f, 0.5f);
            card.style.borderRadius = 8;
            card.style.alignItems = Align.Center;
            card.style.paddingTop = 10;

            // Ảnh
            var img = new Image();
            img.style.width = 64; 
            img.style.height = 64;
            img.style.marginBottom = 5;

            // Tên
            var lbl = new Label(entry.Name);
            lbl.style.fontSize = 10;
            lbl.style.whiteSpace = WhiteSpace.Normal;
            lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            lbl.style.color = Color.white;

            // Logic Mở khóa
            if (entry.IsUnlocked)
            {
                // Đã mở khóa: Load ảnh màu, tên sáng
                StartCoroutine(img.LoadImage(entry.ProductImage));
                card.style.borderColor = new Color(0, 0.8f, 1f); // Viền xanh
            }
            else
            {
                // Chưa mở khóa: Load ảnh nhưng tint đen, tên ???
                StartCoroutine(img.LoadImage(entry.ProductImage));
                img.style.unityBackgroundImageTintColor = Color.black; // Bóng đen
                lbl.text = "???";
                lbl.style.color = Color.gray;
            }

            card.Add(img);
            card.Add(lbl);
            
            // Tooltip khi hover (nếu đã mở khóa)
            if (entry.IsUnlocked)
            {
                card.RegisterCallback<MouseEnterEvent>(e => 
                    TooltipManager.Instance.Show(entry.Name, "Đã sưu tầm", entry.Type, "Common"));
                card.RegisterCallback<MouseLeaveEvent>(e => 
                    TooltipManager.Instance.Hide());
            }

            _wikiGrid.Add(card);
        }
    }
}