using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

    // [QUAN TRỌNG] Đổi IP này nếu chạy trên điện thoại thật (VD: 192.168.1.x)
    // Nếu chạy trong Unity Editor thì dùng localhost là được.
    private const string BASE_URL = "http://localhost:5000/audio/";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
    }

    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.3f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVol", 1.0f);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // Load nhạc nền và âm thanh click
        StartCoroutine(LoadAudioFromWeb("bgm.mp3", AudioType.UNKNOWN, true));
        StartCoroutine(LoadAudioFromWeb("click.mp3", AudioType.UNKNOWN, false));
    }

    public void PlayMusic(string resourcePath)
    {
        // Fallback: Nếu không tải được từ web thì thử load từ Resources
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            _musicSource.clip = clip;
            _musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        // 1. Tìm trong Cache (đã tải từ Web)
        if (_clipCache.TryGetValue(name, out AudioClip webClip) || 
            _clipCache.TryGetValue(name + ".mp3", out webClip))
        {
            _sfxSource.PlayOneShot(webClip);
            return;
        }

        // 2. Nếu không có, tìm trong Resources nội bộ
        var localClip = Resources.Load<AudioClip>("Audio/" + name);
        if (localClip != null)
        {
            _sfxSource.PlayOneShot(localClip);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Không tìm thấy âm thanh: {name}");
        }
    }

    public IEnumerator LoadAudioFromWeb(string filename, AudioType type, bool isMusic)
    {
        // Xử lý đường dẫn
        string url = BASE_URL + filename;
        Debug.Log($"[AudioManager] Đang tải audio từ: {url}");

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            // Kiểm tra lỗi mạng hoặc lỗi HTTP (404, 500)
            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[AudioManager] Lỗi tải {filename}: {www.error}. URL: {url}");
            }
            else
            {
                // [FIX LỖI FMOD] Thử lấy nội dung an toàn
                AudioClip clip = null;
                try 
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AudioManager] Lỗi định dạng file (FMOD): {ex.Message}. File có thể bị hỏng hoặc không phải MP3 thật. URL: {url}");
                }

                if (clip != null)
                {
                    clip.name = filename;
                    if (isMusic)
                    {
                        _musicSource.clip = clip;
                        _musicSource.Play();
                        Debug.Log("[AudioManager] Đã phát nhạc nền: " + filename);
                    }
                    else
                    {
                        // Lưu vào cache để dùng sau (cho SFX)
                        string shortName = filename.Replace(".mp3", "").Replace(".wav", "");
                        if (!_clipCache.ContainsKey(shortName))
                        {
                            _clipCache.Add(shortName, clip);
                        }
                        // Lưu cả tên đầy đủ
                        if (!_clipCache.ContainsKey(filename))
                        {
                            _clipCache.Add(filename, clip);
                        }
                    }
                }
            }
        }
    }

    public void SetMusicVolume(float vol)
    {
        _musicSource.volume = vol;
        PlayerPrefs.SetFloat("MusicVol", vol);
    }

    public void SetSFXVolume(float vol)
    {
        _sfxSource.volume = vol;
        PlayerPrefs.SetFloat("SfxVol", vol);
    }
}