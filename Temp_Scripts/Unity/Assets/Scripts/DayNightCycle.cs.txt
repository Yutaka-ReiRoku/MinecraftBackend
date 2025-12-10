using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DayNightCycle : MonoBehaviour
{
    [Header("Settings")]
    public float CheckInterval = 60f; // Kiểm tra mỗi phút một lần
    
    // Tên file ảnh trong thư mục /images/modes/ trên Server
    public string DayImage = "survival.png";
    public string NightImage = "hardcore.png"; 

    private UIDocument _uiDoc;
    private VisualElement _backgroundContainer;
    private bool _isNight = false;

    void OnEnable()
    {
        _uiDoc = GetComponent<UIDocument>();
        if (_uiDoc == null) return;
        
        var root = _uiDoc.rootVisualElement;
        // Container chính bao trùm toàn màn hình (thường là .screen-container hoặc #Root)
        _backgroundContainer = root.Q<VisualElement>("ScreenContainer"); 

        // Khởi chạy vòng lặp kiểm tra
        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        while (true)
        {
            UpdateSky();
            yield return new WaitForSeconds(CheckInterval);
        }
    }

    void UpdateSky()
    {
        if (_backgroundContainer == null) return;

        int hour = DateTime.Now.Hour;
        bool nowIsNight = hour < 6 || hour >= 18;

        // Chỉ cập nhật nếu trạng thái thay đổi để tiết kiệm hiệu năng
        if (nowIsNight != _isNight || _backgroundContainer.style.backgroundImage.value.texture == null)
        {
            _isNight = nowIsNight;
            string imgName = _isNight ? NightImage : DayImage;
            
            // Gọi ImageLoader để tải ảnh nền từ Server
            // Lưu ý: Cần đường dẫn đầy đủ từ root server
            string url = $"/images/modes/{imgName}";
            
            StartCoroutine(_backgroundContainer.LoadBackgroundImage(url));
            
            // Cập nhật tông màu chung (Optional)
            // Nếu là đêm, có thể làm tối UI đi một chút
            if (_isNight)
            {
                // _backgroundContainer.style.unityBackgroundImageTintColor = new Color(0.7f, 0.7f, 0.8f);
            }
            else
            {
                // _backgroundContainer.style.unityBackgroundImageTintColor = Color.white;
            }
        }
    }
}