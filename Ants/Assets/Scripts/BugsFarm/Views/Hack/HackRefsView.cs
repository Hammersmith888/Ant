using BugsFarm.Installers;
using BugsFarm.Views.Core;
using BugsFarm.Views.Fight;
using UnityEngine;
using Zenject;

namespace BugsFarm.Views.Hack
{
    public class HackRefsView : AView
    {
        [Inject] private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        [Inject] private readonly FightView _fightView;
        
        private static HackRefsView _instance;

        public static HackRefsView Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (HackRefsView) FindObjectOfType(typeof(HackRefsView));

                    if (_instance == null)
                    {
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<HackRefsView>();
                        singletonObject.name = typeof(HackRefsView).ToString() + " (Singleton)";
                    }
                }

                return _instance;
            }
        }

        public BattleSettingsInstaller.BattleSettings BattleSettings => _battleSettings;
        public FightView FightView => _fightView;
    }
}