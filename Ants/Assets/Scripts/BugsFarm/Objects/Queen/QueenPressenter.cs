using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.SpeakerSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete.Components;
using Zenject;

[Serializable]
public class QueenPressenter : APlaceable_T<MB_Queen>
{
    public bool CanSpeak => true;
    public double Age => SimulationOld.GetAge(_tBorn);
    public Consumer Consumer => SmQueen.Consumer;
    public bool IsOccupied => _feeder != null;
    public SM_Queen SmQueen { get; } = new SM_Queen();
    public bool NeedsFeeder
    {
        get => Consumer.IsHungry && SmQueen.State == QueenState.Rest && !IsOccupied;
    }

    [NonSerialized] private UnitPhrases unitTypePhrases;
    [Inject] private MonoFactory MonoFactory { get; }
    [Inject] private Data_Other DataOther { get; }
    
    private double _tBorn;
    private SM_Feed _feeder;

    public QueenPressenter(int placeNum) : base(ObjType.Queen, 0, placeNum)
    {
        _tBorn = SimulationOld.GameAge;
    }

    public override void Init(A_MB_Placeable mb)
    {
        base.Init(mb);
        SmQueen.Init(DataOther, MonoFactory, Mb.Spine, this, _mb);
        unitTypePhrases = Data_Ants.Instance.GetData(AntType.Queen).phrases;
    }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        SmQueen.PostSpawnInit();
    }
    public override void PostLoadRestore()
    {
        base.PostLoadRestore();
        SmQueen.PostLoadRestore();
    }
    public void Occupy(SM_Feed feeder)
    {
        _feeder = feeder;
    }
    public void Free()
    {
        _feeder = null;
    }
    public void Eat()
    {
        SmQueen.Eat();
    }
    public void GiveBirth()
    {
        SmQueen.GiveBirth();
    }
    public override void Update()
    {
        base.Update();

        SmQueen.Update();
    }
    public void Say(SpeakerParams speakerParams, UISpeaker window, float delay)
    {
       // window.SetText(GetText(speakerParams));
        //_mb.SpeakerView.Say(window, delay);
    }

    private string GetText(SpeakerParams bParams )
    {
        IEnumerable<string> phrases;
        var noFood = !FindFood.ForEatAll().Any();
        if (noFood && SmQueen.Consumer.IsHungry)
        {
            phrases = unitTypePhrases.noFood;
        }
        else
        {
            phrases = false ? Phrases.greetings : unitTypePhrases.idle;
        }
        return Tools.RandomItem(phrases.ToArray());
    }
}