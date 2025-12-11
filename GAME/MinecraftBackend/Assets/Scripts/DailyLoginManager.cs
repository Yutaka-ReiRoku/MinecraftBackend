using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DailyLoginManager : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // UI Elements
    private VisualElement _popup;
    private VisualElement _daysContainer;
    private Button _btnClaim;
    private Button _btnClose;

    // DTO nhận dữ liệu từ API Checkin
    [System.Serializable]
    public class DailyCheckinResponse
    {
        public string Message;
        public int Gold;
        public int Streak;
    }
    
    // DTO Profile để check trạng thái (cần thêm fields này vào API Profile nếu chưa có)
    // Hoặc gọi API riêng. Ở đây ta giả định API Profile trả về thông tin này.
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

        // Query UI
        // Cần thêm DailyLoginPopup vào ShopScreen.uxml nếu chưa có
        // Cấu trúc giả định: Overlay -> Box -> DaysRow -> Button
        _popup = _root.Q<VisualElement>("DailyLoginPopup");
        
        if (_popup == null) return;

        _daysContainer = _popup.Q<VisualElement>("DaysContainer"); // Container ngang chứa 7 ô
        _btnClaim = _popup.Q<Button>("BtnClaimDaily");
        _btnClose = _popup.Q<Button>("BtnCloseDaily");

        if (_btnClose != null) _btnClose.clicked += () => _popup.style.display = DisplayStyle.None;
        if (_btnClaim != null) _btnClaim.clicked += () => StartCoroutine(ClaimProcess());

        // Kiểm tra trạng thái ngay khi vào game
        StartCoroutine(CheckDailyStatus());
    }

    IEnumerator CheckDailyStatus()
    {
        // Gọi API lấy thông tin Profile (hoặc API riêng /game/daily-status)
        // Ta tận dụng API Profile hiện có, giả sử nó trả về Streak
        yield return NetworkManager.Instance.SendRequest<CharacterDto>("game/profile", "GET", null,
            (profile) => {
                // Lưu ý: Cần đảm bảo Backend trả về field LoginStreak & HasClaimedDaily
                // Nếu chưa có, bạn cần thêm vào CharacterDto ở Backend và Client
                
                // Giả lập logic client nếu backend chưa update DTO:
                // int streak = profile.LoginStreak; 
                // bool claimed = profile.HasClaimedDaily;
                
                // DEMO: Giả sử chưa nhận để test UI
                int streak = 2; 
                bool claimed = false; 

                if (!claimed)
                {
                    ShowPopup(streak);
                }
            },
            (err) => { /* Silent fail */ }
        );
    }

    void ShowPopup(int currentStreak)
    {
        _popup.style.display = DisplayStyle.Flex;
        
        // Render 7 ngày
        _daysContainer.Clear();
        for (int i = 1; i <= 7; i++)
        {
            var dayBox = new VisualElement();
            dayBox.style.width = 60; 
            dayBox.style.height = 80;
            dayBox.style.marginRight = 5;
            dayBox.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            dayBox.style.borderWidth = 1;
            dayBox.style.borderColor = Color.gray;
            dayBox.style.alignItems = Align.Center;
            dayBox.style.justifyContent = Justify.Center;
            dayBox.style.borderRadius = 5;

            var lblDay = new Label($"Day {i}");
            lblDay.style.fontSize = 10;
            lblDay.style.color = Color.white;

            var icon = new Image();
            icon.style.width = 30; icon.style.height = 30;
            // Load icon vàng
            // StartCoroutine(icon.LoadImage("/images/resources/gold_ingot.png"));

            var lblReward = new Label($"{100 * i} G");
            lblReward.style.fontSize = 12;
            lblReward.style.color = Color.yellow;
            lblReward.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Highlight ngày hiện tại
            if (i == currentStreak + 1)
            {
                dayBox.style.borderColor = Color.yellow;
                dayBox.style.borderWidth = 2;
                dayBox.style.backgroundColor = new Color(1f, 1f, 0, 0.2f);
                lblDay.text = "TODAY";
                lblDay.style.color = Color.yellow;
            }
            // Ngày đã nhận
            else if (i <= currentStreak)
            {
                dayBox.style.opacity = 0.5f;
                var check = new Label("✔");
                check.style.color = Color.green;
                dayBox.Add(check);
            }

            dayBox.Add(lblDay);
            dayBox.Add(icon); // Cần load ảnh thật
            dayBox.Add(lblReward);

            _daysContainer.Add(dayBox);
        }
    }

    IEnumerator ClaimProcess()
    {
        _btnClaim.SetEnabled(false);

        // Gọi API nhận thưởng
        yield return NetworkManager.Instance.SendRequest<DailyCheckinResponse>("game/daily-checkin", "POST", null,
            (res) => {
                ToastManager.Instance.Show(res.Message, true);
                AudioManager.Instance.PlaySFX("coins");
                
                // Hiệu ứng tiền bay
                // Vector2 pos = _btnClaim.worldBound.center;
                // EffectsManager.Instance.SpawnFloatingText(pos, $"+{res.Gold} G", Color.yellow);

                GameEvents.TriggerCurrencyChanged();
                
                // Đóng popup sau 1s
                _root.schedule.Execute(() => _popup.style.display = DisplayStyle.None).ExecuteLater(1000);
            },
            (err) => {
                ToastManager.Instance.Show(err, false);
                _btnClaim.SetEnabled(true);
            }
        );
    }
}