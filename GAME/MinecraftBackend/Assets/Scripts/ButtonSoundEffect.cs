using UnityEngine;
using UnityEngine.UIElements;

public class ButtonSoundEffect : MonoBehaviour
{
    private UIDocument _uiDoc;

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        
        var root = _uiDoc.rootVisualElement;

        
        root.Query<Button>().ForEach(btn => 
        {
            
            btn.clicked += () => 
            {
                if (AudioManager.Instance != null) 
                    AudioManager.Instance.PlaySFX("click");
            };

            
            btn.RegisterCallback<MouseEnterEvent>(evt => 
            {
                
                
            });
        });
    }
}