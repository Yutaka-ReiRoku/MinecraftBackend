using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    private VisualElement _root;
    private UIDocument _uiDoc;

    [Header("Particle Prefabs (Optional)")]
    public GameObject ConfettiPrefab; // Kéo Prefab Particle System vào đây nếu muốn dùng 3D

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc != null) _root = _uiDoc.rootVisualElement;
    }

    /// <summary>
    /// Tạo hiệu ứng chữ bay tại vị trí chỉ định trên màn hình
    /// </summary>
    /// <param name="position">Vị trí màn hình (Screen/Panel Coordinates)</param>
    /// <param name="text">Nội dung (VD: "+100G")</param>
    /// <param name="color">Màu chữ</param>
    /// <param name="fontSize">Cỡ chữ (mặc định 30)</param>
    public void SpawnFloatingText(Vector2 position, string text, Color color, int fontSize = 30)
    {
        if (_root == null)
        {
            _uiDoc = GetComponent<UIDocument>();
            if (_uiDoc != null) _root = _uiDoc.rootVisualElement;
            else return;
        }

        // 1. Tạo Label
        var label = new Label(text);
        
        // 2. Style ban đầu
        label.style.position = Position.Absolute;
        // Chỉnh sửa toạ độ: UI Toolkit gốc toạ độ là Top-Left
        // Nếu position truyền vào là MousePosition (Bottom-Left), cần đảo trục Y
        // Tuy nhiên để an toàn, ta giả định position truyền vào đã là toạ độ UI Toolkit
        label.style.left = position.x; 
        label.style.top = position.y;
        
        label.style.fontSize = fontSize;
        label.style.color = color;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        // Text Shadow giả lập (để chữ nổi bật trên nền)
        label.style.textShadow = new TextShadow { offset = new Vector2(2, 2), blurRadius = 0, color = new Color(0,0,0, 0.5f) };
        
        // Thiết lập Animation
        label.style.transitionProperty = new List<StylePropertyName> { 
            new StylePropertyName("top"), 
            new StylePropertyName("opacity"),
            new StylePropertyName("scale")
        };
        label.style.transitionDuration = new List<TimeValue> { new TimeValue(1.0f) }; // Bay trong 1s
        label.style.transitionTimingFunction = new List<EasingFunction> { EasingFunction.OutExpo };
        
        // Không chắn chuột
        label.pickingMode = PickingMode.Ignore;

        _root.Add(label);

        // 3. Chạy Animation (Sau 1 frame để CSS áp dụng)
        _root.schedule.Execute(() => {
            // Bay lên 100px
            label.style.top = position.y - 100;
            // Mờ dần
            label.style.opacity = 0;
            // Phóng to nhẹ rồi biến mất
            label.style.scale = new Scale(new Vector3(1.5f, 1.5f, 1));
        });

        // 4. Dọn dẹp
        _root.schedule.Execute(() => {
            if(_root.Contains(label)) _root.Remove(label);
        }).ExecuteLater(1200); // Xóa sau 1.2s
    }

    /// <summary>
    /// Hiệu ứng sát thương (Dùng cho Battle)
    /// </summary>
    public void ShowDamage(Vector2 pos, int damage, bool isCrit)
    {
        string text = damage.ToString();
        Color color = Color.white;
        int size = 30;

        if (isCrit)
        {
            text += " CRIT!";
            color = new Color(1f, 0.2f, 0.2f); // Đỏ
            size = 45;
            
            // Rung màn hình nếu Crit
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 5f);
        }

        // Spawn ngẫu nhiên quanh vị trí một chút để không chồng nhau
        float offsetX = Random.Range(-20f, 20f);
        float offsetY = Random.Range(-20f, 20f);
        
        SpawnFloatingText(new Vector2(pos.x + offsetX, pos.y + offsetY), text, color, size);
    }

    /// <summary>
    /// Hiệu ứng pháo hoa 3D (Dùng khi Gacha hoặc Lên cấp)
    /// </summary>
    public void PlayConfetti(Vector3 worldPos)
    {
        if (ConfettiPrefab != null)
        {
            var fx = Instantiate(ConfettiPrefab, worldPos, Quaternion.identity);
            Destroy(fx, 3.0f);
        }
    }
}