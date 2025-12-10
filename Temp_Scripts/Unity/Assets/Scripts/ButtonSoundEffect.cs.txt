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

        // Query tất cả các phần tử là Button trong cây UI
        root.Query<Button>().ForEach(btn => 
        {
            // Gắn sự kiện Click
            btn.clicked += () => 
            {
                if (AudioManager.Instance != null) 
                    AudioManager.Instance.PlaySFX("click");
            };

            // Gắn sự kiện Hover (Lướt chuột qua) - Tạo cảm giác phản hồi cao cấp
            btn.RegisterCallback<MouseEnterEvent>(evt => 
            {
                // Âm thanh "tít" nhẹ khi lướt qua (nếu có file hover.mp3)
                // if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("hover");
            });
        });
    }
}