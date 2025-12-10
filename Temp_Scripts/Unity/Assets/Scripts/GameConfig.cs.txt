using UnityEngine;

public static class GameConfig
{
    // --- SERVER CONFIGURATION ---

    // [QUAN TRỌNG] Cấu hình địa chỉ máy chủ
    // 1. UNITY_EDITOR / PC: Dùng "localhost" là ổn định nhất.
    // 2. UNITY_ANDROID (Giả lập): Dùng "10.0.2.2" (Đây là IP trỏ về máy tính host của Android Emulator).
    // 3. UNITY_ANDROID (Điện thoại thật): Bạn CẦN thay đổi dòng dưới thành IP LAN của máy tính (VD: "http://192.168.1.5:5000").

#if UNITY_EDITOR || UNITY_STANDALONE
    public const string SERVER_URL = "http://localhost:5000";
#elif UNITY_ANDROID
    // Mặc định cho Giả lập (Emulator). 
    // Nếu cài lên điện thoại thật, hãy đổi thành IP LAN máy tính của bạn (VD: "http://192.168.1.12:5000")
    public const string SERVER_URL = "http://10.0.2.2:5000"; 
#else
    public const string SERVER_URL = "http://localhost:5000";
#endif

    public const string API_PREFIX = "/api";

    // --- RESOURCE PATHS (Khớp với cấu trúc thư mục wwwroot trên Server) ---
    
    public const string PATH_IMAGES = "/images/";
    public const string PATH_AUDIO = "/audio/";
    public const string PATH_AVATARS = "/images/avatars/";

    // --- GAMEPLAY CONSTANTS ---
    
    public const int MAX_INVENTORY_SLOTS_BASE = 20;
    public const int MAX_STAMINA = 100;
    public const int MAX_HUNGER = 20;

    // --- HELPERS ---

    /// <summary>
    /// Trả về đường dẫn API đầy đủ (VD: http://localhost:5000/api/game/shop)
    /// </summary>
    public static string GetApiEndpoint(string endpoint)
    {
        // Xử lý dấu / để tránh trùng lặp
        if (endpoint.StartsWith("/")) endpoint = endpoint.Substring(1);
        return $"{SERVER_URL}{API_PREFIX}/{endpoint}";
    }

    /// <summary>
    /// Trả về đường dẫn ảnh đầy đủ để ImageLoader sử dụng
    /// </summary>
    public static string GetImageUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return $"{SERVER_URL}/images/others/default.png";
        
        // Nếu đã là link tuyệt đối (http...) thì giữ nguyên
        if (relativePath.StartsWith("http")) return relativePath;
        
        // Đảm bảo có dấu / đầu tiên
        if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
        
        return $"{SERVER_URL}{relativePath}";
    }

    /// <summary>
    /// Trả về đường dẫn âm thanh đầy đủ
    /// </summary>
    public static string GetAudioUrl(string filename)
    {
        // Xử lý logic tương tự ảnh
        if (string.IsNullOrEmpty(filename)) return "";
        if (filename.StartsWith("http")) return filename;
        
        return $"{SERVER_URL}{PATH_AUDIO}{filename}";
    }
}