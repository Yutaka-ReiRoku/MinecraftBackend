using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DragManipulator : PointerManipulator
{
    private VisualElement _target;
    private VisualElement _root;
    private VisualElement _ghostIcon;
    private bool _isDragging = false;
    private VisualElement _lastHoveredSlot;

    public DragManipulator(VisualElement target, VisualElement root)
    {
        this.target = target;
        _target = target;
        _root = root;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        // SECURITY: Chỉ cho phép kéo từ kho đồ (inv-slot)
        bool canDrag = false;
        VisualElement parent = _target.parent;
        while (parent != null)
        {
            if (parent.ClassListContains("item-card")) 
            {
                // Logic check ID để đảm bảo là item thật
                if (_target.userData != null) 
                {
                    canDrag = true;
                    break;
                }
            }
            parent = parent.parent;
        }

        if (!canDrag) return;

        _isDragging = true;
        target.CapturePointer(evt.pointerId);

        // Tạo Ghost Icon
        _ghostIcon = new VisualElement();
        _ghostIcon.style.backgroundImage = _target.resolvedStyle.backgroundImage;
        _ghostIcon.style.width = 50; 
        _ghostIcon.style.height = 50;
        _ghostIcon.style.position = Position.Absolute;
        _ghostIcon.style.left = evt.position.x - 25;
        _ghostIcon.style.top = evt.position.y - 25;
        _ghostIcon.pickingMode = PickingMode.Ignore; 
        _ghostIcon.style.opacity = 0.8f;
        _root.Add(_ghostIcon);
        
        _target.style.opacity = 0.3f;
        if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX("click");
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;

        _ghostIcon.style.left = evt.position.x - 25;
        _ghostIcon.style.top = evt.position.y - 25;

        // UX: Highlight slot
        VisualElement elementUnderMouse = _root.panel.Pick(evt.position);
        VisualElement slot = FindSlotParent(elementUnderMouse);
        
        if (slot != _lastHoveredSlot)
        {
            if (_lastHoveredSlot != null) _lastHoveredSlot.RemoveFromClassList("slot-highlight"); // Cần định nghĩa CSS này nếu muốn đẹp
            if (slot != null) slot.AddToClassList("slot-highlight");
            _lastHoveredSlot = slot;
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_isDragging) return;

        _isDragging = false;
        target.ReleasePointer(evt.pointerId);
        _target.style.opacity = 1;
        _root.Remove(_ghostIcon);

        if (_lastHoveredSlot != null)
        {
            _lastHoveredSlot.RemoveFromClassList("slot-highlight");
            _lastHoveredSlot = null;
        }

        // LOGIC THẢ (DROP)
        VisualElement dropTarget = _root.panel.Pick(evt.position);
        VisualElement slot = FindSlotParent(dropTarget);

        if (slot != null && _target.userData != null)
        {
            string itemId = (string)_target.userData;

            // A. Thả vào ô Trang Bị (Nếu có)
            if (slot.ClassListContains("equip-slot"))
            {
                GameEvents.TriggerEquipRequest(itemId);
                if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX("equip");
            }
            // B. Thả vào Hotbar (FIXED: Gọi đúng HotbarManager)
            else if (slot.ClassListContains("hotbar-slot"))
            {
                string slotName = slot.name; // VD: "Hotbar1"
                // Lấy số cuối cùng
                string numPart = slotName.Replace("Hotbar", "");
                if (int.TryParse(numPart, out int index))
                {
                    // Index 1-9 -> Array 0-8
                    if (HotbarManager.Instance != null)
                    {
                        HotbarManager.Instance.AssignSlot(index - 1, itemId, _target.resolvedStyle.backgroundImage);
                    }
                }
            }
        }
    }

    private VisualElement FindSlotParent(VisualElement element)
    {
        while (element != null)
        {
            if (element.ClassListContains("equip-slot") || element.ClassListContains("hotbar-slot"))
            {
                return element;
            }
            element = element.parent;
        }
        return null;
    }
}