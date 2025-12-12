using UnityEngine;
using UnityEngine.UIElements;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    private UIDocument _uiDoc;
    private VisualElement _root;
    private VisualElement _tooltipContainer;
    
    
    private Label _lblName;
    private Label _lblStats;
    private Label _lblRarity;
    private Label _lblDesc;

    
    private bool _isVisible = false;

    
    public int CurrentWeaponAtk = 0;
    public int CurrentArmorDef = 0;

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

        
        _tooltipContainer = _root.Q<VisualElement>("TooltipContainer");
        if (_tooltipContainer != null)
        {
            _lblName = _tooltipContainer.Q<Label>("TtName");
            _lblStats = _tooltipContainer.Q<Label>("TtStats");
            _lblRarity = _tooltipContainer.Q<Label>("TtRarity");
            _lblDesc = _tooltipContainer.Q<Label>("TtDesc");
            
            
            _tooltipContainer.style.display = DisplayStyle.None;
            
            _tooltipContainer.pickingMode = PickingMode.Ignore;
        }
    }

    void Update()
    {
        if (_tooltipContainer == null || !_isVisible) return;
        
        
        Vector2 mousePos = Input.mousePosition;
        
        
        
        float uiX = mousePos.x + 15; 
        float uiY = Screen.height - mousePos.y + 15; 

        
        float screenW = _root.resolvedStyle.width;
        float screenH = _root.resolvedStyle.height;
        float tipW = _tooltipContainer.resolvedStyle.width;
        float tipH = _tooltipContainer.resolvedStyle.height;
        
        
        if (uiX + tipW > screenW) 
            uiX = mousePos.x - tipW - 15;
        
        
        
        if (uiY + tipH > screenH) 
            uiY = Screen.height - mousePos.y - tipH - 15;
        
        
        
        float curX = _tooltipContainer.resolvedStyle.left;
        float curY = _tooltipContainer.resolvedStyle.top;
        
        
        float newX = Mathf.Lerp(curX, uiX, Time.deltaTime * 15f);
        float newY = Mathf.Lerp(curY, uiY, Time.deltaTime * 15f);

        _tooltipContainer.style.left = newX;
        _tooltipContainer.style.top = newY;
    }

    
    
    
    
    
    
    
    public void Show(string name, string desc, string stats, string rarity)
    {
        if (_tooltipContainer == null) return;
        
        
        if (_lblName != null) _lblName.text = name;
        if (_lblDesc != null) _lblDesc.text = desc;
        if (_lblStats != null) 
        {
            _lblStats.text = stats;
            
            _lblStats.style.display = string.IsNullOrEmpty(stats) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        
        if (_lblRarity != null)
        {
            _lblRarity.text = rarity;
            Color rarityColor = Color.white;
            
            if (!string.IsNullOrEmpty(rarity))
            {
                switch (rarity.ToLower())
                {
                    case "common": rarityColor = new Color(0.7f, 0.7f, 0.7f); break; 
                    case "rare": rarityColor = new Color(0f, 0.7f, 1f); break; 
                    case "epic": rarityColor = new Color(0.7f, 0f, 1f); break; 
                    case "legendary": rarityColor = new Color(1f, 0.8f, 0f); break; 
                }
            }
            
            _lblRarity.style.color = rarityColor;
            if (_lblName != null) _lblName.style.color = rarityColor; 
        }

        
        
        _tooltipContainer.style.display = DisplayStyle.Flex;
        _tooltipContainer.style.opacity = 1;
        
        _isVisible = true;
    }

    
    
    
    public void Hide()
    {
        if (_tooltipContainer == null) return;
        _isVisible = false;
        _tooltipContainer.style.display = DisplayStyle.None;
    }
}