using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // Cần cài package: com.unity.nuget.newtonsoft-json

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private string _token;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ sống qua các Scene
            
            // Tự động load token đã lưu (nếu có)
            _token = PlayerPrefs.GetString("JWT", null);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Lưu Token sau khi đăng nhập thành công
    /// </summary>
    public void SetToken(string token)
    {
        _token = token;
        PlayerPrefs.SetString("JWT", token);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Xóa Token khi đăng xuất
    /// </summary>
    public void ClearSession()
    {
        _token = null;
        PlayerPrefs.DeleteKey("JWT");
        PlayerPrefs.DeleteKey("CurrentCharID");
        PlayerPrefs.Save();
        
        // Xóa Cache ảnh để giải phóng RAM
        ImageLoader.ClearCache();
    }

    /// <summary>
    /// Hàm gọi API tổng quát (Generic)
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu mong muốn trả về (VD: List<ShopItemDto>)</typeparam>
    /// <param name="endpoint">Đường dẫn API (vd: "game/shop")</param>
    /// <param name="method">GET, POST, PUT, DELETE</param>
    /// <param name="body">Dữ liệu gửi đi (object) hoặc null</param>
    /// <param name="onSuccess">Callback khi thành công</param>
    /// <param name="onError">Callback khi lỗi</param>
    public IEnumerator SendRequest<T>(string endpoint, string method, object body, Action<T> onSuccess, Action<string> onError)
    {
        string url = GameConfig.GetApiEndpoint(endpoint);
        
        // Tạo Request
        using (UnityWebRequest www = new UnityWebRequest(url, method))
        {
            // 1. Setup Body (nếu có)
            if (body != null)
            {
                string jsonBody = JsonConvert.SerializeObject(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }

            // 2. Setup Download Handler
            www.downloadHandler = new DownloadHandlerBuffer();

            // 3. Setup Headers (Auth & Character Context)
            if (!string.IsNullOrEmpty(_token))
            {
                www.SetRequestHeader("Authorization", "Bearer " + _token);
            }

            // Gửi ID nhân vật đang chơi để Backend biết đang thao tác với ai (Inventory, Quest...)
            string charId = PlayerPrefs.GetString("CurrentCharID", "");
            if (!string.IsNullOrEmpty(charId))
            {
                www.SetRequestHeader("X-Character-ID", charId);
            }

            // 4. Gửi Request
            yield return www.SendWebRequest();

            // 5. Xử lý Kết quả
            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;

                // Debug log để kiểm tra (tắt khi release)
                // Debug.Log($"[API] {method} {endpoint}: {jsonResponse}");

                try
                {
                    // Dùng Newtonsoft để parse JSON (kể cả List/Array)
                    // Nếu T là object (không quan tâm kết quả trả về), trả về default
                    if (typeof(T) == typeof(object) && string.IsNullOrEmpty(jsonResponse))
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
                    onError?.Invoke("Lỗi xử lý dữ liệu từ máy chủ.");
                }
            }
            else
            {
                // Xử lý lỗi HTTP
                string errorMsg = www.error;
                
                // Cố gắng đọc message lỗi chi tiết từ Backend (nếu có)
                if (!string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    try 
                    {
                        // Giả sử Backend trả về { "Message": "Lỗi chi tiết..." }
                        var errorObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.downloadHandler.text);
                        if (errorObj != null && errorObj.ContainsKey("Message"))
                        {
                            errorMsg = errorObj["Message"];
                        }
                        else if (errorObj != null && errorObj.ContainsKey("message")) // chữ thường
                        {
                            errorMsg = errorObj["message"];
                        }
                    }
                    catch { /* Không parse được thì dùng lỗi mặc định */ }
                }

                // Xử lý hết hạn phiên đăng nhập (401)
                if (www.responseCode == 401)
                {
                    Debug.LogWarning("Token expired or invalid.");
                    GameEvents.TriggerSessionExpired(); // Bắn sự kiện để UI tự logout
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