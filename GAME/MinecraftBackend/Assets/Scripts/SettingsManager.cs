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
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private VisualElement _passChangeArea;
    private TextField _oldPassField;
    private TextField _newPassField;
    private Button _btnConfirmPass;

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;
        
        // [FIX] Kiểm tra root null
        if (_root == null) return;

        _popup = _root.Q<VisualElement>("SettingsPopup");
        if (_popup == null) return; // Nếu không tìm thấy popup thì dừng, tránh lỗi

        // 1. Setup Audio Sliders
        _musicSlider = _root.Q<Slider>("MusicSlider");
        _sfxSlider = _root.Q<Slider>("SfxSlider");
        
        float savedMusic = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float savedSfx = PlayerPrefs.GetFloat("SfxVol", 1.0f);

        if (_musicSlider != null) {
            _musicSlider.value = savedMusic;
            _musicSlider.RegisterValueChangedCallback(evt => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(evt.newValue);
                PlayerPrefs.SetFloat("MusicVol", evt.newValue);
            });
        }

        if (_sfxSlider != null) {
            _sfxSlider.value = savedSfx;
            _sfxSlider.RegisterValueChangedCallback(evt => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(evt.newValue);
                PlayerPrefs.SetFloat("SfxVol", evt.newValue);
            });
        }

        // 2. Setup Buttons
        var btnOpen = _root.Q<Button>("BtnSettings");
        if (btnOpen != null) btnOpen.clicked += ToggleSettings; 

        var btnClose = _root.Q<Button>("BtnCloseSettings");
        if(btnClose != null) btnClose.clicked += CloseSettings;
        
        var btnLogout = _root.Q<Button>("BtnLogout");
        if(btnLogout != null) btnLogout.clicked += Logout;

        // Change Password UI
        _passChangeArea = _root.Q<VisualElement>("PassChangeArea");
        var btnTogglePass = _root.Q<Button>("BtnChangePass");
        if (btnTogglePass != null) {
            btnTogglePass.clicked += () => {
                if (_passChangeArea != null) {
                    bool isHidden = _passChangeArea.style.display == DisplayStyle.None;
                    _passChangeArea.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
                }
            };
        }

        _oldPassField = _root.Q<TextField>("OldPass");
        _newPassField = _root.Q<TextField>("NewPass");
        _btnConfirmPass = _root.Q<Button>("BtnConfirmPass");
        
        if (_btnConfirmPass != null) {
            _btnConfirmPass.clicked += () => StartCoroutine(ChangePasswordProcess());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleSettings();
    }

    void ToggleSettings()
    {
        if (_popup == null) return;
        bool isVisible = _popup.style.display == DisplayStyle.Flex;
        if (isVisible) CloseSettings(); else OpenSettings();
    }

    void OpenSettings() { _popup.style.display = DisplayStyle.Flex; }
    void CloseSettings() { _popup.style.display = DisplayStyle.None; }

    IEnumerator ChangePasswordProcess()
    {
        string oldPass = _oldPassField.value;
        string newPass = _newPassField.value;

        if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass)) {
            ToastManager.Instance.Show("Vui lòng nhập đầy đủ thông tin!", false);
            yield break;
        }
        if (newPass.Length < 6) {
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