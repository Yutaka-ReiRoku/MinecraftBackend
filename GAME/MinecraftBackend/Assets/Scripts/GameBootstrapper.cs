using UnityEngine;



public class GameBootstrapper : MonoBehaviour
{
    [Header("Prefabs (Optional)")]
    
    
    public GameObject CoreSystemsPrefab; 

    void Awake()
    {
        
        if (NetworkManager.Instance == null)
        {
            Debug.Log("[Bootstrapper] Creating NetworkManager...");
            GameObject networkGO = new GameObject("NetworkManager");
            networkGO.AddComponent<NetworkManager>();
            
        }

        
        if (AudioManager.Instance == null)
        {
            Debug.Log("[Bootstrapper] Creating AudioManager...");
            GameObject audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioManager>();
        }

        
        
        if (!PlayerPrefs.HasKey("MusicVol"))
        {
            PlayerPrefs.SetFloat("MusicVol", 0.5f);
            PlayerPrefs.SetFloat("SfxVol", 1.0f);
            PlayerPrefs.Save();
        }
    }
}