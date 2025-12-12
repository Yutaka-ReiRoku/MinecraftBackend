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
    public GameObject ConfettiPrefab; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
        if (_uiDoc == null) _uiDoc = FindFirstObjectByType<UIDocument>();
        if (_uiDoc != null) _root = _uiDoc.rootVisualElement;
    }

    public void SpawnFloatingText(Vector2 position, string text, Color color, int fontSize = 30)
    {
        
        if (_root == null)
        {
            _uiDoc = FindFirstObjectByType<UIDocument>();
            if (_uiDoc != null) _root = _uiDoc.rootVisualElement;
            else return; 
        }

        
        var label = new Label(text);
        
        
        label.style.position = Position.Absolute;
        label.style.left = position.x;
        label.style.top = position.y;
        
        label.style.fontSize = fontSize;
        label.style.color = color;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.textShadow = new TextShadow { offset = new Vector2(2, 2), blurRadius = 0, color = new Color(0,0,0, 0.5f) };
        
        
        label.style.transitionProperty = new List<StylePropertyName> { 
            new StylePropertyName("top"), 
            new StylePropertyName("opacity"),
            new StylePropertyName("scale")
        };
        label.style.transitionDuration = new List<TimeValue> { new TimeValue(1.0f) }; 

        label.pickingMode = PickingMode.Ignore;

        _root.Add(label);

        
        _root.schedule.Execute(() => {
            label.style.top = position.y - 100; 
            label.style.opacity = 0;            
            label.style.scale = new Scale(new Vector3(1.5f, 1.5f, 1)); 
        }); 

        
        _root.schedule.Execute(() => {
            if(_root.Contains(label)) _root.Remove(label);
        }).ExecuteLater(1200);
    }

    public void ShowDamage(Vector2 pos, int damage, bool isCrit)
    {
        string text = damage.ToString();
        Color color = Color.white;
        int size = 30;

        if (isCrit)
        {
            text += " CRIT!";
            color = new Color(1f, 0.2f, 0.2f); 
            size = 45;
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 5f);
        }

        float offsetX = Random.Range(-20f, 20f);
        float offsetY = Random.Range(-20f, 20f);
        
        SpawnFloatingText(new Vector2(pos.x + offsetX, pos.y + offsetY), text, color, size);
    }

    public void PlayConfetti(Vector3 worldPos)
    {
        if (ConfettiPrefab != null)
        {
            var fx = Instantiate(ConfettiPrefab, worldPos, Quaternion.identity);
            Destroy(fx, 3.0f);
        }
    }
}