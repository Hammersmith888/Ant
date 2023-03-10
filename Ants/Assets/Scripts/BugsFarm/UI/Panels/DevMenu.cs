using BugsFarm.Game;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DevMenu : APanel
{
    [SerializeField] private Text _text_Time;
    [SerializeField] private Text _text_SimTime;
    [SerializeField] private Text _text_DayTime;
    [SerializeField] private Text _text_FPS;
    
    //private DayNight _dayNight;
    //[Inject]
    //public void Inject(DayNight dayNight)
    //{
    //    _dayNight = dayNight;
    //}
    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = false;
        manualClose = false;
    }
   // public void OnDayTime() => _dayNight.SwitchDayTime();
    //public void OnIRL() => _dayNight.SetIRL();
    public void OnSendReport()
    {
        // Emulate error
        /*
        Text text		= null;
        text.text		= "";
        */

        LogSaverAndSender.Instance.Oops();

        Close();

        // throw new Exception( "Test exception (please ignore)." );
    }
    public void OnSimulation()
    {
        if (float.TryParse(_text_SimTime.text, out var hours))
            SimulationOld.Instance.SimulationDemo(hours);
    }
    public void OnReset()
    {
        SimulationOld.Instance.Restore();
        GameInit.Reset(true);
        Close();
    }
    public void OnClear() => GameInit.Clear();
    private void Update()
    {
        //var adjusted = _dayNight.AdjustedTimeSpan();

        //_text_Time.text = adjusted.ToString(@"h\:mm\:ss");

        //switch (DayNight.DayPart)
        //{
        //    case DayPart.Night:
        //        _text_DayTime.text = "Ночь";
        //        break;
        //    case DayPart.NightToDay:
        //        _text_DayTime.text = "Светлеет";
        //        break;
        //    case DayPart.Day:
        //        _text_DayTime.text = "День";
        //        break;
        //    case DayPart.DayToNight:
        //        _text_DayTime.text = "Темнеет";
        //        break;
        //}

        _text_FPS.text = Mathf.RoundToInt(FPS_Counter.FPS).ToString();
    }
}