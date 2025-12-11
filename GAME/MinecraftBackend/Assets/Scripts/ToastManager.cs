using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance;

    private VisualElement _root;
    private ScrollView _logList;
    private VisualElement _redDot;
    private VisualElement _logPanel;
    
    // Lưu tạm log trong RAM để hiển thị lại
    private List<string> _activityLogs = new List<string>();

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Lấy UIDocument từ GameObject hiện tại (thường là GameManager)
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        
        _root = uiDoc.rootVisualElement;

        // Query các phần tử UI (đã định nghĩa trong ShopScreen.uxml)
        _logList = _root.Q<ScrollView>("NotiLogList");
        _redDot = _root.Q<VisualElement>("NotiRedDot");
        _logPanel = _root.Q<VisualElement>("NotiLogPanel");

        // Gắn sự kiện nút mở/đóng Log
        var btnOpen = _root.Q<Button>("BtnNotiLog");
        var btnClose = _root.Q<Button>("BtnCloseNoti");

        if (btnOpen != null)
        {
            btnOpen.clicked += () => {
                if (_logPanel != null) _logPanel.style.display = DisplayStyle.Flex;
                if (_redDot != null) _redDot.style.display = DisplayStyle.None; // Đã xem -> Tắt chấm đỏ
            };
        }

        if (btnClose != null)
        {
            btnClose.clicked += () => {
                if (_logPanel != null) _logPanel.style.display = DisplayStyle.None;
            };
        }
    }

    /// <summary>
    /// Hiển thị thông báo bay (Toast) và lưu vào lịch sử
    /// </summary>
    /// <param name="message">Nội dung thông báo</param>
    /// <param name="isSuccess">True = Màu xanh (Thành công), False = Màu đỏ (Lỗi)</param>
    public void Show(string message, bool isSuccess)
    {
        if (_root == null)
        {
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc != null) _root = uiDoc.rootVisualElement;
            else return;
        }

        // 1. TẠO TOAST VISUAL ELEMENT
        var toast = new Label(message);
        
        // Style trực tiếp
        toast.style.position = Position.Absolute;
        toast.style.bottom = 100; // Cách đáy 100px
        
        // [FIXED] Đặt vị trí cố định bên phải, bỏ animation bay từ ngoài vào để tránh lỗi transition
        toast.style.right = 20; 
        
        toast.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.95f); // Nền tối
        toast.style.color = isSuccess ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f); // Xanh hoặc Đỏ
        
        toast.style.paddingTop = 12;
        toast.style.paddingBottom = 12;
        toast.style.paddingLeft = 20; 
        toast.style.paddingRight = 20;
        
        toast.style.borderLeftWidth = 5;
        toast.style.borderLeftColor = isSuccess ? Color.green : Color.red;
        
        // Set style bo góc từng cạnh để tránh lỗi phiên bản Unity cũ
        toast.style.borderTopRightRadius = 5;
        toast.style.borderBottomRightRadius = 5;
        toast.style.borderTopLeftRadius = 5;
        toast.style.borderBottomLeftRadius = 5;
        
        toast.style.fontSize = 16;
        toast.style.unityFontStyleAndWeight = FontStyle.Bold;

        // [FIXED] Xóa đoạn code Transition/Easing gây lỗi CS0117
        // Việc này làm Toast hiện ra ngay lập tức thay vì bay từ từ, đảm bảo không lỗi compile.

        _root.Add(toast);

        // 2. LOGIC TỰ ĐỘNG TẮT
        // Sau 3s: Xóa khỏi màn hình
        _root.schedule.Execute(() => {
            if(_root.Contains(toast)) _root.Remove(toast);
        }).ExecuteLater(3000);

        // 3. LƯU VÀO NHẬT KÝ (LOG)
        AddToLog(message);
    }

    void AddToLog(string msg)
    {
        string time = System.DateTime.Now.ToString("HH:mm");
        string entry = $"[{time}] {msg}";
        
        // Lưu data
        _activityLogs.Insert(0, entry);
        if (_activityLogs.Count > 20) _activityLogs.RemoveAt(_activityLogs.Count - 1); // Giữ 20 dòng mới nhất

        // Cập nhật UI List nếu đã khởi tạo
        if (_logList != null)
        {
            var label = new Label(entry);
            label.style.fontSize = 12;
            label.style.color = new Color(0.7f, 0.7f, 0.7f); // Màu xám nhạt
            label.style.whiteSpace = WhiteSpace.Normal; // Cho phép xuống dòng
            label.style.borderBottomWidth = 1;
            label.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            label.style.paddingBottom = 5;
            label.style.marginBottom = 5;
            // Thêm vào đầu danh sách
            _logList.Insert(0, label);
        }

        // Bật chấm đỏ thông báo nếu Panel đang đóng
        if (_logPanel != null && _logPanel.style.display == DisplayStyle.None && _redDot != null)
        {
            _redDot.style.display = DisplayStyle.Flex;
        }
    }
}