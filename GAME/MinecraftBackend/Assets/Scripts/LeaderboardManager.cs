using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LeaderboardManager : MonoBehaviour
{
    // DTO nhận dữ liệu
    [System.Serializable]
    public class LeaderboardEntryDto
    {
        public string DisplayName;
        public int Level;
        public string AvatarUrl;
        public int Gold;
    }

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    private VisualElement _popup;
    private ScrollView _listContainer;
    private Button _btnClose;
    private Button _btnOpen;

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        // Query Elements
        _popup = _root.Q<VisualElement>("LeaderboardPopup");
        
        if (_popup == null) return;

        _listContainer = _popup.Q<ScrollView>("LeaderboardList");
        _btnClose = _popup.Q<Button>("BtnCloseLeaderboard");
        _btnOpen = _root.Q<Button>("BtnLeaderboard");
        
        if (_btnOpen != null) _btnOpen.clicked += OpenLeaderboard;
        if (_btnClose != null) _btnClose.clicked += () => _popup.style.display = DisplayStyle.None;
    }

    public void OpenLeaderboard()
    {
        _popup.style.display = DisplayStyle.Flex;
        StartCoroutine(LoadLeaderboardData());
        
        // [FIXED] Xóa animation cũ gây lỗi StyleValues/Easing
        // Chỉ cần scale về 1 để hiển thị
        _popup.style.scale = new Scale(Vector3.one);
    }

    IEnumerator LoadLeaderboardData()
    {
        _listContainer.Clear();
        _listContainer.Add(new Label("Đang tải dữ liệu...") { style = { color = Color.gray, alignSelf = Align.Center } });
        
        yield return NetworkManager.Instance.SendRequest<List<LeaderboardEntryDto>>("game/leaderboard", "GET", null,
            (entries) => {
                RenderList(entries);
            },
            (err) => {
                _listContainer.Clear();
                _listContainer.Add(new Label("Lỗi tải BXH.") { style = { color = Color.red, alignSelf = Align.Center } });
            }
        );
    }

    void RenderList(List<LeaderboardEntryDto> entries)
    {
        _listContainer.Clear();
        if (entries == null || entries.Count == 0)
        {
            _listContainer.Add(new Label("Chưa có dữ liệu xếp hạng.") { style = { color = Color.white, alignSelf = Align.Center } });
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            int rank = i + 1;

            // Tạo Row
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 50;
            row.style.marginBottom = 5;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.backgroundColor = new Color(0, 0, 0, 0.3f);
            
            // [FIXED] Sửa lỗi borderRadius (Set 4 góc)
            row.style.borderTopLeftRadius = 5;
            row.style.borderTopRightRadius = 5;
            row.style.borderBottomLeftRadius = 5;
            row.style.borderBottomRightRadius = 5;

            // Màu sắc Top 3
            if (rank == 1) row.style.borderLeftColor = Color.yellow;
            else if (rank == 2) row.style.borderLeftColor = Color.gray; // Bạc
            else if (rank == 3) row.style.borderLeftColor = new Color(0.8f, 0.5f, 0.2f); // Đồng
            
            if (rank <= 3) row.style.borderLeftWidth = 4;

            // 1. Rank Number
            var lblRank = new Label($"#{rank}");
            lblRank.style.width = 40;
            lblRank.style.fontSize = 18;
            lblRank.style.unityFontStyleAndWeight = FontStyle.Bold;
            lblRank.style.color = (rank == 1) ? Color.yellow : Color.white;
            row.Add(lblRank);

            // 2. Avatar
            var avatar = new Image();
            avatar.style.width = 40; avatar.style.height = 40;
            avatar.style.marginRight = 10;
            
            // [FIXED] Sửa lỗi borderRadius cho Avatar (Tròn)
            avatar.style.borderTopLeftRadius = 20;
            avatar.style.borderTopRightRadius = 20;
            avatar.style.borderBottomLeftRadius = 20;
            avatar.style.borderBottomRightRadius = 20;

            StartCoroutine(avatar.LoadImage(entry.AvatarUrl));
            row.Add(avatar);

            // 3. Name
            var lblName = new Label(entry.DisplayName);
            lblName.style.flexGrow = 1;
            lblName.style.fontSize = 14;
            lblName.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(lblName);

            // 4. Level
            var lblLv = new Label($"Lv.{entry.Level}");
            lblLv.style.color = new Color(0.2f, 1f, 0.4f); // Xanh lá
            lblLv.style.fontSize = 14;
            row.Add(lblLv);

            _listContainer.Add(row);
        }
    }
}