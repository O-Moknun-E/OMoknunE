using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource moveSource;

    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float moveVolume = 1f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        bgmSource.volume = masterVolume * bgmVolume;
        sfxSource.volume = masterVolume * sfxVolume;
        moveSource.volume = masterVolume * moveVolume;
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        bgmSource.volume = masterVolume * bgmVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        sfxSource.volume = masterVolume * sfxVolume;
    }

    public void SetMoveVolume(float value)
    {
        moveVolume = value;
        moveSource.volume = masterVolume * moveVolume;
    }
}