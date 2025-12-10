using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;

public class ProfileManager : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _popup;
    
    // UI Elements - View Mode
    private Label _nameLabel;
    private Label _idLabel;
    private Label _statsAtk;
    private Label _statsDef;
    private VisualElement _avatarContainer;
    
    // UI Elements - Edit Mode
    private VisualElement _infoContainer;
    private VisualElement _updateForm;
    private TextField _editNameField;
    private TextField _editAvatarField;
    private Button _btnEditMode;
    private Button _btnSave;
    private Button _btnCancel;

    // Visual Equipment Layers
    private Image _layerBody;
    private Image _layerHead;
    private Image _layerChest;
    private Image _layerLegs;
    private Image _layerBoots;
    private Image _layerWeapon;
    private Image _layerMount;

    // Avatars mẫu
    private readonly string[] _avatarOptions = new string[] 
    {
        "/images/avatars/steve.png",
        "/images/avatars/alex.png",
        "/images/avatars/zombie.png",
        "/images/avatars/creeper.png"
    };

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        _popup = _root.Q<VisualElement>("ProfilePopup");
        if (_popup == null) return;

        // Query Elements
        _infoContainer = _popup.Q<VisualElement>("InfoContainer");
        _updateForm = _popup.Q<VisualElement>("UpdateForm");
        
        _nameLabel = _popup.Q<Label>("ProfileName");
        _idLabel = _popup.Q<Label>("ProfileID");
        _statsAtk = _popup.Q<Label>("ValAtk");
        _statsDef = _popup.Q<Label>("ValDef");

        // Layers
        _layerBody = _popup.Q<Image>("LayerBody");
        _layerHead = _popup.Q<Image>("LayerHead");
        _layerChest = _popup.Q<Image>("LayerChest");
        _layerLegs = _popup.Q<Image>("LayerLegs");
        _layerBoots = _popup.Q<Image>("LayerBoots");
        _layerWeapon = _popup.Q<Image>("LayerWeapon");
        _layerMount = _popup.Q<Image>("LayerMount");

        // Edit Inputs
        _editNameField = _popup.Q<TextField>("EditName");
        _editAvatarField = _popup.Q<TextField>("EditAvatar");

        // Buttons
        _btnEditMode = _popup.Q<Button>("BtnEditProfile");
        _btnSave = _popup.Q<Button>("BtnSaveProfile");
        _btnCancel = _popup.Q<Button>("BtnCancelEdit");
        var btnClose = _popup.Q<Button>("BtnCloseProfile");

        // Events
        if (btnClose != null) btnClose.clicked += () => _popup.style.display = DisplayStyle.None;
        if (_btnEditMode != null) _btnEditMode.clicked += () => ToggleEditMode(true);
        if (_btnCancel != null) _btnCancel.clicked += () => ToggleEditMode(false);
        if (_btnSave != null) _btnSave.clicked += () => StartCoroutine(UpdateProfileProcess());

        var avatarSelector = _popup.Q<ScrollView>("AvatarSelector");
        if (avatarSelector != null) RenderAvatarSelector(avatarSelector);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TogglePopup();
        }
    }

    void TogglePopup()
    {
        if (_popup.style.display == DisplayStyle.None)
        {
            _popup.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadProfileData());
            _popup.style.scale = new Scale(Vector3.zero);
            _popup.experimental.animation.Start(new StyleValues { scale = new Scale(Vector3.one) }, 200).Ease(Easing.OutBack);
        }
        else
        {
            _popup.style.display = DisplayStyle.None;
        }
    }

    void ToggleEditMode(bool isEditing)
    {
        if (_infoContainer != null) _infoContainer.style.display = isEditing ? DisplayStyle.None : DisplayStyle.Flex;
        if (_updateForm != null) _updateForm.style.display = isEditing ? DisplayStyle.Flex : DisplayStyle.None;
        if (_btnEditMode != null) _btnEditMode.style.display = isEditing ? DisplayStyle.None : DisplayStyle.Flex;
    }

    IEnumerator LoadProfileData()
    {
        // [CHECK] API này là GET, đúng
        yield return NetworkManager.Instance.SendRequest<CharacterDto>("game/profile/me", "GET", null,
            (data) => {
                if (_nameLabel != null) _nameLabel.text = data.CharacterName;
                if (_idLabel != null) _idLabel.text = $"ID: {data.CharacterID}";
                
                if (_editNameField != null) _editNameField.value = data.CharacterName;
                if (_editAvatarField != null) _editAvatarField.value = data.AvatarUrl;

                StartCoroutine(UpdateVisuals(data));

                if (TooltipManager.Instance != null)
                {
                    if (_statsAtk != null) _statsAtk.text = TooltipManager.Instance.CurrentWeaponAtk.ToString();
                    if (_statsDef != null) _statsDef.text = TooltipManager.Instance.CurrentArmorDef.ToString();
                }
            },
            (err) => ToastManager.Instance.Show("Lỗi tải hồ sơ: " + err, false)
        );
    }

    IEnumerator UpdateVisuals(CharacterDto data)
    {
        if (_layerBody != null) yield return _layerBody.LoadImage(data.AvatarUrl);
    }

    IEnumerator UpdateProfileProcess()
    {
        string newName = _editNameField.value;
        string newAvt = _editAvatarField.value;

        var body = new { CharacterName = newName, AvatarUrl = newAvt };
        
        // [FIX QUAN TRỌNG] Đổi "POST" thành "PUT" để khớp với GameApiController
        yield return NetworkManager.Instance.SendRequest<object>("game/profile/update", "PUT", body, 
            (res) => {
                ToastManager.Instance.Show("Cập nhật thành công!", true);
                ToggleEditMode(false);
                StartCoroutine(LoadProfileData()); 
                GameEvents.TriggerRefreshAll(); 
            },
            (err) => ToastManager.Instance.Show("Lỗi cập nhật: " + err, false)
        );
    }

    void RenderAvatarSelector(VisualElement container)
    {
        container.Clear();
        foreach (var url in _avatarOptions)
        {
            var btn = new Button();
            btn.AddToClassList("avatar-option-btn");
            btn.style.width = 50;
            btn.style.height = 50;
            StartCoroutine(btn.LoadBackgroundImage(url));

            btn.clicked += () => {
                if (_editAvatarField != null) _editAvatarField.value = url;
            };
            container.Add(btn);
        }
    }
}