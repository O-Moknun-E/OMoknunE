using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundPanelManager : MonoBehaviour
{
    [Header("슬라이더")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider moveSlider;

    [Header("% 텍스트")]
    public Text masterText;
    public Text bgmText;
    public Text sfxText;
    public Text moveText;

    private const string MASTER = "Vol_Master";
    private const string BGM = "Vol_BGM";
    private const string SFX = "Vol_SFX";
    private const string MOVE = "Vol_Move";

    void Start()
    {
        // 저장된 볼륨 불러오기
        masterSlider.value = PlayerPrefs.GetFloat(MASTER, 1f);
        bgmSlider.value = PlayerPrefs.GetFloat(BGM, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX, 1f);
        moveSlider.value = PlayerPrefs.GetFloat(MOVE, 1f);

        // 슬라이더 이벤트 연결
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        moveSlider.onValueChanged.AddListener(OnMoveChanged);

        // 텍스트 초기값
        UpdateText(masterText, masterSlider.value);
        UpdateText(bgmText, bgmSlider.value);
        UpdateText(sfxText, sfxSlider.value);
        UpdateText(moveText, moveSlider.value);
    }

    public void OnMasterChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat(MASTER, value);
        UpdateText(masterText, value);
    }

    public void OnBGMChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
        PlayerPrefs.SetFloat(BGM, value);
        UpdateText(bgmText, value);
    }

    public void OnSFXChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat(SFX, value);
        UpdateText(sfxText, value);
    }

    public void OnMoveChanged(float value)
    {
        SoundManager.Instance.SetMoveVolume(value);
        PlayerPrefs.SetFloat(MOVE, value);
        UpdateText(moveText, value);
    }

    private void UpdateText(Text text, float value)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value * 100) + "%";
    }
}