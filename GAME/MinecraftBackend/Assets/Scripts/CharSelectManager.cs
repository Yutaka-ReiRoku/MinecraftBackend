using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class CharSelectManager : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument CharSelectDoc;

    private VisualElement _root;
    private ScrollView _charList;
    private Button _btnCreateNew;
    private VisualElement _createPopup;
    private TextField _inputName;
    private Button _btnConfirmCreate;
    private Button _btnCancelCreate;
    private VisualElement _loadingOverlay;
    private VisualElement _modeContainer;

    private string _selectedMode = "Survival";
    private List<string> _availableModes = new List<string> { "Survival", "Creative", "Hardcore" };

    void OnEnable()
    {
        if (CharSelectDoc == null) CharSelectDoc = GetComponent<UIDocument>();
        _root = CharSelectDoc.rootVisualElement;

        _charList = _root.Q<ScrollView>("CharList");
        _btnCreateNew = _root.Q<Button>("BtnCreateNew");
        _createPopup = _root.Q<VisualElement>("CreatePopup");
        _inputName = _root.Q<TextField>("InputCharName");
        _btnConfirmCreate = _root.Q<Button>("BtnConfirmCreate");
        _btnCancelCreate = _root.Q<Button>("BtnCancelCreate");
        _loadingOverlay = _root.Q<VisualElement>("LoadingOverlay");
        _modeContainer = _root.Q<VisualElement>("ModeSelectionContainer");

        // --- [FIX] ÁP DỤNG VÁ LỖI NHẬP LIỆU ---
        if (_inputName != null) _inputName.FixTextFieldInput();
        // ---------------------------------------

        if (_btnCreateNew != null) _btnCreateNew.clicked += ShowCreatePopup;
        if (_btnCancelCreate != null) _btnCancelCreate.clicked += HideCreatePopup;
        if (_btnConfirmCreate != null) _btnConfirmCreate.clicked += OnConfirmCreate;

        HideCreatePopup();
        ToggleLoading(false);
        SetupModeSelection();
        StartCoroutine(LoadCharacters());
    }

    IEnumerator LoadCharacters()
    {
        ToggleLoading(true);
        _charList.Clear();

        yield return NetworkManager.Instance.SendRequest<List<CharacterDto>>("auth/characters", "GET", null,
            (chars) =>
            {
                if (chars.Count == 0) ShowCreatePopup();
                else foreach (var c in chars) CreateCharButton(c);
               
                if (chars.Count >= 3) _btnCreateNew.style.display = DisplayStyle.None;
                else _btnCreateNew.style.display = DisplayStyle.Flex;

                ToggleLoading(false);
            },
            (err) =>
            {
                Debug.LogError("Lỗi tải nhân vật: " + err);
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
        
        btn.style.borderTopWidth = 1; btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth = 1; btn.style.borderRightWidth = 1;
        btn.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
        btn.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);
        btn.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f);
        btn.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f);

        var avatar = new Image();
        avatar.style.width = 60; avatar.style.height = 60;
        avatar.style.marginRight = 20; avatar.style.marginLeft = 10;
        StartCoroutine(avatar.LoadImage(data.AvatarUrl));
        btn.Add(avatar);

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

        btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = new Color(1, 1, 1, 0.1f));
        btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = new Color(0, 0, 0, 0.5f));
        btn.clicked += () => SelectCharacter(data.CharacterID);
        _charList.Add(btn);
    }

    void SelectCharacter(string charId)
    {
        PlayerPrefs.SetString("CurrentCharID", charId);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }

    void ShowCreatePopup()
    {
        if (_createPopup != null)
        {
            _createPopup.style.display = DisplayStyle.Flex;
            _createPopup.style.opacity = 1;
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
            btn.style.backgroundColor = (_selectedMode == mode) ?
                new Color(0, 0.8f, 0.2f) : new Color(0.2f, 0.2f, 0.2f);
            
            btn.clicked += () => {
                _selectedMode = mode;
                SetupModeSelection(); 
            };
            _modeContainer.Add(btn);
        }
    }

    void OnConfirmCreate()
    {
        string name = _inputName.value;
        if (string.IsNullOrEmpty(name)) return;

        ToggleLoading(true);
        var body = new { CharacterName = name, GameMode = _selectedMode };
        StartCoroutine(NetworkManager.Instance.SendRequest<object>("auth/character", "POST", body,
            (res) =>
            {
                Debug.Log("Tạo nhân vật thành công!");
                HideCreatePopup();
                StartCoroutine(LoadCharacters());
            },
            (err) => { Debug.LogError("Lỗi tạo nhân vật: " + err); ToggleLoading(false); }
        ));
    }

    void ToggleLoading(bool isLoading)
    {
        if (_loadingOverlay != null) 
            _loadingOverlay.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
    }
}