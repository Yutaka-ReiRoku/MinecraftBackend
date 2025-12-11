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
    
    // Data Cache - Sử dụng đúng class WikiEntryDto trong GameModels.cs
    private List<WikiEntryDto> _allEntries = new List<WikiEntryDto>();
    private string _currentTab = "ITEM"; // "ITEM" hoặc "MONSTER"

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
        
        // Ẩn mặc định
        if (_wikiPopup != null) _wikiPopup.style.display = DisplayStyle.None;
    }

    void OpenWiki()
    {
        if (_wikiPopup == null) return;
        
        // [FIX] Xóa Animation StyleValues/Easing gây lỗi
        // Chỉ đơn giản là hiện Popup lên
        _wikiPopup.style.display = DisplayStyle.Flex;
        
        StartCoroutine(LoadWikiData());
    }

    void CloseWiki()
    {
        if (_wikiPopup != null) _wikiPopup.style.display = DisplayStyle.None;
    }

    void SwitchTab(string type)
    {
        _currentTab = type;
        // Update UI Tabs (Highlight active tab)
        if (_tabItems != null) _tabItems.EnableInClassList("active-tab", type == "ITEM");
        if (_tabMobs != null) _tabMobs.EnableInClassList("active-tab", type == "MONSTER");

        RenderGrid();
    }

    IEnumerator LoadWikiData()
    {
        if (_wikiGrid != null)
        {
            _wikiGrid.Clear();
            _wikiGrid.Add(new Label("Đang tải dữ liệu...") { style = { color = Color.white, alignSelf = Align.Center, marginTop = 50 } });
        }

        // Gọi API lấy toàn bộ dữ liệu Wiki
        // Sử dụng WikiEntryDto đã định nghĩa trong GameModels.cs
        yield return NetworkManager.Instance.SendRequest<List<WikiEntryDto>>("game/wiki", "GET", null,
            (data) => {
                _allEntries = data;
                SwitchTab("ITEM"); // Mặc định hiển thị Item trước
            },
            (err) => {
                if (_wikiGrid != null)
                {
                    _wikiGrid.Clear();
                    _wikiGrid.Add(new Label("Lỗi tải Wiki.") { style = { color = Color.red, alignSelf = Align.Center } });
                }
            }
        );
    }

    void RenderGrid()
    {
        if (_wikiGrid == null) return;
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
            // Tạo Card UI
            var card = new VisualElement();
            card.style.width = 100;
            card.style.height = 130;
            card.style.marginRight = 10;
            card.style.marginBottom = 10;
            card.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            
            // [FIX] Sửa lỗi CS1061: Set border từng cạnh thay vì set gộp
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            
            card.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
            card.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);
            card.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f);
            card.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f);

            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;

            card.style.alignItems = Align.Center;
            card.style.paddingTop = 10;

            // Ảnh Item
            var img = new Image();
            img.style.width = 64; 
            img.style.height = 64;
            img.style.marginBottom = 5;

            // Tên Item
            var lbl = new Label(entry.Name);
            lbl.style.fontSize = 10;
            lbl.style.whiteSpace = WhiteSpace.Normal;
            lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            lbl.style.color = Color.white;

            // Logic Mở khóa
            if (entry.IsUnlocked)
            {
                // Đã mở khóa: Load ảnh màu, viền xanh
                StartCoroutine(img.LoadImage(entry.ProductImage));
                
                // Highlight viền xanh
                card.style.borderTopColor = new Color(0, 0.8f, 1f);
                card.style.borderBottomColor = new Color(0, 0.8f, 1f);
                card.style.borderLeftColor = new Color(0, 0.8f, 1f);
                card.style.borderRightColor = new Color(0, 0.8f, 1f);
            }
            else
            {
                // Chưa mở khóa: Load ảnh nhưng tint đen, tên ???
                StartCoroutine(img.LoadImage(entry.ProductImage));
                img.style.unityBackgroundImageTintColor = Color.black; 
                lbl.text = "???";
                lbl.style.color = Color.gray;
            }

            card.Add(img);
            card.Add(lbl);

            // Tooltip khi hover (nếu đã mở khóa)
            if (entry.IsUnlocked)
            {
                card.RegisterCallback<MouseEnterEvent>(e => {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Show(entry.Name, "Đã sưu tầm", entry.Type, "Common");
                });
                
                card.RegisterCallback<MouseLeaveEvent>(e => {
                    if (TooltipManager.Instance != null)
                        TooltipManager.Instance.Hide();
                });
            }

            _wikiGrid.Add(card);
        }
    }
}