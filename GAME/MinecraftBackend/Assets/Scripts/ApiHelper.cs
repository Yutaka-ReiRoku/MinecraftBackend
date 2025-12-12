using System.Collections.Generic;
using UnityEngine;

public static class ApiHelper
{

    public static void Get<T>(string endpoint, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (NetworkManager.Instance != null)
        {
            
            NetworkManager.Instance.StartCoroutine(
                NetworkManager.Instance.SendRequest<T>(endpoint, "GET", null, onSuccess, onError)
            );
        }
        else
        {
            Debug.LogError("[ApiHelper] NetworkManager instance is null!");
        }
    }

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

    public static void Put<T>(string endpoint, object body, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartCoroutine(
                NetworkManager.Instance.SendRequest<T>(endpoint, "PUT", body, onSuccess, onError)
            );
        }
    }

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