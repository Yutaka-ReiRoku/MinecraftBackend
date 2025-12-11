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

        // --- [FIX] ÁP DỤNG VÁ LỖI NHẬP LIỆU ---
        if (_loginEmail != null) _loginEmail.FixTextFieldInput();
        if (_loginPass != null) _loginPass.FixTextFieldInput();
        if (_regUser != null) _regUser.FixTextFieldInput();
        if (_regEmail != null) _regEmail.FixTextFieldInput();
        if (_regPass != null) _regPass.FixTextFieldInput();
        if (_regConfirmPass != null) _regConfirmPass.FixTextFieldInput();
        // ---------------------------------------

        // 2. Bind Events
        if (_btnLogin != null) _btnLogin.clicked += OnLoginClicked;
        if (_btnGotoRegister != null) _btnGotoRegister.clicked += () => SwitchMode(false);
        
        if (_btnRegister != null) _btnRegister.clicked += OnRegisterClicked;
        if (_btnGotoLogin != null) _btnGotoLogin.clicked += () => SwitchMode(true);

        // 3. Init State
        SwitchMode(true);
        ToggleLoading(false);
    }

    void SwitchMode(bool isLogin)
    {
        if (_loginContainer != null) 
            _loginContainer.style.display = isLogin ? DisplayStyle.Flex : DisplayStyle.None;

        if (_registerContainer != null) 
            _registerContainer.style.display = !isLogin ? DisplayStyle.Flex : DisplayStyle.None;
        
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
                NetworkManager.Instance.SetToken(res.Token);
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

    void OnRegisterClicked()
    {
        string user = _regUser.value;
        string email = _regEmail.value;
        string pass = _regPass.value;
        string confirm = _regConfirmPass.value;

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
        _loginEmail.value = _regEmail.value;
    }
}