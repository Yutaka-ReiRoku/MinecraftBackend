using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    
    private Label _newsTicker;
    private ScrollView _notiList;
    private VisualElement _redDot;

    [Header("Settings")]
    public float TickerSpeed = 50f; 
    public float RefreshRate = 60f; 

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

        
        _newsTicker = _root.Q<Label>("NewsTicker");
        
        
        _notiList = _root.Q<ScrollView>("NotiLogList");
        _redDot = _root.Q<VisualElement>("NotiRedDot");

        
        StartCoroutine(FetchMOTD());
    }

    void Update()
    {
        
        if (_newsTicker != null)
        {
            
            float x = _newsTicker.resolvedStyle.translate.x;
            
            
            x -= TickerSpeed * Time.deltaTime;

            
            float textWidth = _newsTicker.MeasureTextSize(_newsTicker.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;
            float parentWidth = _root.resolvedStyle.width;

            
            if (x < -(textWidth + 50)) 
            {
                x = parentWidth;
            }

            _newsTicker.style.translate = new Translate(x, 0, 0);
        }
    }

    
    
    
    IEnumerator FetchMOTD()
    {
        while (true)
        {
            
            
            
            /*
            yield return NetworkManager.Instance.SendRequest<string>("game/motd", "GET", null, 
                (msg) => {
                    if (_newsTicker != null) _newsTicker.text = msg;
                },
                null
            );
            */
            
            
            if (_newsTicker != null && string.IsNullOrEmpty(_newsTicker.text))
            {
                 _newsTicker.text = "Welcome to Minecraft RPG Server! | X2 EXP Weekend Event is Live! | Don't forget to claim your Daily Reward.";
            }

            yield return new WaitForSeconds(RefreshRate);
        }
    }

    
    
    
    public void AddSystemNotification(string message)
    {
        if (_notiList != null)
        {
            string time = System.DateTime.Now.ToString("HH:mm");
            
            var lbl = new Label($"[SYSTEM] {message}");
            lbl.style.color = new Color(0.4f, 0.8f, 1f); 
            lbl.style.fontSize = 12;
            lbl.style.whiteSpace = WhiteSpace.Normal;
            lbl.style.borderBottomWidth = 1;
            lbl.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            lbl.style.marginBottom = 5;
            lbl.style.paddingBottom = 5;
            
            
            _notiList.Insert(0, lbl);

            
            
            var panel = _root.Q<VisualElement>("NotiLogPanel");
            if (panel != null && panel.style.display == DisplayStyle.None && _redDot != null)
            {
                _redDot.style.display = DisplayStyle.Flex;
            }
        }
    }
}