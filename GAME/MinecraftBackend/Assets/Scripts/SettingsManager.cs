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
            InitializeSettingsUI();
        }
        else
        {
            Debug.LogError("[SettingsManager] Không tìm thấy UIDocument!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleSettings();
    }

    private void InitializeSettingsUI()
    {
        // 1. Tìm hoặc Tạo Nút Settings
        var btnOpen = _root.Q<Button>("BtnSettings");
        if (btnOpen == null)
        {
            btnOpen = CreateSettingsButton();
            _root.Add(btnOpen);
        }
        btnOpen.clicked += ToggleSettings;

        // 2. Tìm hoặc Tạo Popup Settings
        _popup = _root.Q<VisualElement>("SettingsPopup");
        if (_popup == null)
        {
            _popup = CreateSettingsPopup();
            _root.Add(_popup);
        }
        
        // 3. Gán sự kiện
        var btnClose = _popup.Q<Button>("BtnCloseSettings");
        if (btnClose != null) btnClose.clicked += CloseSettings;

        var btnLogout = _popup.Q<Button>("BtnLogout");
        if (btnLogout != null) btnLogout.clicked += Logout;

        // Sliders
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

        // Logic Đổi Mật Khẩu
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

        // FIX: Unity 6 dùng 'maskChar' thay vì 'isPassword' hoặc 'maskCharacter'
        if (_oldPassField != null) _oldPassField.maskChar = '*';
        if (_newPassField != null) _newPassField.maskChar = '*';

        if (_btnConfirmPass != null)
            _btnConfirmPass.clicked += () => StartCoroutine(ChangePasswordProcess());

        _popup.style.display = DisplayStyle.None;
    }

    // --- CÁC HÀM TẠO UI CODE-FIRST ---
    
    private Button CreateSettingsButton()
    {
        var btn = new Button();
        btn.name = "BtnSettings";
        btn.text = "⚙";
        btn.style.position = Position.Absolute;
        btn.style.top = 20;
        btn.style.right = 20;
        btn.style.width = 50;
        btn.style.height = 50;
        btn.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        btn.style.color = Color.white;
        btn.style.fontSize = 30;

        // Style rõ ràng để tránh lỗi phiên bản
        btn.style.borderTopLeftRadius = 10;
        btn.style.borderTopRightRadius = 10;
        btn.style.borderBottomLeftRadius = 10;
        btn.style.borderBottomRightRadius = 10;

        btn.style.borderTopWidth = 1;
        btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth = 1;
        btn.style.borderRightWidth = 1;

        Color borderColor = new Color(1, 1, 1, 0.2f);
        btn.style.borderTopColor = borderColor;
        btn.style.borderBottomColor = borderColor;
        btn.style.borderLeftColor = borderColor;
        btn.style.borderRightColor = borderColor;

        return btn;
    }

    private VisualElement CreateSettingsPopup()
    {
        var overlay = new VisualElement();
        overlay.name = "SettingsPopup";
        overlay.style.position = Position.Absolute;
        overlay.style.width = Length.Percent(100);
        overlay.style.height = Length.Percent(100);
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.85f);
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        var card = new VisualElement();
        card.style.width = 400;
        card.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        card.style.paddingLeft = 20; card.style.paddingRight = 20;
        card.style.paddingTop = 20; card.style.paddingBottom = 20;
        
        card.style.borderTopLeftRadius = 15; card.style.borderTopRightRadius = 15;
        card.style.borderBottomLeftRadius = 15; card.style.borderBottomRightRadius = 15;

        card.style.borderTopWidth = 1;
        card.style.borderBottomWidth = 1;
        card.style.borderLeftWidth = 1;
        card.style.borderRightWidth = 1;

        Color cardBorderColor = new Color(1, 1, 1, 0.1f);
        card.style.borderTopColor = cardBorderColor;
        card.style.borderBottomColor = cardBorderColor;
        card.style.borderLeftColor = cardBorderColor;
        card.style.borderRightColor = cardBorderColor;

        // Header
        var title = new Label("SETTINGS / PAUSE");
        title.style.fontSize = 24;
        title.style.color = Color.white;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 20;
        title.style.alignSelf = Align.Center;
        card.Add(title);

        // Sliders
        var lblMusic = new Label("Music Volume");
        lblMusic.style.color = Color.gray;
        card.Add(lblMusic);
        
        var sliderMusic = new Slider(0, 1);
        sliderMusic.name = "MusicSlider";
        sliderMusic.style.marginBottom = 15;
        card.Add(sliderMusic);

        var lblSfx = new Label("SFX Volume");
        lblSfx.style.color = Color.gray;
        card.Add(lblSfx);

        var sliderSfx = new Slider(0, 1);
        sliderSfx.name = "SfxSlider";
        sliderSfx.style.marginBottom = 20;
        card.Add(sliderSfx);

        // Change Password Button
        var btnChangePass = new Button { text = "Change Password", name = "BtnChangePass" };
        btnChangePass.style.height = 40;
        btnChangePass.style.marginBottom = 10;
        card.Add(btnChangePass);

        // Password Area
        var passArea = new VisualElement { name = "PassChangeArea" };
        passArea.style.display = DisplayStyle.None;
        passArea.style.backgroundColor = new Color(0,0,0,0.3f);
        passArea.style.paddingLeft = 10; passArea.style.paddingRight = 10;
        passArea.style.paddingTop = 10; passArea.style.paddingBottom = 10;
        passArea.style.marginBottom = 10;
        
        passArea.style.borderTopLeftRadius = 5; passArea.style.borderTopRightRadius = 5;
        passArea.style.borderBottomLeftRadius = 5; passArea.style.borderBottomRightRadius = 5;

        // FIX: maskChar
        var oldPass = new TextField { name = "OldPass" }; 
        oldPass.maskChar = '*'; 
        oldPass.label = "Old Password";
        
        var newPass = new TextField { name = "NewPass" }; 
        newPass.maskChar = '*'; 
        newPass.label = "New Password";

        var btnConfirm = new Button { text = "Confirm Change", name = "BtnConfirmPass" };
        btnConfirm.style.marginTop = 10;
        btnConfirm.style.backgroundColor = new Color(0, 0.5f, 0);
        btnConfirm.style.color = Color.white;

        passArea.Add(oldPass);
        passArea.Add(newPass);
        passArea.Add(btnConfirm);
        card.Add(passArea);

        // Logout & Close Buttons
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.marginTop = 10;

        var btnLogout = new Button { text = "LOGOUT", name = "BtnLogout" };
        btnLogout.style.backgroundColor = new Color(0.6f, 0, 0);
        btnLogout.style.color = Color.white;
        btnLogout.style.flexGrow = 1;
        btnLogout.style.height = 45;

        var btnClose = new Button { text = "CLOSE", name = "BtnCloseSettings" };
        btnClose.style.flexGrow = 1;
        btnClose.style.height = 45;
        btnClose.style.marginLeft = 10;

        row.Add(btnLogout);
        row.Add(btnClose);
        card.Add(row);

        overlay.Add(card);
        return overlay;
    }

    // --- LOGIC ---

    public void ToggleSettings()
    {
        if (_popup == null) return;
        bool isVisible = _popup.style.display == DisplayStyle.Flex;
        if (isVisible) CloseSettings();
        else OpenSettings();
    }

    public void OpenSettings()
    {
        _popup.style.display = DisplayStyle.Flex;
    }

    public void CloseSettings()
    {
        _popup.style.display = DisplayStyle.None;
    }

    IEnumerator ChangePasswordProcess()
    {
        string oldPass = _oldPassField.value;
        string newPass = _newPassField.value;

        if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
        {
            ToastManager.Instance.Show("Vui lòng nhập đầy đủ thông tin!", false);
            yield break;
        }
        if (newPass.Length < 6)
        {
            ToastManager.Instance.Show("Mật khẩu mới quá ngắn!", false);
            yield break;
        }

        var body = new { OldPassword = oldPass, NewPassword = newPass };
        yield return NetworkManager.Instance.SendRequest<object>("auth/password", "PUT", body,
            (res) => {
                ToastManager.Instance.Show("Đổi mật khẩu thành công!", true);
                _oldPassField.value = "";
                _newPassField.value = "";
                if(_passChangeArea != null) _passChangeArea.style.display = DisplayStyle.None;
            },
            (err) => ToastManager.Instance.Show("Lỗi: " + err, false)
        );
    }

    void Logout()
    {
        NetworkManager.Instance.ClearSession();
        SceneManager.LoadScene("SplashScene");
    }
}