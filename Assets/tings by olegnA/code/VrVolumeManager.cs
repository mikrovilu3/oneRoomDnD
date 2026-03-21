using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// VR Volume Manager — identical logic to the flat version.
/// Works with World Space Canvas sliders interacted with via XR Ray Interactor.
/// Attach to any persistent GameObject and assign the Inspector fields.
/// </summary>
public class VRVolumeManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders (World Space Canvas)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Must match the Exposed Parameter names in your AudioMixer exactly
    private const string MasterParam = "MasterVolume";
    private const string MusicParam = "MusicVolume";
    private const string SFXParam = "SFXVolume";

    private void Start()
    {
        float master = PlayerPrefs.GetFloat(MasterParam, 1f);
        float music = PlayerPrefs.GetFloat(MusicParam, 1f);
        float sfx = PlayerPrefs.GetFloat(SFXParam, 1f);

        SetMixerVolume(MasterParam, master);
        SetMixerVolume(MusicParam, music);
        SetMixerVolume(SFXParam, sfx);

        if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
    }

    // --- Called by each Slider's OnValueChanged ---

    public void SetMasterVolume(float value)
    {
        SetMixerVolume(MasterParam, value);
        PlayerPrefs.SetFloat(MasterParam, value);
    }

    public void SetMusicVolume(float value)
    {
        SetMixerVolume(MusicParam, value);
        PlayerPrefs.SetFloat(MusicParam, value);
    }

    public void SetSFXVolume(float value)
    {
        SetMixerVolume(SFXParam, value);
        PlayerPrefs.SetFloat(SFXParam, value);
    }

    // --- Helper ---

    private void SetMixerVolume(string parameter, float linearValue)
    {
        float dB = linearValue > 0.0001f
            ? Mathf.Log10(linearValue) * 20f
            : -80f;

        audioMixer.SetFloat(parameter, dB);
    }
}