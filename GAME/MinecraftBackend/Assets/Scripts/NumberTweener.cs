using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public static class NumberTweener
{
    
    
    
    
    
    
    
    
    
    public static IEnumerator TweenValue(Label label, int startVal, int endVal, string suffix = "", float duration = 0.5f, string format = "N0")
    {
        if (label == null) yield break;

        float elapsed = 0f;

        
        Color originalColor = label.style.color.value;
        Color targetColor = endVal > startVal ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);
        
        
        label.style.color = targetColor;
        
        label.style.scale = new Scale(Vector3.one * 1.2f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            
            
            
            
            int current = (int)Mathf.Lerp(startVal, endVal, progress);
            
            if (label != null)
                label.text = $"{current.ToString(format)}{suffix}";

            yield return null;
        }

        
        if (label != null)
        {
            label.text = $"{endVal.ToString(format)}{suffix}";
            
            
            label.style.color = originalColor;
            label.style.scale = new Scale(Vector3.one);
        }
    }
    
    
    
    
    public static string Compact(int num)
    {
        if (num >= 1000000) return (num / 1000000D).ToString("0.##") + "M";
        if (num >= 1000) return (num / 1000D).ToString("0.##") + "k";
        return num.ToString("N0");
    }
}