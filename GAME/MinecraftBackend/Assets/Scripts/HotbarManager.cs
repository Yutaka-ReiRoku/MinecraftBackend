using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    // Mảng lưu ItemID đang được gán cho từng slot (0-8)
    private string[] _assignedItemIds = new string[9];

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

        // Gán sự kiện click cho các nút trên UI (để hỗ trợ mobile hoặc click chuột)
        for (int i = 0; i < 9; i++)
        {
            int index = i; // Capture biến cho closure
            var btn = _root.Q<Button>($"Hotbar{index + 1}");
            if (btn != null)
            {
                // Xóa text mặc định nếu đã có icon (logic load save nếu cần)
                btn.clicked += () => UseSlot(index);
            }
        }
    }

    void Update()
    {
        // Lắng nghe phím tắt bàn phím (1-9)
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UseSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UseSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UseSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) UseSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) UseSlot(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) UseSlot(8);
    }

    /// <summary>
    /// Được gọi từ DragManipulator khi người chơi thả Item vào ô Hotbar
    /// </summary>
    public void AssignSlot(int index, string itemId, StyleBackground icon)
    {
        if (index < 0 || index >= 9) return;

        // 1. Lưu dữ liệu
        _assignedItemIds[index] = itemId;

        // 2. Cập nhật UI
        var btn = _root.Q<Button>($"Hotbar{index + 1}");
        if (btn != null)
        {
            btn.style.backgroundImage = icon;
            
            // Ẩn số thứ tự hoặc text cũ đi để hiện icon rõ hơn
            var label = btn.Q<Label>(); 
            if (label != null) label.style.display = DisplayStyle.None;

            // Hiệu ứng nảy nhẹ báo hiệu gán thành công
            btn.style.scale = new Scale(Vector3.one * 1.2f);
            btn.schedule.Execute(() => btn.style.scale = new Scale(Vector3.one)).ExecuteLater(150);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("equip");
        ToastManager.Instance.Show($"Đã gán vào phím số {index + 1}", true);
    }

    /// <summary>
    /// Kích hoạt vật phẩm trong slot
    /// </summary>
    public void UseSlot(int index)
    {
        if (index < 0 || index >= 9) return;

        // 1. Hiệu ứng Visual (Sáng viền lên)
        var btn = _root.Q<Button>($"Hotbar{index + 1}");
        if (btn != null)
        {
            // Thêm class highlight (định nghĩa trong USS: border-color: yellow)
            btn.AddToClassList("hotbar-active");
            btn.schedule.Execute(() => btn.RemoveFromClassList("hotbar-active")).ExecuteLater(200);
        }

        // 2. Logic Sử dụng
        string itemId = _assignedItemIds[index];
        if (string.IsNullOrEmpty(itemId)) return;

        // Tìm ShopManager để thực hiện hành động (vì ShopManager nắm giữ logic API)
        // Lưu ý: Hotbar lưu ItemID (Template), ta cần tìm InventoryID thực tế trong kho để dùng
        var shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            // Gọi hàm UseItemByTemplateID bên ShopManager (Cần thêm hàm này vào ShopManager)
            // Hoặc đơn giản là gửi Event toàn cục
            // Ở đây ta giả định ShopManager có hàm hỗ trợ:
            shopManager.UseItemFromHotbar(itemId);
        }
    }
}