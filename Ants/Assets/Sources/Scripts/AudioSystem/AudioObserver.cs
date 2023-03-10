using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioObserver : MonoBehaviour
{
    public static bool IsInit = false;
    private void Awake()
    {
        if (IsInit)
        {
            return;
        }

        IsInit = true;
        SetAudio(AudioManager.AudioEnabled);
        AudioManager.OnChangedAudioEnabled += SetAudio;
    }

    private void SetAudio(bool enabled)
    {
        AudioListener.volume = (enabled) ? 1f : 0f; 
    }

    private void OnDestroy()
    {
        AudioManager.OnChangedAudioEnabled -= SetAudio;
    }
}
