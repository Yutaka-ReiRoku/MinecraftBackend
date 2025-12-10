using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    // Cache để tránh tải lại SFX nhiều lần
    private Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

    // Đổi port nếu cần thiết
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

        // Tạo AudioSource qua code nếu chưa có
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
    }

    void Start()
    {
        // Load setting volume đã lưu (Mặc định Music 30%, SFX 100%)
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.3f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVol", 1.0f);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // Tải nhạc nền từ Server (Stream)
        // Đây là tính năng nâng cao: Load tài nguyên động từ Web
        StartCoroutine(LoadAudioFromWeb("bgm.mp3", AudioType.MPEG, true));

        // Tải trước tiếng click từ Server để sẵn sàng dùng
        StartCoroutine(LoadAudioFromWeb("click.mp3", AudioType.MPEG, false));
    }

    // --- LOCAL RESOURCES (Dự phòng) ---

    public void PlayMusic(string resourcePath)
    {
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            _musicSource.clip = clip;
            _musicSource.Play();
        }
    }

    /// <summary>
    /// Phát âm thanh hiệu ứng (SFX)
    /// </summary>
    /// <param name="name">Tên file (vd: "click")</param>
    public void PlaySFX(string name)
    {
        // 1. Tìm trong Cache (đã tải từ Web)
        // Thử tên gốc hoặc tên có đuôi .mp3
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
            // Debug.LogWarning($"Audio not found: {name}");
        }
    }

    // --- WEB STREAMING ---

    /// <summary>
    /// Tải file âm thanh từ thư mục wwwroot/audio của Server
    /// </summary>
    public IEnumerator LoadAudioFromWeb(string filename, AudioType type, bool isMusic)
    {
        string url = BASE_URL + filename;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = filename;

                if (isMusic)
                {
                    _musicSource.clip = clip;
                    _musicSource.Play();
                }
                else
                {
                    // Cache SFX (ví dụ click.mp3)
                    if (!_clipCache.ContainsKey(filename))
                    {
                        _clipCache.Add(filename, clip);
                    }
                    
                    // Map thêm tên ngắn gọn (vd "click" -> clip) để dễ gọi
                    string shortName = filename.Replace(".mp3", "").Replace(".wav", "");
                    if (!_clipCache.ContainsKey(shortName))
                    {
                        _clipCache.Add(shortName, clip);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Audio] Failed to load {url}: {www.error}. Ensure file exists in wwwroot/audio.");
            }
        }
    }

    // --- CONTROLS ---

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