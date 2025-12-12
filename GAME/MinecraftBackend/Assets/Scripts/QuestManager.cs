using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestManager : MonoBehaviour
{
    
    public static QuestManager Instance;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    
    private VisualElement _popup;
    private ScrollView _questList;
    
    
    private VisualElement _trackerPanel;
    private ScrollView _trackerList;

    
    [System.Serializable]
    public class QuestProgressDto
    {
        public string QuestId;
        public string Name;
        public string Description;
        public int Current;
        public int Target;
        public string Status; 
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

        
        _popup = _root.Q<VisualElement>("WikiPopup"); 
        
        if (_popup == null) _popup = _root.Q<VisualElement>("QuestPopup"); 
        
        
        
        
        
        _trackerPanel = _root.Q<VisualElement>("QuestTracker");
        if (_trackerPanel != null)
        {
            _trackerList = _trackerPanel.Q<ScrollView>("TrackerList");
            var iconTitle = _trackerPanel.Q<Image>("IconQuestTitle");
            if (iconTitle != null) StartCoroutine(iconTitle.LoadImage("/images/others/quest.png"));
        }

        
        GameEvents.OnPlayerDataRefreshNeeded += () => StartCoroutine(LoadQuests());
        
        
        StartCoroutine(LoadQuests());
    }

    void OnDestroy()
    {
        GameEvents.OnPlayerDataRefreshNeeded -= () => StartCoroutine(LoadQuests());
    }

    public void OpenQuestLog()
    {
        
        
        
        Debug.Log("Open Quest Log UI");
        
    }

    IEnumerator LoadQuests()
    {
        
        
        yield return NetworkManager.Instance.SendRequest<List<QuestProgressDto>>("game/my-quests", "GET", null,
            (quests) => {
                UpdateHUD(quests);
                
            },
            (err) => {
                
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
            
            if (q.Status == "CLAIMED") continue;

            hasActiveQuest = true;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 5;
            row.style.alignItems = Align.Center;

            
            var icon = new Label(q.Status == "COMPLETED" ? "✔" : "○");
            icon.style.color = q.Status == "COMPLETED" ? Color.green : Color.yellow;
            icon.style.marginRight = 5;
            icon.style.fontSize = 10;

            
            var label = new Label($"{q.Name}: {q.Current}/{q.Target}");
            label.style.fontSize = 12;
            label.style.color = Color.white;
            label.style.whiteSpace = WhiteSpace.Normal;

            
            if (q.Status == "COMPLETED")
            {
                label.style.color = new Color(0.5f, 1f, 0.5f); 
                
                var btnClaim = new Button();
                btnClaim.text = "NHẬN";
                btnClaim.AddToClassList("btn-confirm"); 
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

        
        if (_trackerPanel != null)
            _trackerPanel.style.display = hasActiveQuest ? DisplayStyle.Flex : DisplayStyle.None;
    }

    IEnumerator ClaimReward(string questId)
    {
        
        yield return NetworkManager.Instance.SendRequest<object>($"game/quests/claim/{questId}", "POST", null,
            (res) => {
                ToastManager.Instance.Show("Nhiệm vụ hoàn thành! Đã nhận thưởng.", true);
                AudioManager.Instance.PlaySFX("success");
                
                
                Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                EffectsManager.Instance.PlayConfetti(Camera.main.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, 10)));

                
                GameEvents.TriggerRefreshAll();
            },
            (err) => ToastManager.Instance.Show("Lỗi: " + err, false)
        );
    }
}