using UnityEngine;
using UnityEngine.UIElements;

public class ClickEffect : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;

    [Header("Settings")]
    public float Duration = 0.4f;      // Thời gian hiệu ứng
    public float StartSize = 5f;       // Kích thước ban đầu
    public float EndSize = 100f;       // Kích thước kết thúc
    public Color RippleColor = new Color(1f, 1f, 1f, 0.4f); // Màu trắng mờ

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        
        _root = _uiDoc.rootVisualElement;

        // Đăng ký sự kiện click toàn cục lên phần tử gốc (Root)
        // Trick: RegisterCallback với TrickleDown để bắt sự kiện trước khi nó đến các nút con
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

        // Bo tròn thành hình tròn
        ripple.style.borderTopLeftRadius = new Length(50, LengthUnit.Percent);
        ripple.style.borderTopRightRadius = new Length(50, LengthUnit.Percent);
        ripple.style.borderBottomLeftRadius = new Length(50, LengthUnit.Percent);
        ripple.style.borderBottomRightRadius = new Length(50, LengthUnit.Percent);

        ripple.style.backgroundColor = RippleColor;
        
        // QUAN TRỌNG: PickingMode.Ignore để click xuyên qua, không chặn thao tác nút bên dưới
        ripple.pickingMode = PickingMode.Ignore;

        _root.Add(ripple);

        // 2. Chạy Animation (Scale to & Fade out)
        // Sử dụng hệ thống Animation thử nghiệm của UI Toolkit (rất mượt)
        ripple.experimental.animation
            .Start(
                new StyleValues { width = StartSize, height = StartSize, opacity = 1f }, 
                new StyleValues { width = EndSize, height = EndSize, opacity = 0f }, 
                (int)(Duration * 1000) // ms
            )
            .Ease(Easing.OutSine) // Hiệu ứng lướt nhẹ
            .OnCompleted(() => 
            {
                // Dọn dẹp sau khi xong
                if (_root.Contains(ripple)) _root.Remove(ripple);
            });
    }
}