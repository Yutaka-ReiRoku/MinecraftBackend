using UnityEngine;

// Script này đảm bảo các hệ thống cốt lõi luôn tồn tại
// Ngay cả khi bạn chạy trực tiếp từ GameScene để test
public class GameBootstrapper : MonoBehaviour
{
    [Header("Prefabs (Optional)")]
    // Kéo prefab chứa NetworkManager/AudioManager vào đây nếu muốn config sẵn
    // Nếu không, script sẽ tự tạo GameObject mới
    public GameObject CoreSystemsPrefab; 

    void Awake()
    {
        // 1. Kiểm tra NetworkManager
        if (NetworkManager.Instance == null)
        {
            Debug.Log("[Bootstrapper] Creating NetworkManager...");
            GameObject networkGO = new GameObject("NetworkManager");
            networkGO.AddComponent<NetworkManager>();
            // NetworkManager Awake() sẽ tự xử lý DontDestroyOnLoad
        }

        // 2. Kiểm tra AudioManager
        if (AudioManager.Instance == null)
        {
            Debug.Log("[Bootstrapper] Creating AudioManager...");
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();
        }

        // 3. Kiểm tra các config khác nếu cần
        // Ví dụ: Load settings âm thanh mặc định nếu chưa có
        if (!PlayerPrefs.HasKey("MusicVol"))
        {
            PlayerPrefs.SetFloat("MusicVol", 0.5f);
            PlayerPrefs.SetFloat("SfxVol", 1.0f);
            PlayerPrefs.Save();
        }
    }
}