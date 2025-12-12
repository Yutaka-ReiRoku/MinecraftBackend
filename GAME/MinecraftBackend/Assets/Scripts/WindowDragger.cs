using UnityEngine;
using UnityEngine.UIElements;

public class WindowDragger : PointerManipulator
{
    private VisualElement _targetHandle; 
    private VisualElement _windowToMove; 
    
    private Vector2 _startMousePos;
    private Vector2 _startWindowPos;
    private bool _isDragging = false;

    
    
    
    
    
    public WindowDragger(VisualElement handle, VisualElement window)
    {
        _targetHandle = handle;
        _windowToMove = window;
        
        
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
        if (evt.button != 0) return; 

        _isDragging = true;
        _targetHandle.CapturePointer(evt.pointerId);

        _startMousePos = evt.position;
        
        
        
        _startWindowPos = new Vector2(_windowToMove.resolvedStyle.left, _windowToMove.resolvedStyle.top);

        
        _windowToMove.style.opacity = 0.8f;
        
        
        _windowToMove.BringToFront();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging || !target.HasPointerCapture(evt.pointerId)) return;

        
        Vector2 delta = (Vector2)evt.position - _startMousePos;

        
        
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
        
        
        if (!_isDragging) return;
        
    }

    private void StopDragging(IPointerEvent evt)
    {
        _isDragging = false;
        _targetHandle.ReleasePointer(evt.pointerId);
        _windowToMove.style.opacity = 1f; 
    }
}