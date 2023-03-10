using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using UnityEngine;


public class HUD_BottomQuests : MB_Singleton<HUD_BottomQuests>
{
    void Start() => GetComponent<Animation>().Play();


    public void OnQuests()
    {
        //UI_Control.Instance.Open(PanelID.MenuQuests);

        Sounds.Play(Sound.button_BottomHUD);
    }
}