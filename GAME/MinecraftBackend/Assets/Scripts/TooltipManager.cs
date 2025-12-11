using UnityEngine;
using UnityEngine.UIElements;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _tooltipContainer;
    
    // Các label con
    private Label _lblName;
    private Label _lblStats;
    private Label _lblRarity;
    private Label _lblDesc;

    // Biến cho logic di chuyển mượt (Lerp)
    private bool _isVisible = false;

    // Biến lưu chỉ số nhân vật để so sánh (nếu cần mở rộng sau này)
    public int CurrentWeaponAtk = 0;
    public int CurrentArmorDef = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        _root = _uiDoc.rootVisualElement;

        // Query các phần tử trong UXML (MainLayout.uxml)
        _tooltipContainer = _root.Q<VisualElement>("TooltipContainer");
        if (_tooltipContainer != null)
        {
            _lblName = _tooltipContainer.Q<Label>("TtName");
            _lblStats = _tooltipContainer.Q<Label>("TtStats");
            _lblRarity = _tooltipContainer.Q<Label>("TtRarity");
            _lblDesc = _tooltipContainer.Q<Label>("TtDesc");
            
            // Mặc định ẩn
            _tooltipContainer.style.display = DisplayStyle.None;
            // Đảm bảo không chặn chuột
            _tooltipContainer.pickingMode = PickingMode.Ignore;
        }
    }

    void Update()
    {
        if (_tooltipContainer == null || !_isVisible) return;
        
        // 1. Lấy vị trí chuột (Toạ độ màn hình: Bottom-Left là 0,0)
        Vector2 mousePos = Input.mousePosition;
        
        // 2. Chuyển đổi sang toạ độ UI Toolkit (Top-Left là 0,0)
        // Lưu ý: Panel có thể scale khác màn hình thật, nhưng thường root.layout trùng screen
        float uiX = mousePos.x + 15; // Cách chuột 15px sang phải
        float uiY = Screen.height - mousePos.y + 15; // Cách chuột 15px xuống dưới

        // 3. Logic "Clamping" (Chống tràn màn hình)
        float screenW = _root.resolvedStyle.width;
        float screenH = _root.resolvedStyle.height;
        float tipW = _tooltipContainer.resolvedStyle.width;
        float tipH = _tooltipContainer.resolvedStyle.height;
        
        // Nếu tràn bên phải -> Đẩy sang trái chuột
        if (uiX + tipW > screenW) 
            uiX = mousePos.x - tipW - 15;
        
        // Nếu tràn xuống dưới -> Đẩy lên trên chuột
        // (Do trục Y đảo ngược: giá trị càng lớn là càng xuống dưới)
        if (uiY + tipH > screenH) 
            uiY = Screen.height - mousePos.y - tipH - 15;
        
        // 4. Di chuyển mượt (Lerp)
        // Lấy vị trí hiện tại
        float curX = _tooltipContainer.resolvedStyle.left;
        float curY = _tooltipContainer.resolvedStyle.top;
        
        // Nội suy từ vị trí cũ sang vị trí mới với tốc độ 15
        float newX = Mathf.Lerp(curX, uiX, Time.deltaTime * 15f);
        float newY = Mathf.Lerp(curY, uiY, Time.deltaTime * 15f);

        _tooltipContainer.style.left = newX;
        _tooltipContainer.style.top = newY;
    }

    /// <summary>
    /// Hiển thị Tooltip với nội dung chi tiết
    /// </summary>
    /// <param name="name">Tên vật phẩm</param>
    /// <param name="desc">Mô tả</param>
    /// <param name="stats">Thông số (VD: "ATK: 10")</param>
    /// <param name="rarity">Độ hiếm (để đổi màu)</param>
    public void Show(string name, string desc, string stats, string rarity)
    {
        if (_tooltipContainer == null) return;
        
        // Cập nhật nội dung
        if (_lblName != null) _lblName.text = name;
        if (_lblDesc != null) _lblDesc.text = desc;
        if (_lblStats != null) 
        {
            _lblStats.text = stats;
            // Ẩn nếu không có chỉ số
            _lblStats.style.display = string.IsNullOrEmpty(stats) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // Cập nhật màu sắc theo độ hiếm
        if (_lblRarity != null)
        {
            _lblRarity.text = rarity;
            Color rarityColor = Color.white;
            
            if (!string.IsNullOrEmpty(rarity))
            {
                switch (rarity.ToLower())
                {
                    case "common": rarityColor = new Color(0.7f, 0.7f, 0.7f); break; // Xám
                    case "rare": rarityColor = new Color(0f, 0.7f, 1f); break; // Xanh dương
                    case "epic": rarityColor = new Color(0.7f, 0f, 1f); break; // Tím
                    case "legendary": rarityColor = new Color(1f, 0.8f, 0f); break; // Vàng Cam
                }
            }
            
            _lblRarity.style.color = rarityColor;
            if (_lblName != null) _lblName.style.color = rarityColor; // Đổi màu tên luôn cho đẹp
        }

        // [FIXED] Xóa bỏ đoạn mã animation gây lỗi (StyleValues)
        // Thay vào đó chỉ cần hiển thị container và đảm bảo opacity là 1
        _tooltipContainer.style.display = DisplayStyle.Flex;
        _tooltipContainer.style.opacity = 1;
        
        _isVisible = true;
    }

    /// <summary>
    /// Ẩn Tooltip
    /// </summary>
    public void Hide()
    {
        if (_tooltipContainer == null) return;
        _isVisible = false;
        _tooltipContainer.style.display = DisplayStyle.None;
    }
}