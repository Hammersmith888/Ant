using System;
using UnityEngine;

public static class AudioManager
{
    public static event Action<bool> OnChangedAudioEnabled;
    public static bool AudioEnabled
    {
        get => PlayerPrefs.GetInt("AUDIO_ENABLED", 1).ToBool();
        set
        {
            PlayerPrefs.SetInt("AUDIO_ENABLED", value.ToInt());
            OnChangedAudioEnabled.Invoke(AudioEnabled);
        }
    }
}