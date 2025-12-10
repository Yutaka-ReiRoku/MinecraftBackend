using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public static class ImageLoader
{
    // Dictionary lưu cache: Key là URL, Value là Texture đã tải
    private static Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

    /// <summary>
    /// Tải ảnh và gán vào control UI <ui:Image>
    /// </summary>
    public static IEnumerator LoadImage(this Image targetImage, string relativeUrl)
    {
        yield return LoadTextureProcess(relativeUrl, (tex) => {
            // Kiểm tra null để tránh lỗi nếu UI đã bị đóng/hủy khi ảnh tải xong
            if (targetImage != null) 
            {
                targetImage.image = tex;
            }
        });
    }

    /// <summary>
    /// Tải ảnh và gán làm hình nền (Background Image) cho bất kỳ VisualElement nào
    /// </summary>
    public static IEnumerator LoadBackgroundImage(this VisualElement element, string relativeUrl)
    {
        yield return LoadTextureProcess(relativeUrl, (tex) => {
            if (element != null) 
            {
                element.style.backgroundImage = new StyleBackground(tex);
            }
        });
    }

    /// <summary>
    /// Logic cốt lõi: Kiểm tra Cache -> Tải từ Web -> Lưu Cache
    /// </summary>
    private static IEnumerator LoadTextureProcess(string relativeUrl, System.Action<Texture2D> onSuccess)
    {
        if (string.IsNullOrEmpty(relativeUrl)) yield break;

        // 1. KIỂM TRA CACHE
        if (_cache.ContainsKey(relativeUrl))
        {
            if (_cache[relativeUrl] != null)
            {
                onSuccess?.Invoke(_cache[relativeUrl]);
                yield break;
            }
            else 
            {
                _cache.Remove(relativeUrl);
            }
        }

        // 2. XÂY DỰNG URL CHUẨN TỪ GAMECONFIG
        // [FIX] Sử dụng GameConfig để lấy IP Server thay vì hardcode
        string fullUrl = GameConfig.GetImageUrl(relativeUrl); 
        
        // Dùng UnityWebRequestTexture tối ưu
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                // Texture pixel art nên để Point Filter cho nét
                texture.filterMode = FilterMode.Point; 
                
                _cache[relativeUrl] = texture;
                onSuccess?.Invoke(texture);
            }
            else
            {
                // 3. FALLBACK
                // Nếu không tải được ảnh chỉ định, thử tải ảnh default
                if (!relativeUrl.Contains("default.png"))
                {
                    yield return LoadTextureProcess("/images/others/default.png", onSuccess);
                }
            }
        }
    }

    public static void ClearCache()
    {
        foreach (var tex in _cache.Values)
        {
            if (tex != null) UnityEngine.Object.Destroy(tex);
        }
        _cache.Clear();
    }
}