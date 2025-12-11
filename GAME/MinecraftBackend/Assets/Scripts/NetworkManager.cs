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
            // 1. Setup Body
            if (body != null)
            {
                string jsonBody = JsonConvert.SerializeObject(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }

            // 2. Setup Headers
            www.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(_token)) 
                www.SetRequestHeader("Authorization", "Bearer " + _token);
            
            string charId = PlayerPrefs.GetString("CurrentCharID", "");
            if (!string.IsNullOrEmpty(charId)) 
                www.SetRequestHeader("X-Character-ID", charId);

            // 3. Send
            yield return www.SendWebRequest();

            // 4. Handle Response
            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                try
                {
                    // Nếu T là object (không quan tâm kết quả trả về), gọi success luôn
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
                    // Vẫn gọi success nếu request thành công nhưng không parse được (để tránh treo game)
                    if (typeof(T) == typeof(object)) onSuccess?.Invoke(default); 
                    else onError?.Invoke("Lỗi xử lý dữ liệu từ máy chủ.");
                }
            }
            else
            {
                // [FIX QUAN TRỌNG] Ưu tiên lấy thông báo lỗi chi tiết từ Server (vd: "Not enough Gold!")
                string errorMsg = www.downloadHandler.text; 
                
                // Nếu text thô rỗng, mới dùng lỗi mặc định của HTTP (vd: "400 Bad Request")
                if (string.IsNullOrEmpty(errorMsg)) errorMsg = www.error;

                // Cố gắng parse JSON nếu server trả về object lỗi (VD: { "message": "..." })
                try 
                {
                    // Backend .NET đôi khi trả về object lỗi chuẩn
                    var errorObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorMsg);
                    if (errorObj != null)
                    {
                        if (errorObj.ContainsKey("message")) errorMsg = errorObj["message"];
                        else if (errorObj.ContainsKey("Message")) errorMsg = errorObj["Message"];
                        else if (errorObj.ContainsKey("title")) errorMsg = errorObj["title"]; // Validate error
                    }
                }
                catch 
                { 
                    // Nếu không phải JSON (VD: server trả về text trơn "Not enough Gold!"), 
                    // thì giữ nguyên text đó để hiển thị.
                }

                // Xử lý hết hạn phiên đăng nhập (401)
                if (www.responseCode == 401)
                {
                    Debug.LogWarning("Token expired.");
                    GameEvents.TriggerSessionExpired();
                    onError?.Invoke("Phiên đăng nhập hết hạn.");
                }
                else
                {
                    Debug.LogError($"[API Error] {url}: {errorMsg}");
                    // Trả về errorMsg đã được xử lý để ToastManager hiển thị cho người chơi
                    onError?.Invoke(errorMsg); 
                }
            }
        }
    }
}