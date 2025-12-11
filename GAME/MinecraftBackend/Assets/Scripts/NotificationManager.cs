using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // UI Elements
    private Label _newsTicker;
    private ScrollView _notiList;
    private VisualElement _redDot;

    [Header("Settings")]
    public float TickerSpeed = 50f; // Tốc độ chữ chạy
    public float RefreshRate = 60f; // Tải lại tin tức mỗi 60s

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

        // 1. Setup News Ticker (Dòng chữ chạy trên cùng)
        _newsTicker = _root.Q<Label>("NewsTicker");
        
        // 2. Setup Notification List (Tham chiếu đến UI trong bảng Log)
        _notiList = _root.Q<ScrollView>("NotiLogList");
        _redDot = _root.Q<VisualElement>("NotiRedDot");

        // Bắt đầu vòng lặp lấy tin tức
        StartCoroutine(FetchMOTD());
    }

    void Update()
    {
        // Logic Animation chữ chạy (Marquee)
        if (_newsTicker != null)
        {
            // Lấy vị trí X hiện tại (dùng style.translate)
            float x = _newsTicker.resolvedStyle.translate.x;
            
            // Di chuyển sang trái
            x -= TickerSpeed * Time.deltaTime;

            // Lấy chiều rộng
            float textWidth = _newsTicker.measureText.width;
            float parentWidth = _root.resolvedStyle.width;

            // Nếu chạy hết chữ ra khỏi màn hình bên trái -> Reset về bên phải
            if (x < -(textWidth + 50)) 
            {
                x = parentWidth;
            }

            _newsTicker.style.translate = new Translate(x, 0, 0);
        }
    }

    /// <summary>
    /// Lấy thông báo máy chủ (Message of the Day) từ API
    /// </summary>
    IEnumerator FetchMOTD()
    {
        while (true)
        {
            // Gọi API lấy thông báo (Cần backend hỗ trợ GET /api/game/motd)
            // Nếu chưa có API, dùng text mặc định hoặc load từ file config
            // Ví dụ gọi API giả định:
            /*
            yield return NetworkManager.Instance.SendRequest<string>("game/motd", "GET", null, 
                (msg) => {
                    if (_newsTicker != null) _newsTicker.text = msg;
                },
                null
            );
            */
            
            // Demo update text
            if (_newsTicker != null && string.IsNullOrEmpty(_newsTicker.text))
            {
                 _newsTicker.text = "Welcome to Minecraft RPG Server! | X2 EXP Weekend Event is Live! | Don't forget to claim your Daily Reward.";
            }

            yield return new WaitForSeconds(RefreshRate);
        }
    }

    /// <summary>
    /// Thêm thông báo hệ thống vào danh sách (Silent Log)
    /// </summary>
    public void AddSystemNotification(string message)
    {
        if (_notiList != null)
        {
            string time = System.DateTime.Now.ToString("HH:mm");
            
            var lbl = new Label($"[SYSTEM] {message}");
            lbl.style.color = new Color(0.4f, 0.8f, 1f); // Màu xanh dương nhạt (khác với Toast thường)
            lbl.style.fontSize = 12;
            lbl.style.whiteSpace = WhiteSpace.Normal;
            lbl.style.borderBottomWidth = 1;
            lbl.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            lbl.style.marginBottom = 5;
            lbl.style.paddingBottom = 5;
            
            // Thêm vào đầu danh sách
            _notiList.Insert(0, lbl);

            // Bật chấm đỏ nếu panel đang đóng (Check visual parent display)
            // Giả sử logic check panel đóng/mở nằm ở ToastManager hoặc ta check style trực tiếp
            var panel = _root.Q<VisualElement>("NotiLogPanel");
            if (panel != null && panel.style.display == DisplayStyle.None && _redDot != null)
            {
                _redDot.style.display = DisplayStyle.Flex;
            }
        }
    }
}