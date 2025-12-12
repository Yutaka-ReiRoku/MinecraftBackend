using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance;

    private UIDocument _uiDoc;
    private VisualElement _root;
    
    
    private string[] _assignedItemIds = new string[9];

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

        
        for (int i = 0; i < 9; i++)
        {
            int index = i; 
            var btn = _root.Q<Button>($"Hotbar{index + 1}");
            if (btn != null)
            {
                btn.clicked += () => UseSlot(index);
            }
        }
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UseSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UseSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UseSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) UseSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) UseSlot(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) UseSlot(8);
    }

    
    
    
    public void AssignSlot(int index, string itemId, StyleBackground icon)
    {
        if (index < 0 || index >= 9) return;

        
        _assignedItemIds[index] = itemId;

        
        var btn = _root.Q<Button>($"Hotbar{index + 1}");
        if (btn != null)
        {
            btn.style.backgroundImage = icon;
            
            
            var label = btn.Q<Label>();
            if (label != null) label.style.display = DisplayStyle.None;

            
            btn.style.scale = new Scale(Vector3.one * 1.2f);
            btn.schedule.Execute(() => btn.style.scale = new Scale(Vector3.one)).ExecuteLater(150);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("equip");
        ToastManager.Instance.Show($"Đã gán vào phím số {index + 1}", true);
    }

    
    
    
    public void UseSlot(int index)
    {
        if (index < 0 || index >= 9) return;

        
        var btn = _root.Q<Button>($"Hotbar{index + 1}");
        if (btn != null)
        {
            
            btn.AddToClassList("hotbar-active");
            btn.schedule.Execute(() => btn.RemoveFromClassList("hotbar-active")).ExecuteLater(200);
        }

        
        string itemId = _assignedItemIds[index];
        if (string.IsNullOrEmpty(itemId)) return;

        
        
        var shopManager = Object.FindFirstObjectByType<ShopManager>();
        
        if (shopManager != null)
        {
            
            shopManager.UseItemFromHotbar(itemId);
        }
    }
}