using UnityEngine;

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
