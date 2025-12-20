using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Settings UI")]
    public UIDocument UiDoc;

    // Biến để ShopManager kiểm tra trạng thái
    public bool IsSettingsOpen => _popup != null && _popup.style.display == DisplayStyle.Flex;

    private VisualElement _root;
    private VisualElement _popup;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    
    // Password Fields
    private VisualElement _passChangeArea;
    private TextField _oldPassField;
    private TextField _newPassField;
    private Button _btnConfirmPass;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (UiDoc == null) UiDoc = GetComponent<UIDocument>();
        if (UiDoc == null) UiDoc = FindFirstObjectByType<UIDocument>();

        if (UiDoc != null)
        {
            _root = UiDoc.rootVisualElement;
            InitializeSettingsPopup();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleSettings();
    }

    private void InitializeSettingsPopup()
    {
        _popup = _root.Q<VisualElement>("SettingsPopup");
        if (_popup == null)
        {
            _popup = CreateSettingsPopup();
            _root.Add(_popup);
        }
        
        var btnClose = _popup.Q<Button>("BtnCloseSettings");
        if (btnClose != null) btnClose.clicked += CloseSettings;

        var btnLogout = _popup.Q<Button>("BtnLogout");
        if (btnLogout != null) btnLogout.clicked += Logout;

        _musicSlider = _popup.Q<Slider>("MusicSlider");
        _sfxSlider = _popup.Q<Slider>("SfxSlider");
        
        float savedMusic = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float savedSfx = PlayerPrefs.GetFloat("SfxVol", 1.0f);

        if (_musicSlider != null)
        {
            _musicSlider.value = savedMusic;
            _musicSlider.RegisterValueChangedCallback(evt =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(evt.newValue);
                PlayerPrefs.SetFloat("MusicVol", evt.newValue);
            });
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = savedSfx;
            _sfxSlider.RegisterValueChangedCallback(evt =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(evt.newValue);
                PlayerPrefs.SetFloat("SfxVol", evt.newValue);
            });
        }

        var btnTogglePass = _popup.Q<Button>("BtnChangePass");
        _passChangeArea = _popup.Q<VisualElement>("PassChangeArea");
        
        if (btnTogglePass != null)
        {
            btnTogglePass.clicked += () =>
            {
                if (_passChangeArea != null)
                {
                    bool isHidden = _passChangeArea.style.display == DisplayStyle.None;
                    _passChangeArea.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
                }
            };
        }

        _oldPassField = _popup.Q<TextField>("OldPass");
        _newPassField = _popup.Q<TextField>("NewPass");
        _btnConfirmPass = _popup.Q<Button>("BtnConfirmPass");

        if (_oldPassField != null) _oldPassField.maskChar = '*';
        if (_newPassField != null) _newPassField.maskChar = '*';

        if (_btnConfirmPass != null)
            _btnConfirmPass.clicked += () => StartCoroutine(ChangePasswordProcess());

        _popup.style.display = DisplayStyle.None;
    }

    private VisualElement CreateSettingsPopup()
    {
        var overlay = new VisualElement();
        overlay.name = "SettingsPopup";
        overlay.style.position = Position.Absolute;
        overlay.style.top = 0; overlay.style.bottom = 0;
        overlay.style.left = 0; overlay.style.right = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.85f);
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        var card = new VisualElement();
        card.style.width = 450;
        card.style.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
        card.style.paddingLeft = 30; card.style.paddingRight = 30;
        card.style.paddingTop = 30; card.style.paddingBottom = 30;
        card.style.borderTopLeftRadius = 20; card.style.borderTopRightRadius = 20;
        card.style.borderBottomLeftRadius = 20; card.style.borderBottomRightRadius = 20;

        card.Add(new Label("PAUSE MENU") { style = { fontSize = 28, color = new Color(1, 0.8f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 25, alignSelf = Align.Center } });
        
        card.Add(new Label("Music Volume") { style = { color = Color.gray, marginTop = 10 } });
        var sliderMusic = new Slider(0, 1) { name = "MusicSlider", style = { marginBottom = 15 } };
        card.Add(sliderMusic);

        card.Add(new Label("SFX Volume") { style = { color = Color.gray } });
        var sliderSfx = new Slider(0, 1) { name = "SfxSlider", style = { marginBottom = 25 } };
        card.Add(sliderSfx);

        var btnChangePass = new Button { text = "Change Password", name = "BtnChangePass", style = { height = 45, marginBottom = 15, fontSize = 16, backgroundColor = new Color(0.3f, 0.3f, 0.35f), color = Color.white } };
        card.Add(btnChangePass);

        var passArea = new VisualElement { name = "PassChangeArea", style = { display = DisplayStyle.None, backgroundColor = new Color(0,0,0,0.3f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, marginBottom = 15, borderTopLeftRadius = 10, borderTopRightRadius = 10, borderBottomLeftRadius = 10, borderBottomRightRadius = 10 } };
        
        var oldPass = new TextField { name = "OldPass", maskChar = '*', label = "Old Password" };
        var newPass = new TextField { name = "NewPass", maskChar = '*', label = "New Password" };
        var btnConfirm = new Button { text = "Confirm", name = "BtnConfirmPass", style = { marginTop = 10, backgroundColor = new Color(0, 0.6f, 0), color = Color.white, height = 40 } };

        passArea.Add(oldPass); passArea.Add(newPass); passArea.Add(btnConfirm);
        card.Add(passArea);

        var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 15 } };
        var btnLogout = new Button { text = "LOGOUT", name = "BtnLogout", style = { backgroundColor = new Color(0.7f, 0.1f, 0.1f), color = Color.white, flexGrow = 1, height = 50, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } };
        var btnClose = new Button { text = "RESUME", name = "BtnCloseSettings", style = { flexGrow = 1, height = 50, marginLeft = 15, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } };

        row.Add(btnLogout); row.Add(btnClose);
        card.Add(row);
        overlay.Add(card);
        return overlay;
    }

    public void ToggleSettings()
    {
        if (_popup == null) return;
        if (_popup.style.display == DisplayStyle.Flex) CloseSettings(); else OpenSettings();
    }

    public void OpenSettings()
    {
        if (_popup == null) return;
        _popup.style.display = DisplayStyle.Flex;
        _popup.BringToFront();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    public void CloseSettings()
    {
        if (_popup == null) return;
        _popup.style.display = DisplayStyle.None;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    IEnumerator ChangePasswordProcess()
    {
        string oldPass = _oldPassField.value;
        string newPass = _newPassField.value;

        if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass)) { ToastManager.Instance.Show("Vui lòng nhập đầy đủ thông tin!", false); yield break; }
        if (newPass.Length < 6) { ToastManager.Instance.Show("Mật khẩu mới quá ngắn (>6 ký tự)!", false); yield break; }

        var body = new { OldPassword = oldPass, NewPassword = newPass };
        yield return NetworkManager.Instance.SendRequest<object>("auth/password", "PUT", body,
            (res) => { ToastManager.Instance.Show("Đổi mật khẩu thành công!", true); _oldPassField.value = ""; _newPassField.value = ""; if(_passChangeArea != null) _passChangeArea.style.display = DisplayStyle.None; },
            (err) => ToastManager.Instance.Show("Lỗi: " + err, false)
        );
    }

    void Logout()
    {
        NetworkManager.Instance.ClearSession();
        SceneManager.LoadScene("SplashScene");
    }
}