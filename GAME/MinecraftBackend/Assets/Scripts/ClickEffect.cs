using UnityEngine;
using UnityEngine.UIElements;

public class ClickEffect : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;

    [Header("Settings")]
    public float Duration = 0.4f;      // Thời gian hiệu ứng
    public float StartSize = 5f;       // Kích thước ban đầu
    public Color RippleColor = new Color(1f, 1f, 1f, 0.4f); // Màu trắng mờ

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        
        _root = _uiDoc.rootVisualElement;

        // Đăng ký sự kiện click toàn cục lên phần tử gốc (Root)
        _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.NoTrickleDown);
    }

    void OnDisable()
    {
        if (_root != null)
        {
            _root.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        }
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        CreateRipple(evt.position);
    }

    private void CreateRipple(Vector2 position)
    {
        var ripple = new VisualElement();
        
        // 1. Thiết lập Style
        ripple.style.position = Position.Absolute;
        ripple.style.width = StartSize;
        ripple.style.height = StartSize;
        
        // Đặt vị trí tại điểm click
        ripple.style.left = position.x;
        ripple.style.top = position.y;
        
        // Dịch chuyển về tâm (anchor center)
        ripple.style.translate = new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent), 0);
        
        // [FIXED] Sửa lỗi StyleValues/Easing bằng cách bỏ animation phức tạp
        // Thay vào đó dùng style tĩnh bo tròn
        ripple.style.borderTopLeftRadius = 50;
        ripple.style.borderTopRightRadius = 50;
        ripple.style.borderBottomLeftRadius = 50;
        ripple.style.borderBottomRightRadius = 50;

        ripple.style.backgroundColor = RippleColor;
        ripple.pickingMode = PickingMode.Ignore;
        
        _root.Add(ripple);

        // 2. Logic đơn giản: Phóng to nhẹ và xóa sau thời gian Duration
        // Không dùng experimental.animation nữa
        
        // Fake animation bằng Schedule (Scale to)
        ripple.schedule.Execute(() => {
            ripple.style.width = 50; 
            ripple.style.height = 50;
            ripple.style.opacity = 0; // Mờ dần (nếu có transition css)
        });

        // Xóa khỏi UI sau khi xong
        ripple.schedule.Execute(() => 
        {
            if (_root.Contains(ripple)) _root.Remove(ripple);
        }).ExecuteLater((long)(Duration * 1000));
    }
}