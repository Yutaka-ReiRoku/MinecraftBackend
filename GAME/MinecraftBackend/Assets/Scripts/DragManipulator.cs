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
        
        bool canDrag = false;
        VisualElement parent = _target.parent;
        while (parent != null)
        {
            if (parent.ClassListContains("item-card")) 
            {
                
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

        
        VisualElement elementUnderMouse = _root.panel.Pick(evt.position);
        VisualElement slot = FindSlotParent(elementUnderMouse);
        
        if (slot != _lastHoveredSlot)
        {
            if (_lastHoveredSlot != null) _lastHoveredSlot.RemoveFromClassList("slot-highlight"); 
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

        
        VisualElement dropTarget = _root.panel.Pick(evt.position);
        VisualElement slot = FindSlotParent(dropTarget);

        if (slot != null && _target.userData != null)
        {
            string itemId = (string)_target.userData;

            
            if (slot.ClassListContains("equip-slot"))
            {
                GameEvents.TriggerEquipRequest(itemId);
                if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX("equip");
            }
            
            else if (slot.ClassListContains("hotbar-slot"))
            {
                string slotName = slot.name; 
                
                string numPart = slotName.Replace("Hotbar", "");
                if (int.TryParse(numPart, out int index))
                {
                    
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