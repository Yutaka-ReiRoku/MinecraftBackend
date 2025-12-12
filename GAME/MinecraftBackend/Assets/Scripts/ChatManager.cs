using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatManager : MonoBehaviour
{
    [System.Serializable]
    public class ChatMessageDto
    {
        public string Sender;
        public string Content;
        public string Time;
    }

    private UIDocument _uiDoc;
    private VisualElement _root;
    private ScrollView _chatHistory;
    private TextField _chatInput;

    [Header("Settings")]
    public float RefreshRate = 3.0f; 

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        _chatHistory = _root.Q<ScrollView>("ChatHistory");
        _chatInput = _root.Q<TextField>("ChatInput");

        
        if (_chatInput != null)
        {
            _chatInput.FixTextFieldInput(); 
            
            _chatInput.RegisterCallback<KeyDownEvent>(evt => 
            {
                if (evt.keyCode == KeyCode.Return) SendChat();
            });
        }
        

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
        yield return NetworkManager.Instance.SendRequest<List<ChatMessageDto>>("game/chat", "GET", null,
            (messages) => {
                if (_chatHistory == null) return;
                _chatHistory.Clear();
                foreach (var msg in messages)
                {
                    var lbl = new Label($"<b>[{msg.Time}] {msg.Sender}:</b> {msg.Content}");
                    lbl.style.fontSize = 12;
                    lbl.style.color = Color.white;
                    lbl.style.whiteSpace = WhiteSpace.Normal; 
                    lbl.style.marginBottom = 2;
                    lbl.enableRichText = true; 
                    _chatHistory.Add(lbl);
                }
            },
            (err) => { }
        );
    }

    void SendChat()
    {
        string content = _chatInput.value;
        if (string.IsNullOrWhiteSpace(content)) return;

        _chatInput.value = "";
        var body = new { msg = content };
        
        StartCoroutine(NetworkManager.Instance.SendRequest<object>("game/chat", "POST", body,
            (res) => {
                StartCoroutine(LoadChatMessages());
            },
            (err) => {
                ToastManager.Instance.Show("Lỗi gửi tin nhắn", false);
            }
        ));
    }
}