using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioButton : MonoBehaviour
{
    public Button button;
    public Image render;

    [Space]
    public Sprite on;
    public Sprite off;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);

        SetStatus(AudioManager.AudioEnabled);
        AudioManager.OnChangedAudioEnabled += SetStatus;
    }

    [OPS.Obfuscator.Attribute.DoNotRenameAttribute]
    public void OnClick()
    {
        AudioManager.AudioEnabled = !AudioManager.AudioEnabled;
    }

    private void SetStatus(bool isActive)
    {
        render.sprite = (isActive) ? on : off;
    }

    private void OnDestroy()
    {
        AudioManager.OnChangedAudioEnabled -= SetStatus;
    }
}
