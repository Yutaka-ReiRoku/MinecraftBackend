using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public static class NumberTweener
{
    /// <summary>
    /// Coroutine chạy hiệu ứng số tăng/giảm dần trên Label
    /// </summary>
    /// <param name="label">Label UI cần cập nhật</param>
    /// <param name="startVal">Giá trị bắt đầu</param>
    /// <param name="endVal">Giá trị kết thúc</param>
    /// <param name="suffix">Đuôi (VD: " G", " HP")</param>
    /// <param name="duration">Thời gian chạy (giây)</param>
    /// <param name="format">Định dạng số (VD: "N0" để có dấu phẩy 1,000)</param>
    public static IEnumerator TweenValue(Label label, int startVal, int endVal, string suffix = "", float duration = 0.5f, string format = "N0")
    {
        if (label == null) yield break;

        float elapsed = 0f;

        // Xác định màu sắc chỉ số thay đổi (Xanh tăng / Đỏ giảm)
        Color originalColor = label.style.color.value;
        Color targetColor = endVal > startVal ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);
        
        // Hiệu ứng nháy màu
        label.style.color = targetColor;
        // Phóng to nhẹ
        label.style.scale = new Scale(Vector3.one * 1.2f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Sử dụng đường cong EaseOutQuad cho tự nhiên (nhanh đầu, chậm đuôi)
            // float t = progress * (2 - progress); 
            
            // Lerp giá trị
            int current = (int)Mathf.Lerp(startVal, endVal, progress);
            
            if (label != null)
                label.text = $"{current.ToString(format)}{suffix}";

            yield return null;
        }

        // Chốt giá trị cuối cùng chính xác
        if (label != null)
        {
            label.text = $"{endVal.ToString(format)}{suffix}";
            
            // Trả về màu và kích thước cũ
            label.style.color = originalColor;
            label.style.scale = new Scale(Vector3.one);
        }
    }
    
    /// <summary>
    /// Hàm tiện ích định dạng số ngắn gọn (1.5k, 1M)
    /// </summary>
    public static string Compact(int num)
    {
        if (num >= 1000000) return (num / 1000000D).ToString("0.##") + "M";
        if (num >= 1000) return (num / 1000D).ToString("0.##") + "k";
        return num.ToString("N0");
    }
}