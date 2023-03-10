using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum StateAnt
    {
        None = 0,

        // Common
        Fall = 1,
        Drink = 3,
        Eat = 4,
        Rest = 2,
        Sleep = 15,
        Dead = 7,
        Walk = 14,

        // Worker
        Feed = 8,
        Restock = 10,
        RestockFight = 11,
        RestockHerbs = 20,
        Goldmine = 5,
        Clean = 12,
        Garden = 16,
        Build = 17,
        Dig = 6,
        VineBuild = 21,


        // Warriors
        Patrol = 13,
        TrainArcher = 9,
        TrainPikeman = 18,
        TrainWorker = 19,
        TrainHavySquad = 23,
        TrainSwordMan = 24,
        TrainButterfly = 25, // последний добавленный
        
        //NPC
        Dealer = 22,
    }
    
    [Serializable]
    public class SM_AntRoot : AStateMachine<StateAnt>, IPostLoadRestorable, IPostSpawnInitable, IDisposable
    {
        public float? CurrentTaskTime => CurrentAi?.Task.TimerTask.Left;
        public double TimeBorn => _timeBorn;
        [field:NonSerialized] public bool IsForceTransition { get; private set; }
        [field:NonSerialized] public CfgAnt Data { get; private set; }
        [field:NonSerialized] public AIMover AiMover { get; private set; }
        [field:NonSerialized] public AntAnimator Animator { get; private set; }
        [field:NonSerialized] public AUnitView UnitView { get; private set; }
        [field:NonSerialized] public AiStats AiStats { get; private set; }
        [field:NonSerialized] public AntPresenter AntPresenter { get; private set; }
        [field:NonSerialized] public Transform ArrowPoint { get; private set; }
        [field:NonSerialized] public UnitRipSceneObject UnitRipSceneObject { get; private set; }
        [field:NonSerialized] public IAiControlable CurrentAi { get; private set; }
        [field:NonSerialized] public event Action OnAiStatsUpdate;
        [field:NonSerialized] public event Action OnPostSpawnInit;
        [field:NonSerialized] public BlackList BlackList { get; private set; }
        
        private double _timeBorn;
        private readonly List<IAiControlable> _aiControlables;
        public SM_AntRoot(AntPresenter antPresenter)
        {
            _timeBorn = SimulationOld.GameAge;
            var data = Data_Ants.Instance.GetData(antPresenter.AntType);
            _aiControlables = new List<IAiControlable>();
            var aiPriority = data.GetTaskCount(); 
            foreach (var ai in data.GetTasks())
            {
                var created = AiFactory.Create(ai, aiPriority--);
                if (created.IsNullOrDefault()) continue;
                created.Task.SetTaskTime(data.GetTaskTime(ai, true));
                _aiControlables.Add(created);
            }
        }
        public virtual void Setup(DiContainer container,AntPresenter root)
        {
            BlackList = new BlackList();
            AntPresenter = root;
            AiMover = root.AiMover;
            Animator = root.Animator;
            UnitView = root.UnitView;
            AiStats = root.AiStats;
            UnitRipSceneObject = root.UnitRipSceneObject;
            ArrowPoint = root.ArrowPoint;
            Data = Data_Ants.Instance.GetData(root.AntType);

            _aiControlables.ForEach(x =>
            {
                container.Inject(x);
                x.Setup(this);
            });
        }
        public virtual void PostSpawnInit()
        {
            OnAiStatsUpdate?.Invoke();
            AiMover.PostSpawnInit();
            OnPostSpawnInit?.Invoke();
        }
        public virtual void PostLoadRestore()
        {
            OnAiStatsUpdate?.Invoke();
            AiMover.PostLoadRestore();
            var ai = GetAi(State);
            ai?.PostLoadRestore();
            CurrentAi = ai;
            Debug.LogError($"{this} : PostLoadRestore CurrentAi '{CurrentAi}', State {State}");
        }
        public void Dispose()
        {
            foreach (var ai in _aiControlables)
            {
                ai.Dispose();
            }
            BlackList.Dispose();
        }
        public bool CanReachTarget(IPosSide pos, params object[] toBlackList)
        {
            if (BlackList.HasObjects(toBlackList) || !AiMover.CanReachTarget(pos.Position))
            {
                BlackList.Add(toBlackList);
                return false;
            }
            return true;
        }
        public override void Update()
        {
            if(!AiStats.IsAlive) return;
            OnAiStatsUpdate?.Invoke();
            if (State.IsNullOrDefault()) // пассивное получение задачи
            {
                CurrentAi = NextByPriority();
                if (CurrentAi.IsNullOrDefault() || !CurrentAi.Task.TaskStart())
                {
                    return;
                }
                
                Debug.Log($"{this} : Задача стартует : '{CurrentAi}'");
                IsForceTransition = false;
                Transition(CurrentAi.AiType);
            }
            else if (AvailableTask(CurrentAi, out var task)) // обновление текущей задачи
            {
                task.Update();
            }
            else // задача была выполнена, переход к пассивному получению задач.
            {
                Transition(StateAnt.None);
            }
        }
        
        /// <summary>
        /// Переключает задачи автоматически по приоритету.
        /// Внутренняя функция.
        /// </summary>
        public bool AutoTransition(IAiControlable forceAi)
        {
            if (!AiStats.IsAlive)
            {
                Debug.Log($"{this} : Автоматическое переключение не доступно : жучек мертв!!!");
                return IsForceTransition = false;
            }

            if (IsForceTransition)
            {
                Debug.Log($"{this} : Автоматическое переключение не доступно : \n" +
                          $"в данный момент производится переключение, запрос на переключение ['{forceAi}'] ");
                return false;
            }
            
            IsForceTransition = true;
            
            if (forceAi.IsNullOrDefault())
            {
                Debug.LogException(new Exception($"{this} : Автоматическое переключение не доступно : \n" +
                                                 $"отсутствует задача для переключения."));
                return IsForceTransition = false;
            }
            
            if (!CurrentAi.IsNullOrDefault())
            {
                if (forceAi == CurrentAi)
                {
                    Debug.Log($"{this} : Автоматическое переключение не доступно : \n" +
                              $"задача уже в работе ['{forceAi}']");
                    return IsForceTransition = false;
                }
                
                if (forceAi.Priority < CurrentAi.Priority)
                {
                    Debug.Log($"{this} : Автоматическое переключение не доступно : \n" +
                                   $"приоритет задачи ['{forceAi}'] ниже текущей задачи ['{CurrentAi}']!!!");
                    return IsForceTransition = false;
                }
                
                if (!CurrentAi.IsInterruptible)
                {
                    Debug.Log($"{this} : Автоматическое переключение не доступно : \n" +
                              $"текущая задача ['{CurrentAi}'] не прирываемая.");
                    return IsForceTransition = false;;
                }
                CurrentAi.Task.ForceHardFinish();
            }

            if (!IsForceTransition)
            {
                Debug.Log($"{this} : Автоматическое переключение отмененно : \n" +
                          $"['{CurrentAi}'] на ['{forceAi}'].");
                return IsForceTransition = false;
            }

            if (forceAi.Task.TaskStart())
            {
                Debug.Log($"{this} : Автоматическое переключение завершено : \n" +
                          $"['{CurrentAi}'] на ['{forceAi}'].");
                IsForceTransition = false;
                CurrentAi = forceAi;
                Transition(forceAi.AiType);
                return true;
            }

            Debug.Log($"{this} : Автоматическое не достигло цели : \n" +
                      $"задача '{forceAi}' недоступна.");
            IsForceTransition = false;
            return false;
        }

        /// <summary>
        /// Переключить задачу принудительно, может прирвать AutoTransition
        /// </summary>
        public void ForceTransition(IAiControlable forceAi)
        {
            if (!AiStats.IsAlive)
            {
                Debug.LogException(new Exception($"{this} : Принудительное переключение не доступно : \n" +
                                                 $"жучек мертв, вызывает задача ['{forceAi}']"));
                return ;
            }

            if (forceAi.IsNullOrDefault())
            {
                Debug.LogException(new Exception($"{this} : Принудительное переключение не доступно : \n" +
                                                 $"отсутствует задача для переключения."));
                return;
            }

            if (forceAi == CurrentAi)
            {
                Debug.LogException(new Exception($"{this} : Принудительное переключение не доступно : \n" +
                                                 $"уже в работе ['{forceAi}']"));
                return;
            }

            if (!CurrentAi.IsNullOrDefault())
            {                
                if (!CurrentAi.IsInterruptible)
                {
                    Debug.Log($"{this} : Принудительное переключение не доступно : \n" +
                              $"текущая задача непрерываемая ['{CurrentAi}'].");
                    return;
                }

                ForceHardFinish();
            }
            
            if (IsForceTransition)
            {
                Debug.Log($"{this} : Принудительное переключение : \n" +
                          $"переключение другого процесса отменено.");
                IsForceTransition = false;
            }

            if (forceAi.Task.TaskStart())
            {
                Debug.Log($"{this} : Принудительное переключение завершено : \n" +
                          $"['{CurrentAi}'] на ['{forceAi}'].");
                IsForceTransition = false;
                CurrentAi = forceAi;
                Transition(forceAi.AiType);
            }
            else
            {
                Debug.Log($"{this} : Принудительное переключение не достигло цели : \n" +
                          $"задача не доступна ['{forceAi}'].");
                IsForceTransition = false;
            }
        }
        public override void ForceHardFinish()
        {
            CurrentAi?.Task.ForceHardFinish();
            base.ForceHardFinish();
        }

        private IAiControlable NextByPriority()
        {
            if (!AiStats.IsAlive) return default;
            var ai = _aiControlables.FirstOrDefault(x =>
            {
                if (x.TryStart())
                {
                    //Debug.LogError($"{this} : Следующая задача по приоритету : {x}");
                    return true;
                }
                //Debug.LogError($"{this} : Задача по приоритету не доступна : {x}");
                return false;
            });
            if (ai.IsNullOrDefault())
            {
                Debug.LogException(new Exception($"{this} : Не удалось найти следующую задачу по приоритету!!!!'"));
            }
            return ai;
        }
        public int CountAvailable()
        {
            return AiStats.IsAlive ? _aiControlables.Count(x => x.TryStart()) : 0;
        }
        public IAiControlable SelectByCycles(IAiControlable fromAi, bool reverse)
        {
            if (!AiStats.IsAlive) return default;
            
            var checkList = new List<IAiControlable>();
            var countAi = _aiControlables.Count;
            var currentIndex = countAi - (fromAi?.Priority ?? CurrentAi?.Priority ?? 1); // текущий индекс
            var nextIndex = currentIndex + 1; // следующий индекс
            var lastPosition = nextIndex;     // текущая позиция
            var nextPosition = nextIndex + 1; // следующая позиция
            
            Debug.Log($"{this} : SelectByCycles [currentIndex = {currentIndex}], [Ai = '{_aiControlables[currentIndex]}'], [currentPosition = '{nextIndex}']");
            Debug.Log($"{this} : SelectByCycles [nextIndex = {nextIndex}], [nextPosition = {nextPosition}]");

            if (State.IsNullOrDefault() || nextIndex >= countAi || nextIndex <= 0)
            {
                Debug.Log($"{this} : SelectByCycles проверка с нуля : [State.IsNone() = {State.IsNullOrDefault()}], [currentPosition >= countAi {nextIndex >= countAi}], [currentPosition == 1 {nextIndex == 1}] ");
                checkList = _aiControlables;
            }
            else // переносит остаток задач в начало списка, можно проверить для всех задач начиная с текущей или с указаной задачи.
            {
                var countAtStart = countAi - lastPosition; // кол-во от следующей позиции до конца списка
                var countAtEnd = nextPosition;             // кол-во от начала до следующей позиции списка
                Debug.Log($"{this} : SelectByCycles перенос : [countAtStart = {countAtStart}], [countAtEnd {countAtEnd}]");

                var startRange = _aiControlables.GetRange(nextIndex, countAtStart);
                var endRange = _aiControlables.GetRange(0, countAtEnd);
                Debug.Log($"{this} : SelectByCycles перенос : [countAi = {countAi}] [startRange = {startRange.Count}], [endRange {endRange.Count}]");

                checkList.AddRange(startRange);
                checkList.AddRange(endRange);
                Debug.Log($"{this} : SelectByCycles перенос : [checkList = {checkList.Count}]");
            }

            if (reverse)
            {
                checkList.Reverse();
                foreach (var aiControlable in checkList)
                {
                    Debug.Log($"{this} : SelectByCycles Назад : [AI = '{aiControlable}']");
                }
            }
            else
            {
                foreach (var aiControlable in checkList)
                {
                    Debug.Log($"{this} : SelectByCycles Вперед : [AI = '{aiControlable}']");
                }
            }

            var ai = checkList.FirstOrDefault(x => x.TryStart());
            if (!ai.IsNullOrDefault())
            {
                Debug.Log($"{this} : Следующая задача по циклу : '{ai}'");
            }
            else
            {
                Debug.LogException(new Exception($"{this} : Не удалось найти следующую задачу по циклу!!!!'"));
            }
            return ai;
        }
        public IAiControlable GetAi(StateAnt aiType)
        {
            return _aiControlables.FirstOrDefault(ai => ai.AiType == aiType);
        }

        private bool AvailableTask(IAiControlable ai, out ATask task)
        {
            task = ai?.Task;
            return !task.IsNullOrDefault() && task.Status.IsCurrent();
        }
        public override string ToString()
        {
            return AntPresenter?.Name ?? "Владелец ИИ отсутствует";
        }
    }
}