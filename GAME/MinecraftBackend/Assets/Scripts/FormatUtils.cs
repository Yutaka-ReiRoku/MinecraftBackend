using UnityEngine;

public static class FormatUtils
{
    
    
    
    public static string Number(int value)
    {
        return value.ToString("N0");
    }

    
    
    
    
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

    
    
    
    
    public static string Time(float seconds)
    {
        if (seconds < 60) return $"{Mathf.CeilToInt(seconds)}s";
        
        int min = Mathf.FloorToInt(seconds / 60);
        int sec = Mathf.FloorToInt(seconds % 60);
        return $"{min:00}:{sec:00}";
    }

    
    
    
    public static string RarityToColorHex(string rarity)
    {
        if (string.IsNullOrEmpty(rarity)) return "#FFFFFF"; 

        switch (rarity.ToLower())
        {
            case "common": return "#AAAAAA";    
            case "rare": return "#00BFFF";      
            case "epic": return "#A020F0";      
            case "legendary": return "#FFA500"; 
            default: return "#FFFFFF";
        }
    }
}