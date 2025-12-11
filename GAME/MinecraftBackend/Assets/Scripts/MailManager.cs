using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MailManager : MonoBehaviour
{
    [System.Serializable]
    public class MailDto
    {
        public int Id;
        public string Title;
        public string Content;
        public string AttachedItemId;
        public string AttachedItemName;
        public int AttachedAmount;
        public bool IsRead;
        public bool IsClaimed;
        public string SentDate;
    }

    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _mailPopup;
    private ScrollView _mailList;
    private VisualElement _mailRedDot;
    private Button _btnOpenMail;

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        _mailPopup = _root.Q<VisualElement>("MailPopup");
        _mailList = _root.Q<ScrollView>("MailList");
        _mailRedDot = _root.Q<VisualElement>("MailRedDot");
        _btnOpenMail = _root.Q<Button>("BtnMail");

        if (_btnOpenMail != null)
        {
            _btnOpenMail.clicked += () => {
                if (_mailPopup.style.display == DisplayStyle.None)
                {
                    _mailPopup.style.display = DisplayStyle.Flex;
                    StartCoroutine(LoadMails());
                }
                else _mailPopup.style.display = DisplayStyle.None;
            };
        }

        var btnClose = _root.Q<Button>("BtnCloseMail");
        if (btnClose != null) btnClose.clicked += () => _mailPopup.style.display = DisplayStyle.None;
        
        StartCoroutine(CheckNewMailRoutine());
    }

    IEnumerator CheckNewMailRoutine()
    {
        while (true)
        {
            yield return LoadMails(true);
            yield return new WaitForSeconds(60f);
        }
    }

    IEnumerator LoadMails(bool checkOnly = false)
    {
        // [FIX] Sửa đường dẫn API thành "game/mail" để khớp với GameApiController
        yield return NetworkManager.Instance.SendRequest<List<MailDto>>("game/mail", "GET", null,
            (mails) => {
                if (mails == null) return;
                int unreadCount = 0;
                foreach (var m in mails)
                {
                    if (!m.IsRead && !m.IsClaimed) unreadCount++;
                }

                if (_mailRedDot != null)
                    _mailRedDot.style.display = unreadCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;

                if (!checkOnly && _mailList != null) RenderMailList(mails);
            },
            (err) => { }
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
            
            // Border radius
            row.style.borderTopLeftRadius = 5; row.style.borderTopRightRadius = 5;
            row.style.borderBottomLeftRadius = 5; row.style.borderBottomRightRadius = 5;
            row.style.borderTopWidth = 1; row.style.borderBottomWidth = 1;
            row.style.borderLeftWidth = 1; row.style.borderRightWidth = 1;

            Color borderColor = mail.IsRead ? new Color(0.5f, 0.5f, 0.5f) : new Color(0, 1f, 1f);
            row.style.borderTopColor = borderColor; row.style.borderBottomColor = borderColor;
            row.style.borderLeftColor = borderColor; row.style.borderRightColor = borderColor;

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

            var contentLbl = new Label(mail.Content);
            contentLbl.style.whiteSpace = WhiteSpace.Normal;
            contentLbl.style.fontSize = 12;
            contentLbl.style.marginBottom = 5;
            row.Add(contentLbl);

            if (!string.IsNullOrEmpty(mail.AttachedItemId) && !mail.IsClaimed)
            {
                var giftBox = new VisualElement();
                giftBox.style.flexDirection = FlexDirection.Row;
                giftBox.style.alignItems = Align.Center;
                giftBox.style.backgroundColor = new Color(1f, 1f, 0, 0.1f);
                giftBox.style.paddingLeft = 5;
                giftBox.style.borderTopLeftRadius = 4; giftBox.style.borderTopRightRadius = 4;
                giftBox.style.borderBottomLeftRadius = 4; giftBox.style.borderBottomRightRadius = 4;

                var giftLbl = new Label($"Quà: {mail.AttachedAmount}x {mail.AttachedItemName ?? "Item"}");
                giftLbl.style.color = Color.yellow;

                var btnClaim = new Button();
                btnClaim.text = "NHẬN";
                btnClaim.AddToClassList("btn-confirm");
                btnClaim.style.height = 25;
                btnClaim.style.marginLeft = StyleKeyword.Auto;
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
        // [FIX] Sửa API endpoint cho Claim
        yield return NetworkManager.Instance.SendRequest<object>($"game/mail/claim/{mailId}", "POST", null,
            (res) => {
                ToastManager.Instance.Show("Đã nhận quà thành công!", true);
                AudioManager.Instance.PlaySFX("success");
                StartCoroutine(LoadMails());
                GameEvents.TriggerRefreshAll();
            },
            (err) => ToastManager.Instance.Show("Lỗi nhận quà: " + err, false)
        );
    }
}