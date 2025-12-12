using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DayNightCycle : MonoBehaviour
{
    [Header("Settings")]
    public float CheckInterval = 60f; 
    
    
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
        
        _backgroundContainer = root.Q<VisualElement>("ScreenContainer"); 

        
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

        
        if (nowIsNight != _isNight || _backgroundContainer.style.backgroundImage.value.texture == null)
        {
            _isNight = nowIsNight;
            string imgName = _isNight ? NightImage : DayImage;
            
            
            
            string url = $"/images/modes/{imgName}";
            
            StartCoroutine(_backgroundContainer.LoadBackgroundImage(url));
            
            
            
            if (_isNight)
            {
                
            }
            else
            {
                
            }
        }
    }
}