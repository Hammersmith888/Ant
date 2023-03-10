using System;
using System.Linq;
using BugsFarm.AstarGraph;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using Spine.Unity;


public enum QueenState
{
    None,

    Rest,
    Eat,
    GiveBirth,
    Sleep,
}


[Serializable]
public class SM_Queen : AStateMachine<QueenState>, IPostSpawnInitable, IPostLoadRestorable
{
    #region Getters

    public Consumer Consumer => _consumer;
    public Timer TimerPregnancy => _timerPregnancy;

    #endregion
    #region NonSerialized

    [NonSerialized] FB_CfgQueen _data;
    [NonSerialized] QueenPressenter _queen;
    [NonSerialized] MB_Queen _mb_Queen;
    [NonSerialized] AnimPlayer _player;

    #endregion
    #region Serialized

    private Consumer _consumer = new Consumer();
    private Timer _timerPregnancy = new Timer();
    private Timer _timerEating = new Timer();
    private Timer _timerSleep = new Timer();
    private Timer _timerAwake = new Timer();

    private int _maggotsToBirth;
    private int _maggotPoint;

    #endregion

        [NonSerialized] private MonoFactory _monoFactory;
    public void Init(Data_Other dataOther, MonoFactory monoFactory, SkeletonAnimation spine, QueenPressenter queen, MB_Queen mb_Queen)
    {
        _data = dataOther.Other.Queen;
        _queen = queen;
        _mb_Queen = mb_Queen;
        _player = new AnimPlayer(spine);
        _monoFactory = monoFactory;
        _consumer.Init(_data.Consumption.FoodNeedTime, _data.Consumption.DaysWithoutFoodAndWater, true);
    }


    public void PostSpawnInit()
    {
        _consumer.PostSpawnInit();

        SetAwakeTimer(true);
        SetPregnancyTimer();
    }


    public void PostLoadRestore()
    {
        SetAnim();
    }


    public void Eat()
    {
        _consumer.ConsumptionStart();
        _consumer.ConsumptionEnd(1, 1);
        Keeper.Stats.cb_FoodEaten(_data.Consumption.FoodConsumption);
        Transition(QueenState.Eat);
    }


    public void GiveBirth()
    {
        CalcMaggotsToBirth();

        Transition(QueenState.GiveBirth);
    }


    public override void Update()
    {
        if (State != QueenState.Eat)
            _consumer.Update();

        CheckTransitions();
    }


    bool ShouldSleep()
    {
        return
            !_queen.IsOccupied &&
            _timerAwake.IsReady
        ;
    }


    void CheckTransitions()
    {
        switch (State)
        {
            case QueenState.None:
                Transition(QueenState.Rest);
                break;

            case QueenState.Eat:
                if (_timerEating.IsReady)
                {
                    Transition(QueenState.Rest);
                    CheckTransitions();                 // GiveBirth, if ready. Don't wait for 1 frame in Rest state. Otherwise, it looks strange in Info panel.
                }
                break;

            case QueenState.Sleep:
                if (_timerSleep.IsReady)
                {
                    SetAwakeTimer(false);

                    Transition(QueenState.Rest);
                }
                break;

            case QueenState.Rest:
                if (ShouldSleep())
                {
                    Transition(QueenState.Sleep);
                }
                else if (
                        !_consumer.IsHungry &&
                        _timerPregnancy.IsReady &&
                        CalcMaggotsToBirth()
                    )
                    Transition(QueenState.GiveBirth);
                break;

            case QueenState.GiveBirth:
                if (_player.IsAnimComplete)
                {
                    _maggotsToBirth--;

                    var gPos = _mb_Queen.MaggotPoints[(_maggotPoint++) % _mb_Queen.MaggotPoints.Length];
                    //var covertedNode = new PositionInfo() { Position = gPos.Position };
                    //_monoFactory.Spawn_Maggot(covertedNode);

                    if (_maggotsToBirth == 0)
                    {
                        SetPregnancyTimer();

                        Transition(QueenState.Rest);
                    }
                    else
                    {
                        SetAnim();
                    }
                }
                break;
        }
    }


    protected override void OnEnter()
    {
        switch (State)
        {
            case QueenState.Rest: OnEnter_Rest(); break;
            case QueenState.Eat: OnEnter_Eat(); break;
            case QueenState.GiveBirth: OnEnter_GiveBirth(); break;
            case QueenState.Sleep: OnEnter_Sleep(); break;
        }
    }


    void OnEnter_Rest()
    {
        SetAnim();
    }


    void OnEnter_Sleep()
    {
        _timerSleep.Set(_data.SleepHours * 60 * 60);

        SetAnim();
    }


    void OnEnter_Eat()
    {
        _timerEating.Set(Constants.QueenFoodConsumptionTime);

        SetAnim();
    }


    void OnEnter_GiveBirth()
    {
        SetAnim();
    }


    bool CalcMaggotsToBirth()
    {
        var rooms = 0;//BugsFarm.Game.GameInit.FarmServices.Rooms.OpenedCount;
        var ants = Keeper.Ants.Count(x => x.IsAlive) + Keeper.Maggots.Count;
        var ratio = (float)ants / rooms;

        _maggotsToBirth = 0;

        foreach (var er in _data.Eggs)
        {
            if (ratio <= er.ratio)
            {
                _maggotsToBirth = er.eggs;
                break;
            }
        }

        return _maggotsToBirth > 0;
    }


    void SetAwakeTimer(bool randomForward)
    {
        GameTools.SetAwakeTimer(_timerAwake, _data.SleepHours * 60, randomForward);
    }


    void SetPregnancyTimer()
    {
        _timerPregnancy.Set(_data.PregnancyHours * 60 * 60);
    }


    void SetAnim()
    {
        GetAnim(out string anim, out bool loop);

        if (anim != null)
            _player.Play(anim, loop);
    }


    void GetAnim(out string anim, out bool loop)
    {
        switch (State)
        {
            case QueenState.Rest: anim = "breath"; loop = true; break;
            case QueenState.Eat: anim = "eat"; loop = true; break;
            case QueenState.GiveBirth: anim = "birth"; loop = false; break;
            case QueenState.Sleep: anim = "sleep"; loop = true; break;

            default: anim = null; loop = false; break;
        }
    }


    public void WakeUp()
    {
        _timerSleep.Set(0);
    }
}

