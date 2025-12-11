using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatManager : MonoBehaviour
{
    // DTO tin nhắn
    [System.Serializable]
    public class ChatMessageDto
    {
        public string Sender;
        public string Content;
        public string Time;
    }

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // UI Elements
    private ScrollView _chatHistory;
    private TextField _chatInput;

    [Header("Settings")]
    public float RefreshRate = 3.0f; // Tải tin mới mỗi 3 giây

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        _chatHistory = _root.Q<ScrollView>("ChatHistory");
        _chatInput = _root.Q<TextField>("ChatInput");

        // Đăng ký sự kiện nhập liệu
        if (_chatInput != null)
        {
            _chatInput.RegisterCallback<KeyDownEvent>(evt => 
            {
                if (evt.keyCode == KeyCode.Return) // Nhấn Enter để gửi
                {
                    SendChat();
                }
            });
        }

        // Bắt đầu vòng lặp tải tin nhắn
        StartCoroutine(ChatLoop());
    }

    IEnumerator ChatLoop()
    {
        while (true)
        {
            yield return LoadChatMessages();
            yield return new WaitForSeconds(RefreshRate);
        }
    }

    IEnumerator LoadChatMessages()
    {
        // Gọi API lấy tin nhắn (Cần backend hỗ trợ GET /api/game/chat)
        // Nếu backend chưa có, script này sẽ báo lỗi 404 nhẹ nhàng và không làm crash game
        yield return NetworkManager.Instance.SendRequest<List<ChatMessageDto>>("game/chat", "GET", null,
            (messages) => {
                if (_chatHistory == null) return;
                
                _chatHistory.Clear();
                foreach (var msg in messages)
                {
                    var lbl = new Label($"<b>[{msg.Time}] {msg.Sender}:</b> {msg.Content}");
                    lbl.style.fontSize = 12;
                    lbl.style.color = Color.white;
                    lbl.style.whiteSpace = WhiteSpace.Normal; // Cho phép xuống dòng
                    lbl.style.marginBottom = 2;
                    
                    // Style tên người gửi khác màu
                    lbl.enableRichText = true; // Cần bật Rich Text trong UI Builder hoặc code

                    _chatHistory.Add(lbl);
                }
                
                // Tự động cuộn xuống dưới cùng (nếu cần)
                // _chatHistory.ScrollTo(_chatHistory.contentContainer.layout.height);
            },
            (err) => {
                // Silent fail: Không spam lỗi nếu server chưa có chat
            }
        );
    }

    void SendChat()
    {
        string content = _chatInput.value;
        if (string.IsNullOrWhiteSpace(content)) return;

        // Reset input ngay lập tức cho mượt
        _chatInput.value = "";

        // Gửi lên Server
        // Body: { msg = "Hello" } - Tùy backend định nghĩa
        var body = new { msg = content };

        StartCoroutine(NetworkManager.Instance.SendRequest<object>("game/chat", "POST", body,
            (res) => {
                // Gửi thành công -> Reload ngay lập tức để hiện tin mình vừa chat
                StartCoroutine(LoadChatMessages());
            },
            (err) => {
                ToastManager.Instance.Show("Lỗi gửi tin nhắn", false);
            }
        ));
    }
}