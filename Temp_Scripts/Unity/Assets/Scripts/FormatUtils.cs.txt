using UnityEngine;

public static class FormatUtils
{
    /// <summary>
    /// Định dạng số với dấu phẩy phân cách hàng nghìn (VD: 1,234,567)
    /// </summary>
    public static string Number(int value)
    {
        return value.ToString("N0");
    }

    /// <summary>
    /// Định dạng số gọn (Compact Notation) cho các vị trí chật hẹp
    /// VD: 1500 -> 1.5K, 2000000 -> 2M
    /// </summary>
    public static string Compact(int value)
    {
        if (value >= 1000000)
        {
            return (value / 1000000f).ToString("0.##") + "M";
        }
        if (value >= 1000)
        {
            return (value / 1000f).ToString("0.##") + "K";
        }
        
        return value.ToString();
    }

    /// <summary>
    /// Định dạng thời gian (Giây -> mm:ss)
    /// Dùng cho Cooldown hoặc Crafting Time
    /// </summary>
    public static string Time(float seconds)
    {
        if (seconds < 60) return $"{Mathf.CeilToInt(seconds)}s";
        
        int min = Mathf.FloorToInt(seconds / 60);
        int sec = Mathf.FloorToInt(seconds % 60);
        return $"{min:00}:{sec:00}";
    }

    /// <summary>
    /// Chuyển đổi độ hiếm sang mã màu Hex để dùng trong Rich Text
    /// </summary>
    public static string RarityToColorHex(string rarity)
    {
        if (string.IsNullOrEmpty(rarity)) return "#FFFFFF"; // Trắng

        switch (rarity.ToLower())
        {
            case "common": return "#AAAAAA";    // Xám
            case "rare": return "#00BFFF";      // Xanh dương
            case "epic": return "#A020F0";      // Tím
            case "legendary": return "#FFA500"; // Cam Vàng
            default: return "#FFFFFF";
        }
    }
}