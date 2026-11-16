using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Twins every volume slider that uses the same PlayerPrefs key (e.g. the master volume of the
/// settings menu and of the pause menu): moving one moves the others live, and each slider shows
/// the saved value when it appears, so the value also survives a restart.
/// The volume itself is still applied / saved by UIHandler.SetXxxVolume (slider onValueChanged).
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderSync : MonoBehaviour
{
    private static event Action<string, float, VolumeSliderSync> VolumeChanged;

    [Tooltip("PlayerPrefs key: MasterVolume, MusicVolume, SFXVolume or AmbianceVolume")]
    [SerializeField] private string _prefsKey = "MasterVolume";

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(OnSliderChanged);
        VolumeChanged += OnOtherSliderChanged; // also while hidden, so it's already right when shown
    }

    private void OnDestroy()
    {
        VolumeChanged -= OnOtherSliderChanged;
        if (_slider) _slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey(_prefsKey))
            _slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(_prefsKey));
    }

    private void OnSliderChanged(float value) => VolumeChanged?.Invoke(_prefsKey, value, this);

    private void OnOtherSliderChanged(string key, float value, VolumeSliderSync sender)
    {
        if (sender == this || key != _prefsKey) return;
        // No notify: the sender already applied and saved the volume.
        _slider.SetValueWithoutNotify(value);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => VolumeChanged = null;
}
