using UnityEngine;
using UnityEngine.UI; // Slider 사용하려고 추가

public class AudioManager : MonoBehaviour
{
    [Header("-------Audio Source-------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource MOVESource;


    [Header("-------Audio Clip-------")]
    public AudioClip musicaudio;
    public AudioClip sfxaudio;
    public AudioClip moveaudio;

    // ---------------- 추가된 부분 ----------------
    [Header("-------Volume Slider-------")]

    // 배경음 슬라이더
    public Slider musicSlider;

    // 효과음 슬라이더
    public Slider sfxSlider;

    // 착수음 슬라이더
    public Slider moveSlider;
    // --------------------------------------------

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    } //추가! 씬 바꿔도 노래 안 꺼지게



    private void Start()
    {
        musicSource.clip = musicaudio;
        musicSource.Play();
    }
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void PlayMove(AudioClip clip)
    {
        MOVESource.PlayOneShot(clip);
    }
}
