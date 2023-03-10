using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Game;
using BugsFarm.SpeakerSystem;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class AntPresenter : ABugPresenter
    {
        public int LevelP1 => _level + 1;
        public AIMover AiMover { get; private set; }
        public override bool IsAlive => AiStats.IsAlive;
        public override bool CanSpeak => AiStats.IsAlive;
        public Transform ArrowPoint => _antView.ArrowPoint;
        public UnitRipSceneObject UnitRipSceneObject => _antView.UnitRipSceneObject;
        public override AUnitView UnitView => _antView;
        [field:NonSerialized] public CfgUnit CfgUnit { get; private set; }
        [field: NonSerialized] public AntAnimator Animator { get; private set; }
        [field: NonSerialized] public AiStats AiStats { get; private set; }
        
        [NonSerialized] private UnitPhrases unitTypePhrases;
        [NonSerialized] private AntView _antView;
        private SM_AntRoot AiRoot { get; }
        private const float _xPscale = 31; // TODO: Load!!!
        private int _level;
        private float _xp;
        
        public AntPresenter(AntType type) : base(type)
        {
            _level = 0;
            AiRoot = new SM_AntRoot(this);
        }
        //public void Init(DiContainer container, AntView antView, PositionInfo position = null)
        //{
        //    _antView = antView;
        //    antView.Init(this);
//
        //    CfgUnit = Data_Fight.Instance.units[AntType];
        //    unitTypePhrases = Data_Ants.Instance.GetData(AntType).phrases;
        //    Animator = new AntAnimator(AntType, antView.SpineMain, antView.SpineClimb,antView.UnitRipSceneObject);
        //    AiStats = new AiStats(AiRoot);
        //    container.Inject(AiRoot);
        //    AiRoot.Setup(container, this);
//
        //    // нужно для создания и размещения - не в момент загрузки сохранения.
        //    if (!position.IsNullOrDefault())
        //    {
        //        AiMover.SetPosition(position);
        //    }
        //}
        public override void PostSpawnInit()
        {
            AiRoot.PostSpawnInit();
            GameEvents.OnAntSpawned?.Invoke();
        }
        public override void PostLoadRestore()
        {
            AiRoot.PostLoadRestore();
        }
        public override void Dispose()
        {
            AiRoot.Dispose();
            UnityEngine.Object.Destroy(_antView.gameObject);
        }
        public override void Update()
        {
            AiRoot.Update();
        }
        public override void SetCollidersAllowed(bool allowed)
        {
            UnitView.SetCollidersAllowed(allowed);
        }

        public void Upgrade()
        {
            _xp += XpRequiredToNextLevel();

            LevelUp(true);
        }
        public void AddXp(float delta)
        {
            _xp += delta;

            while (
                LevelP1 < Constants.MaxUnitLevel &&
                XpRequiredToNextLevel() <= 0
                )
                LevelUp(false);

            GameEvents.OnAntGotXP?.Invoke(this);
        }
        public float XpRequiredToNextLevel()
        {
            var power1 = CfgUnit.CalcPower(1);
            var power2 = CfgUnit.CalcPower(2);
            var n = 1 / (power2 - power1);

            var nextPower = CfgUnit.CalcPower(LevelP1 + 1);
            var deltaPower = nextPower - power1;

            var nextNXp = deltaPower * n;
            var nextXp = nextNXp * _xPscale;

            var required = nextXp - _xp;

            return required;
        }
        public void Kick(IAiControlable nextAi = null)
        {

            if(!nextAi.IsNullOrDefault())
            {
                // переключить задачу
                AiRoot.ForceTransition(nextAi);
                return;
            }
            // завершить текущую задачу, задача возьмется автоматически по приоритету.
            AiRoot.ForceHardFinish();
        }
        
        protected override string GetSayText(SpeakerParams speakerParams)
        {
            IEnumerable<string> phrases = default;
            var noWater = Keeper.GetObjects<BowlPresenter>().Any(x => x.QuantityCur <= 0);
            var noFood = !FindFood.ForEatAll().Any();

            var currentAiState = AiStats.CurrentAi?.AiType;
            if (currentAiState.HasValue)
            {
                if (noWater && AiStats.IsHungryWater)
                {
                    phrases = unitTypePhrases.noWater;
                } 
                else if  (noFood && AiStats.IsHungryFood)
                {
                    phrases = unitTypePhrases.noFood;
                }
                else if (false)
                {
                    phrases = Phrases.greetings;
                }
                else
                {
                    switch (AntType)
                    {
                        case AntType.Worker :
                            if (currentAiState == StateAnt.Dig)
                            {
                                phrases = unitTypePhrases.digging;
                            }
                            else if (currentAiState == StateAnt.Goldmine)
                            {
                                phrases = unitTypePhrases.mining;
                            }
                            break;
                        case AntType.Archer:
                        case AntType.Pikeman:
                        case AntType.Swordman:
                            if (currentAiState.AnyOff(StateAnt.TrainArcher, StateAnt.TrainPikeman, StateAnt.TrainWorker))
                            {
                                phrases = unitTypePhrases.training;
                                break;
                            }
                            phrases = currentAiState == StateAnt.Patrol
                                ? unitTypePhrases.patrol
                                : unitTypePhrases.idle;

                            break;
                    }
                }
            }
            return Tools.RandomItem(phrases.IsNullOrDefault() ? unitTypePhrases.idle.ToArray() : phrases.ToArray());
        }
        private void LevelUp(bool sendEvent)
        {
            _level++;

            if (sendEvent)
                GameEvents.OnAntTypeLevelUp?.Invoke(this);
        }
    }
    
    [Serializable]
    public abstract class ABugPresenter : AUnitPresenter
    {
        public AntType AntType { get; }
        public abstract bool IsAlive { get; }
        public abstract bool CanSpeak { get; }
        protected ABugPresenter(AntType type)
        {
            AntType = type;
        }
        public void Say(SpeakerParams speakerParams, UISpeaker window, float delay)
        {
            if (!window)
            {
                Debug.LogError($"{this} : Say - UI window is empty!!!");
                return;
            }

            // if (!SpeakerView)
            // {
            //     Debug.LogError($"{this} : Say - View is empty!!!");
            //     return;
            // }
            // window.SetText(GetSayText(speakerParams));
            // SpeakerView.Say(window, delay);
        }
        public override void SetCollidersAllowed(bool allowed)
        {
            UnitView.SetCollidersAllowed(allowed);
        }
        public override void Dispose()
        {
            if(UnitView)
            {
                Object.Destroy(UnitView.SeflContainer);
            }
        }
        protected abstract string GetSayText(SpeakerParams param);
    }
}