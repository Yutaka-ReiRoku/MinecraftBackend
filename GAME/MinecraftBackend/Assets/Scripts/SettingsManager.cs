using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _popup;
    
    // Sliders
    private Slider _musicSlider;
    private Slider _sfxSlider;

    // Password Change
    private VisualElement _passChangeArea;
    private TextField _oldPassField;
    private TextField _newPassField;
    private Button _btnConfirmPass;

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        _popup = _root.Q<VisualElement>("SettingsPopup");
        if (_popup == null) return;

        // 1. Setup Audio Sliders
        _musicSlider = _root.Q<Slider>("MusicSlider");
        _sfxSlider = _root.Q<Slider>("SfxSlider");

        float savedMusic = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float savedSfx = PlayerPrefs.GetFloat("SfxVol", 1.0f);
        
        if (_musicSlider != null)
        {
            _musicSlider.value = savedMusic;
            _musicSlider.RegisterValueChangedCallback(evt => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(evt.newValue);
                PlayerPrefs.SetFloat("MusicVol", evt.newValue);
            });
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = savedSfx;
            _sfxSlider.RegisterValueChangedCallback(evt => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(evt.newValue);
                PlayerPrefs.SetFloat("SfxVol", evt.newValue);
            });
        }

        // 2. Setup Buttons
        var btnOpen = _root.Q<Button>("BtnSettings");
        if (btnOpen != null) btnOpen.clicked += ToggleSettings; // Đổi thành Toggle

        _root.Q<Button>("BtnCloseSettings").clicked += CloseSettings;
        _root.Q<Button>("BtnLogout").clicked += Logout;
        _root.Q<Button>("BtnExit").clicked += ExitGame;

        // Change Password UI
        _passChangeArea = _root.Q<VisualElement>("PassChangeArea");
        var btnTogglePass = _root.Q<Button>("BtnChangePass");
        if (btnTogglePass != null)
        {
            btnTogglePass.clicked += () => {
                bool isHidden = _passChangeArea.style.display == DisplayStyle.None;
                _passChangeArea.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
            };
        }

        _oldPassField = _root.Q<TextField>("OldPass");
        _newPassField = _root.Q<TextField>("NewPass");
        _btnConfirmPass = _root.Q<Button>("BtnConfirmPass");
        
        if (_btnConfirmPass != null)
        {
            _btnConfirmPass.clicked += () => StartCoroutine(ChangePasswordProcess());
        }

        _root.Q<Button>("BtnAbout").clicked += ShowAboutInfo;
    }

    // [CẬP NHẬT] Thêm Update loop để bắt sự kiện phím ESC
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    void ToggleSettings()
    {
        if (_popup == null) return;
        
        bool isVisible = _popup.style.display == DisplayStyle.Flex;
        if (isVisible)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    void OpenSettings()
    {
        _popup.style.display = DisplayStyle.Flex;
        // Animation pop-in
        _popup.style.scale = new Scale(Vector3.zero);
        _popup.experimental.animation.Start(new StyleValues { scale = new Scale(Vector3.one) }, 200).Ease(Easing.OutBack);
    }

    void CloseSettings()
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
        
        // Gọi API Put Password (đã có trong AuthController)
        yield return NetworkManager.Instance.SendRequest<object>("auth/password", "PUT", body,
            (res) => {
                ToastManager.Instance.Show("Đổi mật khẩu thành công!", true);
                _oldPassField.value = "";
                _newPassField.value = "";
                _passChangeArea.style.display = DisplayStyle.None;
            },
            (err) => ToastManager.Instance.Show("Lỗi: " + err, false)
        );
    }

    void Logout()
    {
        NetworkManager.Instance.ClearSession();
        SceneManager.LoadScene("SplashScene"); // Về lại Splash
    }

    void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void ShowAboutInfo()
    {
        ToastManager.Instance.Show("Minecraft RPG Backend v1.0", true);
    }
}