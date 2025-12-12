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
    
    
    private List<string> _activityLogs = new List<string>();

    void Awake()
    {
        
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        
        _root = uiDoc.rootVisualElement;

        
        _logList = _root.Q<ScrollView>("NotiLogList");
        _redDot = _root.Q<VisualElement>("NotiRedDot");
        _logPanel = _root.Q<VisualElement>("NotiLogPanel");

        
        var btnOpen = _root.Q<Button>("BtnNotiLog");
        var btnClose = _root.Q<Button>("BtnCloseNoti");

        if (btnOpen != null)
        {
            btnOpen.clicked += () => {
                if (_logPanel != null) _logPanel.style.display = DisplayStyle.Flex;
                if (_redDot != null) _redDot.style.display = DisplayStyle.None; 
            };
        }

        if (btnClose != null)
        {
            btnClose.clicked += () => {
                if (_logPanel != null) _logPanel.style.display = DisplayStyle.None;
            };
        }
    }

    
    
    
    
    
    public void Show(string message, bool isSuccess)
    {
        if (_root == null)
        {
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc != null) _root = uiDoc.rootVisualElement;
            else return;
        }

        
        var toast = new Label(message);
        
        
        toast.style.position = Position.Absolute;
        toast.style.bottom = 100; 
        
        
        toast.style.right = 20; 
        
        toast.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.95f); 
        toast.style.color = isSuccess ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f); 
        
        toast.style.paddingTop = 12;
        toast.style.paddingBottom = 12;
        toast.style.paddingLeft = 20; 
        toast.style.paddingRight = 20;
        
        toast.style.borderLeftWidth = 5;
        toast.style.borderLeftColor = isSuccess ? Color.green : Color.red;
        
        
        toast.style.borderTopRightRadius = 5;
        toast.style.borderBottomRightRadius = 5;
        toast.style.borderTopLeftRadius = 5;
        toast.style.borderBottomLeftRadius = 5;
        
        toast.style.fontSize = 16;
        toast.style.unityFontStyleAndWeight = FontStyle.Bold;

        
        

        _root.Add(toast);

        
        
        _root.schedule.Execute(() => {
            if(_root.Contains(toast)) _root.Remove(toast);
        }).ExecuteLater(3000);

        
        AddToLog(message);
    }

    void AddToLog(string msg)
    {
        string time = System.DateTime.Now.ToString("HH:mm");
        string entry = $"[{time}] {msg}";
        
        
        _activityLogs.Insert(0, entry);
        if (_activityLogs.Count > 20) _activityLogs.RemoveAt(_activityLogs.Count - 1); 

        
        if (_logList != null)
        {
            var label = new Label(entry);
            label.style.fontSize = 12;
            label.style.color = new Color(0.7f, 0.7f, 0.7f); 
            label.style.whiteSpace = WhiteSpace.Normal; 
            label.style.borderBottomWidth = 1;
            label.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            label.style.paddingBottom = 5;
            label.style.marginBottom = 5;
            
            _logList.Insert(0, label);
        }

        
        if (_logPanel != null && _logPanel.style.display == DisplayStyle.None && _redDot != null)
        {
            _redDot.style.display = DisplayStyle.Flex;
        }
    }
}