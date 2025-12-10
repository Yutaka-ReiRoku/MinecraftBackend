using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestManager : MonoBehaviour
{
    // Singleton để dễ gọi từ các sự kiện global
    public static QuestManager Instance;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // Popup lớn (Danh sách chi tiết)
    private VisualElement _popup;
    private ScrollView _questList;
    
    // HUD Tracker (Góc màn hình)
    private VisualElement _trackerPanel;
    private ScrollView _trackerList;

    // DTO cho Client (Map với API trả về)
    [System.Serializable]
    public class QuestProgressDto
    {
        public string QuestId;
        public string Name;
        public string Description;
        public int Current;
        public int Target;
        public string Status; // "IN_PROGRESS", "COMPLETED", "CLAIMED"
        public string RewardName;
        public string IconUrl;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        // 1. Query Elements
        _popup = _root.Q<VisualElement>("WikiPopup"); // Tận dụng popup Wiki hoặc tạo QuestPopup riêng
        // Nếu dùng QuestPopup riêng trong UXML:
        if (_popup == null) _popup = _root.Q<VisualElement>("QuestPopup"); 
        
        // Nếu chưa có QuestPopup, ta dùng chung container Wiki nhưng đổi tiêu đề (tiết kiệm UI)
        // Hoặc giả định bạn đã thêm QuestPopup vào MainLayout.uxml theo hướng dẫn trước.
        
        // Để an toàn, tôi sẽ query theo tên chuẩn đã định nghĩa trong ShopScreen.uxml phần HUD
        _trackerPanel = _root.Q<VisualElement>("QuestTracker");
        if (_trackerPanel != null)
        {
            _trackerList = _trackerPanel.Q<ScrollView>("TrackerList");
            [cite_start]// Tải icon Quest chuẩn cho HUD [cite: 3026]
            var iconTitle = _trackerPanel.Q<Image>("IconQuestTitle");
            if (iconTitle != null) StartCoroutine(iconTitle.LoadImage("/images/others/quest.png"));
        }

        // Lắng nghe sự kiện Refresh để cập nhật tiến độ
        GameEvents.OnPlayerDataRefreshNeeded += () => StartCoroutine(LoadQuests());
        
        // Tải lần đầu
        StartCoroutine(LoadQuests());
    }

    void OnDestroy()
    {
        GameEvents.OnPlayerDataRefreshNeeded -= () => StartCoroutine(LoadQuests());
    }

    public void OpenQuestLog()
    {
        // Hàm này gọi khi bấm nút Quest trên HUD hoặc Menu
        // Hiện tại ta dùng chung UI hoặc cần một Popup riêng. 
        // Demo: Log ra console hoặc mở Wiki tab Quest
        Debug.Log("Open Quest Log UI");
        // Logic mở popup chi tiết sẽ nằm ở đây
    }

    IEnumerator LoadQuests()
    {
        // Gọi API lấy danh sách nhiệm vụ của người chơi
        // API: GET /api/game/my-quests (Cần đảm bảo backend có endpoint này trả về tiến độ)
        yield return NetworkManager.Instance.SendRequest<List<QuestProgressDto>>("game/my-quests", "GET", null,
            (quests) => {
                UpdateHUD(quests);
                // UpdatePopup(quests); // Nếu đang mở popup
            },
            (err) => {
                // Nếu lỗi hoặc chưa có quest, ẩn tracker
                if(_trackerPanel != null) _trackerPanel.style.display = DisplayStyle.None;
            }
        );
    }

    void UpdateHUD(List<QuestProgressDto> quests)
    {
        if (_trackerList == null) return;
        _trackerList.Clear();

        bool hasActiveQuest = false;

        foreach (var q in quests)
        {
            // Chỉ hiện những quest chưa nhận thưởng
            if (q.Status == "CLAIMED") continue;

            hasActiveQuest = true;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 5;
            row.style.alignItems = Align.Center;

            // Icon trạng thái
            var icon = new Label(q.Status == "COMPLETED" ? "✔" : "○");
            icon.style.color = q.Status == "COMPLETED" ? Color.green : Color.yellow;
            icon.style.marginRight = 5;
            icon.style.fontSize = 10;

            // Tên và Tiến độ
            var label = new Label($"{q.Name}: {q.Current}/{q.Target}");
            label.style.fontSize = 12;
            label.style.color = Color.white;
            label.style.whiteSpace = WhiteSpace.Normal;

            // Nếu hoàn thành, thêm nút nhận thưởng ngay trên HUD cho tiện
            if (q.Status == "COMPLETED")
            {
                label.style.color = new Color(0.5f, 1f, 0.5f); // Xanh lá
                
                var btnClaim = new Button();
                btnClaim.text = "NHẬN";
                btnClaim.AddToClassList("btn-confirm"); // Style xanh
                btnClaim.style.height = 20;
                btnClaim.style.fontSize = 10;
                btnClaim.style.marginLeft = 5;
                
                btnClaim.clicked += () => StartCoroutine(ClaimReward(q.QuestId));
                
                row.Add(icon);
                row.Add(label);
                row.Add(btnClaim);
            }
            else
            {
                row.Add(icon);
                row.Add(label);
            }

            _trackerList.Add(row);
        }

        // Ẩn HUD nếu không có nhiệm vụ nào
        if (_trackerPanel != null)
            _trackerPanel.style.display = hasActiveQuest ? DisplayStyle.Flex : DisplayStyle.None;
    }

    IEnumerator ClaimReward(string questId)
    {
        // Gọi API nhận thưởng
        yield return NetworkManager.Instance.SendRequest<object>($"game/quests/claim/{questId}", "POST", null,
            (res) => {
                ToastManager.Instance.Show("Nhiệm vụ hoàn thành! Đã nhận thưởng.", true);
                AudioManager.Instance.PlaySFX("success");
                
                // Hiệu ứng pháo hoa
                Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                EffectsManager.Instance.PlayConfetti(Camera.main.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, 10)));

                // Reload lại dữ liệu (Tiền, Túi đồ, Quest)
                GameEvents.TriggerRefreshAll();
            },
            (err) => ToastManager.Instance.Show("Lỗi: " + err, false)
        );
    }
}