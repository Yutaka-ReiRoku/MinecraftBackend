using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class OrbManager : MonoBehaviour
{
    public static OrbManager Instance;

    private VisualElement _root;
    private VisualElement _expBarTarget; 

    [Header("Settings")]
    public float FlySpeed = 1.5f; 
    public string OrbImageUrl = "/images/others/exp.png";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc != null)
        {
            _root = uiDoc.rootVisualElement;
            
            
            _expBarTarget = _root.Q<VisualElement>("ExpBar");
        }
    }

    
    
    
    
    
    public void SpawnOrbs(Vector2 startPos, int amount)
    {
        if (_root == null || _expBarTarget == null) return;

        
        
        int orbCount = Mathf.Clamp(amount / 5, 1, 20);

        for (int i = 0; i < orbCount; i++)
        {
            
            StartCoroutine(FlyOrbProcess(startPos, i * 0.1f)); 
        }
    }

    private IEnumerator FlyOrbProcess(Vector2 startPos, float delay)
    {
        
        yield return new WaitForSeconds(delay);

        
        var orb = new Image();
        orb.style.width = 20; 
        orb.style.height = 20;
        orb.style.position = Position.Absolute;
        
        
        float offsetX = Random.Range(-50f, 50f);
        float offsetY = Random.Range(-50f, 50f);
        
        
        
        orb.style.left = startPos.x + offsetX; 
        orb.style.top = startPos.y + offsetY;
        
        
        orb.pickingMode = PickingMode.Ignore;

        
        StartCoroutine(orb.LoadImage(OrbImageUrl));

        _root.Add(orb);

        
        float t = 0;
        Vector2 p0 = new Vector2(orb.style.left.value.value, orb.style.top.value.value);
        
        while (t < 1)
        {
            t += Time.deltaTime * FlySpeed;

            
            
            Vector2 targetPos = _expBarTarget.worldBound.center;

            
            
            Vector2 currentPos = Vector2.Lerp(p0, targetPos, t * t);

            orb.style.left = currentPos.x;
            orb.style.top = currentPos.y;

            yield return null;
        }

        
        if (AudioManager.Instance != null) 
        {
            
            
            
            AudioManager.Instance.PlaySFX("click"); 
        }

        _root.Remove(orb); 
    }
}