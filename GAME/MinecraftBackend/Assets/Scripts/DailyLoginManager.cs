using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System; 

public class DailyLoginManager : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;
    
    private VisualElement _popup;
    private VisualElement _daysContainer;
    private Button _btnClaim;
    private Button _btnClose;

    
    [System.Serializable]
    public class DailyCheckinResponse
    {
        public string Message;
        public int Gold;
        public int Streak;
    }
    
    [System.Serializable]
    public class DailyStatusDto
    {
        public int LoginStreak;
        public bool HasClaimedDaily;
    }

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        
        _popup = _root.Q<VisualElement>("DailyLoginPopup");
        if (_popup == null) return;

        _daysContainer = _popup.Q<VisualElement>("DaysContainer"); 
        _btnClaim = _popup.Q<Button>("BtnClaimDaily");
        _btnClose = _popup.Q<Button>("BtnCloseDaily");

        if (_btnClose != null) _btnClose.clicked += () => _popup.style.display = DisplayStyle.None;
        if (_btnClaim != null) _btnClaim.clicked += () => StartCoroutine(ClaimProcess());

        
        StartCoroutine(CheckDailyStatus());
    }

    IEnumerator CheckDailyStatus()
    {
        
        yield return NetworkManager.Instance.SendRequest<CharacterDto>("game/profile/me", "GET", null,
            (profile) => {
                
                
                string lastClaimDate = PlayerPrefs.GetString("LastDailyClaim", "");
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                
                if (lastClaimDate != today)
                {
                    
                    ShowPopup(1); 
                }
                else
                {
                    
                    _popup.style.display = DisplayStyle.None;
                }
            },
            (err) => { 
                
                _popup.style.display = DisplayStyle.None; 
            }
        );
    }

    void ShowPopup(int currentStreak)
    {
        _popup.style.display = DisplayStyle.Flex;
        _daysContainer.Clear();
        for (int i = 1; i <= 7; i++)
        {
            var dayBox = new VisualElement();
            dayBox.style.width = 60; 
            dayBox.style.height = 80;
            dayBox.style.marginRight = 5;
            dayBox.style.backgroundColor = new Color(0, 0, 0, 0.5f);

            
            dayBox.style.borderTopWidth = 1;
            dayBox.style.borderBottomWidth = 1;
            dayBox.style.borderLeftWidth = 1;
            dayBox.style.borderRightWidth = 1;

            dayBox.style.borderTopColor = Color.gray;
            dayBox.style.borderBottomColor = Color.gray;
            dayBox.style.borderLeftColor = Color.gray;
            dayBox.style.borderRightColor = Color.gray;

            dayBox.style.borderTopLeftRadius = 5;
            dayBox.style.borderTopRightRadius = 5;
            dayBox.style.borderBottomLeftRadius = 5;
            dayBox.style.borderBottomRightRadius = 5;

            dayBox.style.alignItems = Align.Center;
            dayBox.style.justifyContent = Justify.Center;

            var lblDay = new Label($"Day {i}");
            lblDay.style.fontSize = 10;
            lblDay.style.color = Color.white;

            var icon = new Image();
            icon.style.width = 30; icon.style.height = 30;
            
            var lblReward = new Label($"{100 * i} G");
            lblReward.style.fontSize = 12;
            lblReward.style.color = Color.yellow;
            lblReward.style.unityFontStyleAndWeight = FontStyle.Bold;

            
            if (i == currentStreak + 1)
            {
                
                dayBox.style.borderTopColor = Color.yellow;
                dayBox.style.borderBottomColor = Color.yellow;
                dayBox.style.borderLeftColor = Color.yellow;
                dayBox.style.borderRightColor = Color.yellow;

                
                dayBox.style.borderTopWidth = 2;
                dayBox.style.borderBottomWidth = 2;
                dayBox.style.borderLeftWidth = 2;
                dayBox.style.borderRightWidth = 2;

                dayBox.style.backgroundColor = new Color(1f, 1f, 0, 0.2f);
                lblDay.text = "TODAY";
                lblDay.style.color = Color.yellow;
            }
            
            else if (i <= currentStreak)
            {
                dayBox.style.opacity = 0.5f;
                var check = new Label("✔");
                check.style.color = Color.green;
                dayBox.Add(check);
            }

            dayBox.Add(lblDay);
            dayBox.Add(icon); 
            dayBox.Add(lblReward);

            _daysContainer.Add(dayBox);
        }
    }

    IEnumerator ClaimProcess()
    {
        _btnClaim.SetEnabled(false);
        yield return NetworkManager.Instance.SendRequest<DailyCheckinResponse>("game/daily-checkin", "POST", null,
            (res) => {
                ToastManager.Instance.Show(res.Message, true);
                AudioManager.Instance.PlaySFX("coins");
                GameEvents.TriggerCurrencyChanged();
                
                
                PlayerPrefs.SetString("LastDailyClaim", DateTime.Now.ToString("yyyy-MM-dd"));
                PlayerPrefs.Save();
                
                
                _root.schedule.Execute(() => _popup.style.display = DisplayStyle.None).ExecuteLater(1000);
            },
            (err) => {
                ToastManager.Instance.Show(err, false);
                _btnClaim.SetEnabled(true);
            }
        );
    }
}