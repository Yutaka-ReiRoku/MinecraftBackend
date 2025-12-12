using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    private string _token;

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
            _token = PlayerPrefs.GetString("JWT", null); 
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    public void SetToken(string token) 
    { 
        _token = token; 
        PlayerPrefs.SetString("JWT", token); 
        PlayerPrefs.Save(); 
    }
    
    public void ClearSession() 
    { 
        _token = null; 
        PlayerPrefs.DeleteKey("JWT"); 
        PlayerPrefs.DeleteKey("CurrentCharID"); 
        PlayerPrefs.Save(); 
        ImageLoader.ClearCache(); 
    }

    public IEnumerator SendRequest<T>(string endpoint, string method, object body, Action<T> onSuccess, Action<string> onError)
    {
        string url = GameConfig.GetApiEndpoint(endpoint);
        using (UnityWebRequest www = new UnityWebRequest(url, method))
        {
            
            if (body != null)
            {
                string jsonBody = JsonConvert.SerializeObject(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }

            
            www.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(_token)) 
                www.SetRequestHeader("Authorization", "Bearer " + _token);
            
            string charId = PlayerPrefs.GetString("CurrentCharID", "");
            if (!string.IsNullOrEmpty(charId)) 
                www.SetRequestHeader("X-Character-ID", charId);

            
            yield return www.SendWebRequest();

            
            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                try
                {
                    
                    if (typeof(T) == typeof(object))
                    {
                        onSuccess?.Invoke(default);
                    }
                    else
                    {
                        T data = JsonConvert.DeserializeObject<T>(jsonResponse);
                        onSuccess?.Invoke(data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSON Error] {ex.Message} \n Raw: {jsonResponse}");
                    
                    if (typeof(T) == typeof(object)) onSuccess?.Invoke(default); 
                    else onError?.Invoke("Lỗi xử lý dữ liệu từ máy chủ.");
                }
            }
            else
            {
                
                string errorMsg = www.downloadHandler.text; 
                
                
                if (string.IsNullOrEmpty(errorMsg)) errorMsg = www.error;

                
                try 
                {
                    
                    var errorObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorMsg);
                    if (errorObj != null)
                    {
                        if (errorObj.ContainsKey("message")) errorMsg = errorObj["message"];
                        else if (errorObj.ContainsKey("Message")) errorMsg = errorObj["Message"];
                        else if (errorObj.ContainsKey("title")) errorMsg = errorObj["title"]; 
                    }
                }
                catch 
                { 
                    
                    
                }

                
                if (www.responseCode == 401)
                {
                    Debug.LogWarning("Token expired.");
                    GameEvents.TriggerSessionExpired();
                    onError?.Invoke("Phiên đăng nhập hết hạn.");
                }
                else
                {
                    Debug.LogError($"[API Error] {url}: {errorMsg}");
                    
                    onError?.Invoke(errorMsg); 
                }
            }
        }
    }
}