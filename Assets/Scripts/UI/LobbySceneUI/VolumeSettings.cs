using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider MoveSlider;
    [SerializeField] private Slider MasterSlider; // 추가



    private void Start()
    {
        if(PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetMoveVolume();
            SetMasterVolume(); //
        }

        
    }
    private bool isMasterChanging = false; ////
    public void SetMusicVolume()
    {
        if (isMasterChanging) return; ////
        float volume = musicSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("musicVolume" , volume);
    }
    public void SetSFXVolume()
    {
        if (isMasterChanging) return;////
        float volume = SFXSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("SFXVolume" , volume);
    }
    public void SetMoveVolume()
    {
        if (isMasterChanging) return;////
        float volume = MoveSlider.value;
        myMixer.SetFloat("Move", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("MoveVolume" , volume);
    }
    public void SetMasterVolume()
    {
        isMasterChanging = true; //// 차단 시작

        float volume = MasterSlider.value;              
        myMixer.SetFloat("Master", Mathf.Log10(volume) * 20); 
        PlayerPrefs.SetFloat("MasterVolume", volume);

        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f) * volume;
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f) * volume;
        MoveSlider.value = PlayerPrefs.GetFloat("MoveVolume", 1f) * volume;//추가

        isMasterChanging = false; //// 차단 해제
    }

    private void LoadVolume()
    {

        MasterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f); // 추가
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        MoveSlider.value = PlayerPrefs.GetFloat("MoveVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetMoveVolume();
        SetMasterVolume();
    }
}
