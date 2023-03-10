using BugsFarm.Services.BootstrapService;
using BugsFarm.SpeakerSystem;
using DG.Tweening;
using UnityEngine;

namespace BugsFarm.BootstrapCommon
{
    public class InitOldSystemsCommand : Command
    {
        private readonly ISpeakerSystem _speakerSystem;
        //private readonly DayNight _dayNightSystem;

        public InitOldSystemsCommand(ISpeakerSystem speakerSystem) //,DayNight dayNightSystem)
        {
            _speakerSystem = speakerSystem;
            //_dayNightSystem = dayNightSystem;
        }
        
        public override void Do()
        {
            TechStuff();
            // Analytics.SessionStart();
            // Stock.Init(_monoFactory);
            // FirstHour.Init(_dataOther);
            _speakerSystem.Welcome();
            OnDone();
        }
        private void TechStuff()
        {
        #if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        #endif

        #if UNITY_STANDALONE_WIN
		Screen.SetResolution( 1080, 1920, true );
        #endif

            if (Application.isMobilePlatform)
                Application.targetFrameRate = 60;

            QualitySettings.maxQueuedFrames = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            DOTween.Init(false, true, null);
            HandleAspectRatio();
            Quests.Init();
            StatsManager.Init();
            //todo check it
            //Fight.Init(_game);
        }
        private void HandleAspectRatio()
        {
            //_dayNightSystem.HandleAspectRatio();
        }
    }
}