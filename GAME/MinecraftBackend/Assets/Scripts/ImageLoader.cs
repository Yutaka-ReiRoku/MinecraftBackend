using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public static class ImageLoader
{
    
    private static Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

    
    
    
    public static IEnumerator LoadImage(this Image targetImage, string relativeUrl)
    {
        yield return LoadTextureProcess(relativeUrl, (tex) => {
            
            if (targetImage != null) 
            {
                targetImage.image = tex;
            }
        });
    }

    
    
    
    public static IEnumerator LoadBackgroundImage(this VisualElement element, string relativeUrl)
    {
        yield return LoadTextureProcess(relativeUrl, (tex) => {
            if (element != null) 
            {
                element.style.backgroundImage = new StyleBackground(tex);
            }
        });
    }

    
    
    
    private static IEnumerator LoadTextureProcess(string relativeUrl, System.Action<Texture2D> onSuccess)
    {
        if (string.IsNullOrEmpty(relativeUrl)) yield break;

        
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

        
        
        string fullUrl = GameConfig.GetImageUrl(relativeUrl); 
        
        
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                
                texture.filterMode = FilterMode.Point; 
                
                _cache[relativeUrl] = texture;
                onSuccess?.Invoke(texture);
            }
            else
            {
                
                
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