using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MailManager : MonoBehaviour
{
    // DTO nhận dữ liệu từ API
    [System.Serializable]
    public class MailDto
    {
        public int Id;
        public string Title;
        public string Content;
        public string AttachedItemId; // ID vật phẩm đính kèm
        public string AttachedItemName; // Tên vật phẩm (Backend cần join để lấy)
        public int AttachedAmount;
        public bool IsRead;
        public bool IsClaimed;
        public string SentDate;
    }

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // UI Elements
    private VisualElement _mailPopup;
    private ScrollView _mailList;
    private VisualElement _mailRedDot;
    private Button _btnOpenMail;

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        // Query UI
        _mailPopup = _root.Q<VisualElement>("MailPopup"); // Popup hiển thị danh sách thư
        _mailList = _root.Q<ScrollView>("MailList"); // List view chứa các thư
        _mailRedDot = _root.Q<VisualElement>("MailRedDot"); // Chấm đỏ báo thư mới
        _btnOpenMail = _root.Q<Button>("BtnMail"); // Nút mở hòm thư trên HUD

        // Gắn sự kiện
        if (_btnOpenMail != null)
        {
            _btnOpenMail.clicked += () => {
                if (_mailPopup.style.display == DisplayStyle.None)
                {
                    _mailPopup.style.display = DisplayStyle.Flex;
                    StartCoroutine(LoadMails()); // Tải lại danh sách khi mở
                }
                else
                {
                    _mailPopup.style.display = DisplayStyle.None;
                }
            };
        }

        var btnClose = _root.Q<Button>("BtnCloseMail");
        if (btnClose != null) btnClose.clicked += () => _mailPopup.style.display = DisplayStyle.None;

        // Kiểm tra thư mới định kỳ (Polling)
        StartCoroutine(CheckNewMailRoutine());
    }

    IEnumerator CheckNewMailRoutine()
    {
        while (true)
        {
            yield return LoadMails(true); // Chỉ tải để check số lượng chưa đọc
            yield return new WaitForSeconds(60f); // Check mỗi 1 phút
        }
    }

    IEnumerator LoadMails(bool checkOnly = false)
    {
        // Gọi API lấy danh sách thư
        yield return NetworkManager.Instance.SendRequest<List<MailDto>>("mail", "GET", null,
            (mails) => {
                int unreadCount = 0;
                foreach (var m in mails)
                {
                    if (!m.IsRead && !m.IsClaimed) unreadCount++;
                }

                // Cập nhật chấm đỏ
                if (_mailRedDot != null)
                    _mailRedDot.style.display = unreadCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;

                // Nếu không phải chỉ check nền -> Render danh sách ra UI
                if (!checkOnly && _mailList != null)
                {
                    RenderMailList(mails);
                }
            },
            (err) => {
                // Debug.LogWarning("Không tải được thư: " + err);
            }
        );
    }

    void RenderMailList(List<MailDto> mails)
    {
        _mailList.Clear();
        
        if (mails.Count == 0)
        {
            _mailList.Add(new Label("Hòm thư trống.") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 20 } });
            return;
        }

        foreach (var mail in mails)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Column;
            row.style.backgroundColor = new Color(0, 0, 0, 0.3f);
            row.style.marginBottom = 5;
            row.style.paddingLeft = 10; row.style.paddingRight = 10;
            row.style.paddingTop = 5; row.style.paddingBottom = 5;
            row.style.borderRadius = 5;
            row.style.borderWidth = 1;
            row.style.borderColor = mail.IsRead ? new Color(0.5f, 0.5f, 0.5f) : new Color(0, 1f, 1f); // Xanh cyan nếu chưa đọc

            // Header: Tiêu đề + Ngày
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            
            var titleLbl = new Label(mail.Title);
            titleLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLbl.style.color = mail.IsRead ? Color.gray : Color.white;
            
            var dateLbl = new Label(mail.SentDate);
            dateLbl.style.fontSize = 10;
            dateLbl.style.color = Color.gray;

            header.Add(titleLbl);
            header.Add(dateLbl);
            row.Add(header);

            // Content: Nội dung
            var contentLbl = new Label(mail.Content);
            contentLbl.style.whiteSpace = WhiteSpace.Normal;
            contentLbl.style.fontSize = 12;
            contentLbl.style.marginBottom = 5;
            row.Add(contentLbl);

            // Attachment: Quà đính kèm (nếu có và chưa nhận)
            if (!string.IsNullOrEmpty(mail.AttachedItemId) && !mail.IsClaimed)
            {
                var giftBox = new VisualElement();
                giftBox.style.flexDirection = FlexDirection.Row;
                giftBox.style.alignItems = Align.Center;
                giftBox.style.backgroundColor = new Color(1f, 1f, 0, 0.1f);
                giftBox.style.paddingLeft = 5;
                giftBox.style.borderRadius = 4;

                var giftLbl = new Label($"Quà: {mail.AttachedAmount}x {mail.AttachedItemName ?? "Item"}"); // Backend cần trả về tên item
                giftLbl.style.color = Color.yellow;
                
                var btnClaim = new Button();
                btnClaim.text = "NHẬN";
                btnClaim.AddToClassList("btn-confirm");
                btnClaim.style.height = 25;
                btnClaim.style.marginLeft = 'auto'; // Đẩy sang phải cùng

                btnClaim.clicked += () => StartCoroutine(ClaimMail(mail.Id));

                giftBox.Add(giftLbl);
                giftBox.Add(btnClaim);
                row.Add(giftBox);
            }
            else if (mail.IsClaimed)
            {
                var claimedLbl = new Label("Đã nhận quà");
                claimedLbl.style.color = Color.green;
                claimedLbl.style.fontSize = 10;
                claimedLbl.style.alignSelf = Align.FlexEnd;
                row.Add(claimedLbl);
            }

            _mailList.Add(row);
        }
    }

    IEnumerator ClaimMail(int mailId)
    {
        yield return NetworkManager.Instance.SendRequest<object>($"mail/claim/{mailId}", "POST", null,
            (res) => {
                ToastManager.Instance.Show("Đã nhận quà thành công!", true);
                AudioManager.Instance.PlaySFX("success");
                
                // Reload lại danh sách thư để cập nhật trạng thái "Đã nhận"
                StartCoroutine(LoadMails());
                
                // Reload kho đồ và tiền
                GameEvents.TriggerRefreshAll();
            },
            (err) => ToastManager.Instance.Show("Lỗi nhận quà: " + err, false)
        );
    }
}