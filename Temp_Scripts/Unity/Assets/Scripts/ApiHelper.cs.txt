using System.Collections.Generic;
using UnityEngine;

// Lớp tiện ích giúp gọi API gọn gàng hơn
// Tự động tương thích với NetworkManager mới (Newtonsoft.Json)
public static class ApiHelper
{
    /// <summary>
    /// Gửi yêu cầu GET (Lấy dữ liệu)
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
    /// <param name="endpoint">Đường dẫn API (vd: "game/profile")</param>
    public static void Get<T>(string endpoint, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (NetworkManager.Instance != null)
        {
            // Gọi NetworkManager với 5 tham số (Không cần truyền Header thủ công nữa)
            NetworkManager.Instance.StartCoroutine(
                NetworkManager.Instance.SendRequest<T>(endpoint, "GET", null, onSuccess, onError)
            );
        }
        else
        {
            Debug.LogError("[ApiHelper] NetworkManager instance is null!");
        }
    }

    /// <summary>
    /// Gửi yêu cầu POST (Gửi dữ liệu/Hành động)
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
    /// <param name="endpoint">Đường dẫn API (vd: "game/buy")</param>
    /// <param name="body">Object dữ liệu gửi đi</param>
    public static void Post<T>(string endpoint, object body, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartCoroutine(
                NetworkManager.Instance.SendRequest<T>(endpoint, "POST", body, onSuccess, onError)
            );
        }
        else
        {
            Debug.LogError("[ApiHelper] NetworkManager instance is null!");
        }
    }

    /// <summary>
    /// Gửi yêu cầu PUT (Cập nhật)
    /// </summary>
    public static void Put<T>(string endpoint, object body, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartCoroutine(
                NetworkManager.Instance.SendRequest<T>(endpoint, "PUT", body, onSuccess, onError)
            );
        }
    }

    /// <summary>
    /// Gửi yêu cầu DELETE (Xóa)
    /// </summary>
    public static void Delete<T>(string endpoint, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartCoroutine(
                NetworkManager.Instance.SendRequest<T>(endpoint, "DELETE", null, onSuccess, onError)
            );
        }
    }
}