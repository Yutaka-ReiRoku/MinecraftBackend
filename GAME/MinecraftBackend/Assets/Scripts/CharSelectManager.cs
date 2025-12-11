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
    
    // Popup Tạo nhân vật
    private VisualElement _createPopup;
    private TextField _inputName;
    private Button _btnConfirmCreate;
    private Button _btnCancelCreate;
    private VisualElement _modeContainer;
    private VisualElement _loadingOverlay;

    private string _selectedMode = "Survival";
    private List<string> _availableModes = new List<string> { "Survival", "Creative", "Hardcore" };

    void OnEnable()
    {
        if (CharSelectDoc == null) CharSelectDoc = GetComponent<UIDocument>();
        if (CharSelectDoc == null) 
        {
            Debug.LogError("Thiếu UIDocument! Hãy gán vào GameObject.");
            return;
        }
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

        // --- Fix Lỗi Nhập Liệu ---
        if (_inputName != null) _inputName.FixTextFieldInput();

        // Bind Events
        if (_btnCreateNew != null) _btnCreateNew.clicked += ShowCreatePopup;
        if (_btnCancelCreate != null) _btnCancelCreate.clicked += HideCreatePopup;
        if (_btnConfirmCreate != null) _btnConfirmCreate.clicked += OnConfirmCreate;

        // Init State
        HideCreatePopup();
        ToggleLoading(false);
        SetupModeSelection();
        
        // Tải dữ liệu ngay khi bật
        StartCoroutine(LoadCharacters());
    }

    IEnumerator LoadCharacters()
    {
        ToggleLoading(true);
        if (_charList != null)
        {
            _charList.Clear();
            _charList.Add(new Label("Loading heroes...") { style = { color = Color.gray, alignSelf = Align.Center } });
        }

        yield return NetworkManager.Instance.SendRequest<List<CharacterDto>>("auth/characters", "GET", null,
            (chars) =>
            {
                if (_charList != null) _charList.Clear();

                if (chars == null || chars.Count == 0)
                {
                    // Chưa có nhân vật -> Mở popup tạo mới ngay
                    ShowCreatePopup();
                }
                else
                {
                    foreach (var c in chars) CreateCharButton(c);
                }

                // Giới hạn 3 nhân vật, ẩn nút tạo nếu đã đủ
                if (_btnCreateNew != null)
                {
                    _btnCreateNew.style.display = (chars != null && chars.Count >= 3) ? DisplayStyle.None : DisplayStyle.Flex;
                }

                ToggleLoading(false);
            },
            (err) =>
            {
                Debug.LogError("Lỗi tải nhân vật: " + err);
                if (_charList != null)
                {
                    _charList.Clear();
                    _charList.Add(new Label("Failed to load data.") { style = { color = Color.red, alignSelf = Align.Center } });
                }
                ToggleLoading(false);
                // Vẫn hiện nút tạo để test nếu cần
                if (_btnCreateNew != null) _btnCreateNew.style.display = DisplayStyle.Flex;
            }
        );
    }

    void CreateCharButton(CharacterDto data)
    {
        if (_charList == null) return;

        // Tạo Button chứa thông tin nhân vật (Style Responsive)
        var btn = new Button();
        btn.AddToClassList("panel-glass"); // Sử dụng class kính mờ có sẵn
        
        // Override một chút style để phù hợp layout danh sách
        btn.style.height = 100;
        btn.style.flexDirection = FlexDirection.Row;
        btn.style.alignItems = Align.Center;
        btn.style.marginBottom = 15;
        btn.style.paddingLeft = 20;
        btn.style.paddingRight = 20;
        // Hiệu ứng hover thủ công vì Button mặc định của Unity đôi khi ghi đè
        btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = new Color(1, 1, 1, 0.15f));
        btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 0.8f)); // Màu gốc của panel-glass

        // 1. Avatar
        var avatar = new Image();
        avatar.style.width = 70; 
        avatar.style.height = 70;
        avatar.style.marginRight = 20;
        // Bo tròn avatar
        avatar.style.borderTopLeftRadius = 35;
        avatar.style.borderTopRightRadius = 35;
        avatar.style.borderBottomLeftRadius = 35;
        avatar.style.borderBottomRightRadius = 35;
        avatar.style.borderRightWidth = 2;
        avatar.style.borderBottomWidth = 2;
        avatar.style.borderRightColor = new Color(0, 0.8f, 1f); // Neon Blue border
        avatar.style.borderBottomColor = new Color(0, 0.8f, 1f);
        
        StartCoroutine(avatar.LoadImage(data.AvatarUrl));
        btn.Add(avatar);

        // 2. Thông tin text (Cột giữa)
        var infoCol = new VisualElement();
        infoCol.style.flexGrow = 1;
        infoCol.style.justifyContent = Justify.Center;

        var nameLbl = new Label(data.CharacterName);
        nameLbl.style.fontSize = 22;
        nameLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLbl.style.color = new Color(1f, 0.84f, 0f); // Gold color
        
        var detailLbl = new Label($"Lv.{data.Level}  •  {data.GameMode}");
        detailLbl.style.fontSize = 14;
        detailLbl.style.color = new Color(0.8f, 0.8f, 0.8f);

        infoCol.Add(nameLbl);
        infoCol.Add(detailLbl);
        btn.Add(infoCol);

        // 3. Mũi tên chỉ dẫn (Icon Play)
        var playIcon = new Label("▶");
        playIcon.style.fontSize = 20;
        playIcon.style.color = new Color(0, 0.8f, 1f);
        btn.Add(playIcon);

        // Sự kiện Click -> Vào game
        btn.clicked += () => SelectCharacter(data.CharacterID);
        
        _charList.Add(btn);
    }

    void SelectCharacter(string charId)
    {
        // Lưu ID nhân vật hiện tại để các scene sau biết đang chơi ai
        PlayerPrefs.SetString("CurrentCharID", charId);
        PlayerPrefs.Save();
        
        // Chuyển cảnh
        SceneManager.LoadScene("GameScene");
    }

    // --- POPUP LOGIC ---

    void ShowCreatePopup()
    {
        if (_createPopup != null)
        {
            _createPopup.style.display = DisplayStyle.Flex;
            SetupModeSelection(); // Reset mode về mặc định
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
            btn.AddToClassList("btn-secondary"); // Dùng style chung
            btn.style.flexGrow = 1;
            btn.style.height = 40;
            
            // Highlight mode đang chọn
            if (_selectedMode == mode)
            {
                btn.style.backgroundColor = new Color(0, 0.8f, 1f, 0.3f); // Xanh sáng
                
                // [FIX LỖI CS1061]: Thay vì btn.style.borderColor = ..., ta set từng cạnh
                Color highlightColor = new Color(0, 0.8f, 1f);
                btn.style.borderTopColor = highlightColor;
                btn.style.borderBottomColor = highlightColor;
                btn.style.borderLeftColor = highlightColor;
                btn.style.borderRightColor = highlightColor;
                
                btn.style.color = Color.white;
            }
            
            btn.clicked += () => {
                _selectedMode = mode;
                SetupModeSelection(); // Re-render để cập nhật highlight
            };
            _modeContainer.Add(btn);
        }
    }

    void OnConfirmCreate()
    {
        string name = _inputName.value;
        if (string.IsNullOrEmpty(name)) 
        {
            // Có thể thêm Toast báo lỗi ở đây
            return;
        }

        ToggleLoading(true);
        var body = new CreateCharacterDto { CharacterName = name, GameMode = _selectedMode };
        
        StartCoroutine(NetworkManager.Instance.SendRequest<object>("auth/character", "POST", body,
            (res) =>
            {
                Debug.Log("Tạo nhân vật thành công!");
                HideCreatePopup();
                StartCoroutine(LoadCharacters()); // Reload danh sách
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