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

    // Biến công khai để ShopManager kiểm tra (nếu đang mở setting thì không click mua đồ được)
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
        // Tự tìm UIDocument nếu chưa gán
        if (UiDoc == null) UiDoc = GetComponent<UIDocument>();
        if (UiDoc == null) UiDoc = FindFirstObjectByType<UIDocument>();

        if (UiDoc != null)
        {
            _root = UiDoc.rootVisualElement;
            InitializeSettingsPopup();
        }
        else
        {
            Debug.LogError("[SettingsManager] Không tìm thấy UIDocument trong Scene!");
        }
    }

    void Update()
    {
        // Phím tắt ESC để mở/đóng nhanh
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleSettings();
    }

    private void InitializeSettingsPopup()
    {
        // 1. Tìm xem trong UXML đã có popup chưa
        _popup = _root.Q<VisualElement>("SettingsPopup");

        // 2. Nếu chưa có, TỰ TẠO (Code-First) để đảm bảo luôn chạy được
        if (_popup == null)
        {
            _popup = CreateSettingsPopup();
            _root.Add(_popup);
        }
        
        // 3. Gán sự kiện cho các nút trong Popup
        var btnClose = _popup.Q<Button>("BtnCloseSettings");
        if (btnClose != null) btnClose.clicked += CloseSettings;

        var btnLogout = _popup.Q<Button>("BtnLogout");
        if (btnLogout != null) btnLogout.clicked += Logout;

        // 4. Sliders Âm thanh
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

        // 5. Logic Đổi Mật Khẩu
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

        // FIX: Xử lý hiển thị dấu * cho mật khẩu
        // Unity 6/2023+ dùng maskChar, các bản cũ dùng isPasswordField. 
        // Code này ưu tiên maskChar, nếu lỗi bạn có thể đổi thành .isPasswordField = true;
        if (_oldPassField != null) _oldPassField.maskChar = '*';
        if (_newPassField != null) _newPassField.maskChar = '*';

        if (_btnConfirmPass != null)
            _btnConfirmPass.clicked += () => StartCoroutine(ChangePasswordProcess());

        // Mặc định ẩn popup khi game bắt đầu
        _popup.style.display = DisplayStyle.None;
    }

    // --- HÀM TẠO GIAO DIỆN POPUP BẰNG CODE (Không cần vẽ trong UXML) ---
    private VisualElement CreateSettingsPopup()
    {
        // Lớp nền tối (Overlay)
        var overlay = new VisualElement();
        overlay.name = "SettingsPopup";
        overlay.style.position = Position.Absolute;
        overlay.style.top = 0; overlay.style.bottom = 0;
        overlay.style.left = 0; overlay.style.right = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.85f); // Đen mờ 85%
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        // Thẻ chứa nội dung (Card)
        var card = new VisualElement();
        card.style.width = 450;
        card.style.backgroundColor = new Color(0.12f, 0.12f, 0.18f); // Màu tím than đậm
        card.style.paddingLeft = 30; card.style.paddingRight = 30;
        card.style.paddingTop = 30; card.style.paddingBottom = 30;
        
        // Bo góc
        card.style.borderTopLeftRadius = 20; card.style.borderTopRightRadius = 20;
        card.style.borderBottomLeftRadius = 20; card.style.borderBottomRightRadius = 20;

        // Viền sáng nhẹ
        card.style.borderTopWidth = 2; card.style.borderBottomWidth = 2;
        card.style.borderLeftWidth = 2; card.style.borderRightWidth = 2;
        Color borderColor = new Color(1, 1, 1, 0.15f);
        card.style.borderTopColor = borderColor; card.style.borderBottomColor = borderColor;
        card.style.borderLeftColor = borderColor; card.style.borderRightColor = borderColor;

        // Tiêu đề
        var title = new Label("PAUSE MENU");
        title.style.fontSize = 28;
        title.style.color = new Color(1, 0.8f, 0.4f); // Vàng nhạt
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 25;
        title.style.alignSelf = Align.Center;
        card.Add(title);

        // --- Slider Nhạc ---
        var lblMusic = new Label("Music Volume");
        lblMusic.style.color = Color.gray;
        lblMusic.style.marginTop = 10;
        card.Add(lblMusic);
        
        var sliderMusic = new Slider(0, 1);
        sliderMusic.name = "MusicSlider";
        sliderMusic.style.marginBottom = 15;
        card.Add(sliderMusic);

        // --- Slider SFX ---
        var lblSfx = new Label("SFX Volume");
        lblSfx.style.color = Color.gray;
        card.Add(lblSfx);

        var sliderSfx = new Slider(0, 1);
        sliderSfx.name = "SfxSlider";
        sliderSfx.style.marginBottom = 25;
        card.Add(sliderSfx);

        // --- Nút Đổi Mật Khẩu ---
        var btnChangePass = new Button { text = "Change Password", name = "BtnChangePass" };
        btnChangePass.style.height = 45;
        btnChangePass.style.marginBottom = 15;
        btnChangePass.style.fontSize = 16;
        btnChangePass.style.backgroundColor = new Color(0.3f, 0.3f, 0.35f);
        btnChangePass.style.color = Color.white;
        card.Add(btnChangePass);

        // --- Khu vực nhập mật khẩu (Mặc định ẩn) ---
        var passArea = new VisualElement { name = "PassChangeArea" };
        passArea.style.display = DisplayStyle.None;
        passArea.style.backgroundColor = new Color(0,0,0,0.3f);
        passArea.style.paddingLeft = 15; passArea.style.paddingRight = 15;
        passArea.style.paddingTop = 15; passArea.style.paddingBottom = 15;
        passArea.style.marginBottom = 15;
        passArea.style.borderTopLeftRadius = 10; passArea.style.borderTopRightRadius = 10;
        passArea.style.borderBottomLeftRadius = 10; passArea.style.borderBottomRightRadius = 10;

        var oldPass = new TextField { name = "OldPass" }; 
        oldPass.maskChar = '*'; // Ẩn ký tự
        oldPass.label = "Old Password";
        
        var newPass = new TextField { name = "NewPass" }; 
        newPass.maskChar = '*'; // Ẩn ký tự
        newPass.label = "New Password";

        var btnConfirm = new Button { text = "Confirm Change", name = "BtnConfirmPass" };
        btnConfirm.style.marginTop = 10;
        btnConfirm.style.backgroundColor = new Color(0, 0.6f, 0); // Xanh lá
        btnConfirm.style.color = Color.white;
        btnConfirm.style.height = 40;

        passArea.Add(oldPass);
        passArea.Add(newPass);
        passArea.Add(btnConfirm);
        card.Add(passArea);

        // --- Hàng nút cuối (Logout & Resume) ---
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.marginTop = 15;

        var btnLogout = new Button { text = "LOGOUT", name = "BtnLogout" };
        btnLogout.style.backgroundColor = new Color(0.7f, 0.1f, 0.1f); // Đỏ
        btnLogout.style.color = Color.white;
        btnLogout.style.flexGrow = 1;
        btnLogout.style.height = 50;
        btnLogout.style.fontSize = 16;
        btnLogout.style.unityFontStyleAndWeight = FontStyle.Bold;

        var btnClose = new Button { text = "RESUME", name = "BtnCloseSettings" };
        btnClose.style.flexGrow = 1;
        btnClose.style.height = 50;
        btnClose.style.marginLeft = 15;
        btnClose.style.fontSize = 16;
        btnClose.style.unityFontStyleAndWeight = FontStyle.Bold;

        row.Add(btnLogout);
        row.Add(btnClose);
        card.Add(row);

        overlay.Add(card);
        return overlay;
    }

    // --- LOGIC ĐIỀU KHIỂN ---

    public void ToggleSettings()
    {
        if (_popup == null) return;
        bool isVisible = _popup.style.display == DisplayStyle.Flex;
        if (isVisible) CloseSettings();
        else OpenSettings();
    }

    public void OpenSettings()
    {
        if (_popup == null) return;
        _popup.style.display = DisplayStyle.Flex;
        
        // Đưa popup lên trên cùng danh sách hiển thị (đè lên Shop)
        _popup.BringToFront(); 
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    public void CloseSettings()
    {
        if (_popup == null) return;
        _popup.style.display = DisplayStyle.None;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    // --- LOGIC ĐỔI MẬT KHẨU ---
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
            ToastManager.Instance.Show("Mật khẩu mới quá ngắn (>6 ký tự)!", false);
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

    // --- LOGIC ĐĂNG XUẤT ---
    void Logout()
    {
        NetworkManager.Instance.ClearSession();
        SceneManager.LoadScene("SplashScene");
    }
}