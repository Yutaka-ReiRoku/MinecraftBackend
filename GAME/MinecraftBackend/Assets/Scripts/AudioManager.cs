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
        StartCoroutine(LoadAudioFromWeb("bgm.mp3", AudioType.MPEG, true));
        StartCoroutine(LoadAudioFromWeb("click.mp3", AudioType.MPEG, false));
    }

    public void PlayMusic(string resourcePath)
    {
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            _musicSource.clip = clip;
            _musicSource.Play();
        }
    }
    public void PlaySFX(string name)
    {
        if (_clipCache.TryGetValue(name, out AudioClip webClip) || 
            _clipCache.TryGetValue(name + ".mp3", out webClip))
        {
            _sfxSource.PlayOneShot(webClip);
            return;
        }

        var localClip = Resources.Load<AudioClip>("Audio/" + name);
        if (localClip != null)
        {
            _sfxSource.PlayOneShot(localClip);
        }
        else
        {
        }
    }

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
                    if (!_clipCache.ContainsKey(filename))
                    {
                        _clipCache.Add(filename, clip);
                    }
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