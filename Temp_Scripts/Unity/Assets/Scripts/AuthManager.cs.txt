using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument AuthDoc;

    // UI Elements
    private VisualElement _root;
    
    // Containers
    private VisualElement _loginContainer;
    private VisualElement _registerContainer;

    // Inputs (Login)
    private TextField _loginEmail;
    private TextField _loginPass;
    private Button _btnLogin;
    private Button _btnGotoRegister;

    // Inputs (Register)
    private TextField _regUser;
    private TextField _regEmail;
    private TextField _regPass;
    private TextField _regConfirmPass;
    private Button _btnRegister;
    private Button _btnGotoLogin;
    
    // Status
    private Label _statusLabel;
    private VisualElement _loadingOverlay;

    void OnEnable()
    {
        if (AuthDoc == null) AuthDoc = GetComponent<UIDocument>();
        _root = AuthDoc.rootVisualElement;

        // 1. Query Elements
        _loginContainer = _root.Q<VisualElement>("LoginContainer");
        _registerContainer = _root.Q<VisualElement>("RegisterContainer");
        _loadingOverlay = _root.Q<VisualElement>("LoadingOverlay");
        _statusLabel = _root.Q<Label>("StatusLabel");

        // Login Form
        _loginEmail = _root.Q<TextField>("LoginEmail");
        _loginPass = _root.Q<TextField>("LoginPass");
        _btnLogin = _root.Q<Button>("BtnLoginSubmit");
        _btnGotoRegister = _root.Q<Button>("BtnSwitchToReg");

        // Register Form
        _regUser = _root.Q<TextField>("RegUsername");
        _regEmail = _root.Q<TextField>("RegEmail");
        _regPass = _root.Q<TextField>("RegPass");
        _regConfirmPass = _root.Q<TextField>("RegConfirmPass");
        _btnRegister = _root.Q<Button>("BtnRegisterSubmit");
        _btnGotoLogin = _root.Q<Button>("BtnSwitchToLogin");

        // 2. Bind Events
        if (_btnLogin != null) _btnLogin.clicked += OnLoginClicked;
        if (_btnGotoRegister != null) _btnGotoRegister.clicked += () => SwitchMode(false);
        
        if (_btnRegister != null) _btnRegister.clicked += OnRegisterClicked;
        if (_btnGotoLogin != null) _btnGotoLogin.clicked += () => SwitchMode(true);

        // 3. Init State (Mặc định hiện Login)
        SwitchMode(true);
        ToggleLoading(false);
    }

    void SwitchMode(bool isLogin)
    {
        if (_loginContainer != null) 
        {
            _loginContainer.style.display = isLogin ? DisplayStyle.Flex : DisplayStyle.None;
            // Animation fade in
            if(isLogin) _loginContainer.experimental.animation.Start(new StyleValues { opacity = 0f }, new StyleValues { opacity = 1f }, 300);
        }

        if (_registerContainer != null) 
        {
            _registerContainer.style.display = !isLogin ? DisplayStyle.Flex : DisplayStyle.None;
            if(!isLogin) _registerContainer.experimental.animation.Start(new StyleValues { opacity = 0f }, new StyleValues { opacity = 1f }, 300);
        }
        
        if (_statusLabel != null) _statusLabel.text = "";
    }

    void ToggleLoading(bool isLoading)
    {
        if (_loadingOverlay != null)
            _loadingOverlay.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void SetStatus(string msg, bool isError)
    {
        if (_statusLabel == null) return;
        _statusLabel.text = msg;
        _statusLabel.style.color = isError ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.4f);
    }

    // --- LOGIC ĐĂNG NHẬP ---

    void OnLoginClicked()
    {
        string email = _loginEmail.value;
        string pass = _loginPass.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetStatus("Vui lòng nhập đầy đủ thông tin.", true);
            return;
        }

        ToggleLoading(true);

        var body = new LoginRequest { Email = email, Password = pass };

        StartCoroutine(NetworkManager.Instance.SendRequest<TokenResponse>("auth/login", "POST", body,
            (res) => {
                Debug.Log("Login Success! Token: " + res.Token);
                
                // 1. Lưu Token vào PlayerPrefs
                NetworkManager.Instance.SetToken(res.Token);

                // 2. Chuyển sang màn hình chọn nhân vật
                ToggleLoading(false);
                SceneManager.LoadScene("CharSelectScene");
            },
            (err) => {
                ToggleLoading(false);
                SetStatus(err, true);
                Debug.LogError("Login Failed: " + err);
            }
        ));
    }

    // --- LOGIC ĐĂNG KÝ ---

    void OnRegisterClicked()
    {
        string user = _regUser.value;
        string email = _regEmail.value;
        string pass = _regPass.value;
        string confirm = _regConfirmPass.value;

        // Validate cơ bản
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetStatus("Không được để trống.", true);
            return;
        }

        if (pass != confirm)
        {
            SetStatus("Mật khẩu xác nhận không khớp.", true);
            return;
        }

        if (pass.Length < 6)
        {
            SetStatus("Mật khẩu phải dài hơn 6 ký tự.", true);
            return;
        }

        ToggleLoading(true);

        var body = new RegisterRequest { Username = user, Email = email, Password = pass };

        StartCoroutine(NetworkManager.Instance.SendRequest<object>("auth/register", "POST", body,
            (res) => {
                ToggleLoading(false);
                SetStatus("Đăng ký thành công! Hãy đăng nhập.", false);
                
                // Chuyển về tab login sau 1.5s
                StartCoroutine(DelaySwitchToLogin());
            },
            (err) => {
                ToggleLoading(false);
                SetStatus(err, true);
            }
        ));
    }

    IEnumerator DelaySwitchToLogin()
    {
        yield return new WaitForSeconds(1.5f);
        SwitchMode(true);
        // Tự điền email giúp user
        _loginEmail.value = _regEmail.value;
    }
}