using UnityEngine;
using UnityEngine.UIElements;

public class WindowDragger : PointerManipulator
{
    private VisualElement _targetHandle; // Phần tử dùng để cầm kéo (thường là Header/Title)
    private VisualElement _windowToMove; // Cửa sổ sẽ di chuyển
    
    private Vector2 _startMousePos;
    private Vector2 _startWindowPos;
    private bool _isDragging = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="handle">Vùng nhận sự kiện kéo (VD: Thanh tiêu đề)</param>
    /// <param name="window">Đối tượng cửa sổ cần di chuyển</param>
    public WindowDragger(VisualElement handle, VisualElement window)
    {
        _targetHandle = handle;
        _windowToMove = window;
        
        // Kích hoạt manipulator
        target = handle;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerOutEvent>(OnPointerOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerOutEvent>(OnPointerOut);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0) return; // Chỉ nhận chuột trái

        _isDragging = true;
        _targetHandle.CapturePointer(evt.pointerId);

        _startMousePos = evt.position;
        
        // Lưu vị trí hiện tại của cửa sổ
        // Sử dụng resolvedStyle để lấy vị trí thực tế sau khi layout
        _startWindowPos = new Vector2(_windowToMove.resolvedStyle.left, _windowToMove.resolvedStyle.top);

        // Hiệu ứng visual khi đang kéo (mờ đi chút)
        _windowToMove.style.opacity = 0.8f;
        
        // Đưa lên trên cùng
        _windowToMove.BringToFront();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging || !target.HasPointerCapture(evt.pointerId)) return;

        // Tính toán độ lệch (Delta)
        Vector2 delta = (Vector2)evt.position - _startMousePos;

        // Cập nhật vị trí mới
        // Lưu ý: Cần đảm bảo window có style position: Absolute
        _windowToMove.style.left = _startWindowPos.x + delta.x;
        _windowToMove.style.top = _startWindowPos.y + delta.y;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_isDragging) return;
        StopDragging(evt);
    }
    
    private void OnPointerOut(PointerOutEvent evt)
    {
        // Tùy chọn: Nếu kéo quá nhanh ra khỏi vùng handle thì có thể stop hoặc giữ capture
        // Với CapturePointer thì thường không cần stop ở đây, nhưng để an toàn:
        if (!_isDragging) return;
        // StopDragging(evt); // Uncomment nếu muốn thả chuột khi ra ngoài
    }

    private void StopDragging(IPointerEvent evt)
    {
        _isDragging = false;
        _targetHandle.ReleasePointer(evt.pointerId);
        _windowToMove.style.opacity = 1f; // Trả lại độ đậm
    }
}