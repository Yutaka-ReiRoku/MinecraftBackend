using UnityEngine;
using UnityEngine.UIElements;

public class ClickEffect : MonoBehaviour
{
    private UIDocument _uiDoc;
    private VisualElement _root;

    [Header("Settings")]
    public float Duration = 0.4f;      
    public float StartSize = 5f;       
    public Color RippleColor = new Color(1f, 1f, 1f, 0.4f); 

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        
        _root = _uiDoc.rootVisualElement;

        
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
        
        
        ripple.style.position = Position.Absolute;
        ripple.style.width = StartSize;
        ripple.style.height = StartSize;
        
        
        ripple.style.left = position.x;
        ripple.style.top = position.y;
        
        
        ripple.style.translate = new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent), 0);
        
        
        
        ripple.style.borderTopLeftRadius = 50;
        ripple.style.borderTopRightRadius = 50;
        ripple.style.borderBottomLeftRadius = 50;
        ripple.style.borderBottomRightRadius = 50;

        ripple.style.backgroundColor = RippleColor;
        ripple.pickingMode = PickingMode.Ignore;
        
        _root.Add(ripple);

        
        
        
        
        ripple.schedule.Execute(() => {
            ripple.style.width = 50; 
            ripple.style.height = 50;
            ripple.style.opacity = 0; 
        });

        
        ripple.schedule.Execute(() => 
        {
            if (_root.Contains(ripple)) _root.Remove(ripple);
        }).ExecuteLater((long)(Duration * 1000));
    }
}