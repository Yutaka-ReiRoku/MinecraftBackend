using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class CharSelectManager : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument CharSelectDoc;

    // UI Elements
    private VisualElement _root;
    private ScrollView _charList;
    private Button _btnCreateNew;
    private VisualElement _createPopup;
    private TextField _inputName;
    private Button _btnConfirmCreate;
    private Button _btnCancelCreate;
    private VisualElement _loadingOverlay;
    private VisualElement _modeContainer;

    // Logic Variables
    private string _selectedMode = "Survival";
    private List<string> _availableModes = new List<string> { "Survival", "Creative", "Hardcore" };

    void OnEnable()
    {
        if (CharSelectDoc == null) CharSelectDoc = GetComponent<UIDocument>();
        _root = CharSelectDoc.rootVisualElement;

        // Query Elements
        _charList = _root.Q<ScrollView>("CharList");
        _btnCreateNew = _root.Q<Button>("BtnCreateNew");
        _createPopup = _root.Q<VisualElement>("CreatePopup");
        _inputName = _root.Q<TextField>("InputCharName");
        _btnConfirmCreate = _root.Q<Button>("BtnConfirmCreate");
        _btnCancelCreate = _root.Q<Button>("BtnCancelCreate");
        _loadingOverlay = _root.Q<VisualElement>("LoadingOverlay");
        _modeContainer = _root.Q<VisualElement>("ModeSelectionContainer");

        // Gắn sự kiện
        if (_btnCreateNew != null) _btnCreateNew.clicked += ShowCreatePopup;
        if (_btnCancelCreate != null) _btnCancelCreate.clicked += HideCreatePopup;
        if (_btnConfirmCreate != null) _btnConfirmCreate.clicked += OnConfirmCreate;

        // Ẩn Popup mặc định
        HideCreatePopup();
        ToggleLoading(false);

        // Setup Mode & Load Data
        SetupModeSelection();
        StartCoroutine(LoadCharacters());
    }

    // --- 1. LOAD CHARACTERS ---

    IEnumerator LoadCharacters()
    {
        ToggleLoading(true);
        _charList.Clear();

        // Gọi đúng API lấy danh sách nhân vật từ AuthController
        yield return NetworkManager.Instance.SendRequest<List<CharacterDto>>("auth/characters", "GET", null,
            (chars) =>
            {
                if (chars.Count == 0)
                {
                    // Chưa có nhân vật -> Mở popup tạo mới ngay
                    ShowCreatePopup();
                }
                else
                {
                    foreach (var c in chars)
                    {
                        CreateCharButton(c);
                    }
                }
                
                // Giới hạn tối đa 3 nhân vật: Ẩn nút tạo nếu đã đủ
                if (chars.Count >= 3) _btnCreateNew.style.display = DisplayStyle.None;
                else _btnCreateNew.style.display = DisplayStyle.Flex;

                ToggleLoading(false);
            },
            (err) =>
            {
                Debug.LogError("Lỗi tải nhân vật: " + err);
                // Nếu lỗi mạng hoặc 404, hiện nút tạo để retry
                _btnCreateNew.style.display = DisplayStyle.Flex;
                ToggleLoading(false);
            }
        );
    }

    void CreateCharButton(CharacterDto data)
    {
        var btn = new Button();
        btn.style.height = 80;
        btn.style.marginBottom = 10;
        btn.style.flexDirection = FlexDirection.Row;
        btn.style.alignItems = Align.Center;
        btn.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        btn.style.borderWidth = 1;
        btn.style.borderColor = new Color(0.5f, 0.5f, 0.5f);

        // Avatar
        var avatar = new Image();
        avatar.style.width = 60; avatar.style.height = 60;
        avatar.style.marginRight = 20;
        avatar.style.marginLeft = 10;
        StartCoroutine(avatar.LoadImage(data.AvatarUrl));
        btn.Add(avatar);

        // Info
        var info = new VisualElement();
        var nameLbl = new Label(data.CharacterName);
        nameLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLbl.style.fontSize = 18;
        nameLbl.style.color = Color.white;
        
        var detailsLbl = new Label($"Lv.{data.Level} | {data.GameMode} | {data.Gold} G");
        detailsLbl.style.color = Color.gray;

        info.Add(nameLbl);
        info.Add(detailsLbl);
        btn.Add(info);

        // Hover Effect
        btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = new Color(1, 1, 1, 0.1f));
        btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = new Color(0, 0, 0, 0.5f));

        // Click Select
        btn.clicked += () => SelectCharacter(data.CharacterID);

        _charList.Add(btn);
    }

    void SelectCharacter(string charId)
    {
        // Lưu ID nhân vật để dùng cho các API Gameplay
        PlayerPrefs.SetString("CurrentCharID", charId);
        PlayerPrefs.Save();
        
        // Vào Game
        SceneManager.LoadScene("GameScene");
    }

    // --- 2. CREATE CHARACTER LOGIC ---

    void ShowCreatePopup()
    {
        if (_createPopup != null)
        {
            _createPopup.style.display = DisplayStyle.Flex;
            _createPopup.style.opacity = 0;
            _createPopup.experimental.animation.Start(new StyleValues { opacity = 0 }, new StyleValues { opacity = 1 }, 200);
        }
    }

    void HideCreatePopup()
    {
        if (_createPopup != null) _createPopup.style.display = DisplayStyle.None;
    }

    void SetupModeSelection()
    {
        if (_modeContainer == null) return;
        _modeContainer.Clear();
        
        foreach (var mode in _availableModes)
        {
            var btn = new Button();
            btn.text = mode;
            btn.style.flexGrow = 1;
            btn.style.height = 40;
            btn.style.marginRight = 5;
            btn.style.backgroundColor = (_selectedMode == mode) ? new Color(0, 0.8f, 0.2f) : new Color(0.2f, 0.2f, 0.2f);
            
            btn.clicked += () => {
                _selectedMode = mode;
                SetupModeSelection(); // Re-render để update màu
            };

            _modeContainer.Add(btn);
        }
    }

    void OnConfirmCreate()
    {
        string name = _inputName.value;
        if (string.IsNullOrEmpty(name)) return;

        ToggleLoading(true);

        // Body phải khớp với CreateCharacterDto ở Backend
        var body = new { CharacterName = name, GameMode = _selectedMode };

        // FIX: Đã sửa endpoint từ "game/player" thành "auth/character"
        StartCoroutine(NetworkManager.Instance.SendRequest<object>("auth/character", "POST", body,
            (res) =>
            {
                Debug.Log("Tạo nhân vật thành công!");
                HideCreatePopup();
                StartCoroutine(LoadCharacters()); // Reload list
            },
            (err) =>
            {
                Debug.LogError("Lỗi tạo nhân vật: " + err);
                ToggleLoading(false);
            }
        ));
    }

    void ToggleLoading(bool isLoading)
    {
        if (_loadingOverlay != null) 
            _loadingOverlay.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
    }
}