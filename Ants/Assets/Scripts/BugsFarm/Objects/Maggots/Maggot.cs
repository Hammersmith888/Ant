using System;
using System.Collections.Generic;
using BugsFarm.AstarGraph;
using BugsFarm.Game;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.SpeakerSystem;
using Zenject;
using Object = UnityEngine.Object;

[Serializable]
public class Maggot : IPostSpawnInitable, IPostLoadRestorable, IDisposable
{
    public bool CanSpeak => true;
    public double Age => SimulationOld.GetAge(_timeBorn);
    //public INode Position { get; private set; }
    public int Stage { get; private set; }
    
    [NonSerialized] private UnitPhrases unitTypePhrases;
    [NonSerialized] private MB_Maggot _mb;
    [NonSerialized] private float _stageTime;
    
    private static readonly List<( AntType, float )> _weights = new List<( AntType, float )>();
    private double _timeBorn;
    private AntType _antType;
    [NonSerialized] private MonoFactory _monoFactory;
    [NonSerialized] private static Data_Other _dataOther;
    public Maggot(bool dummy)
    {
        SetWeights();

        _timeBorn = SimulationOld.GameAge;
        _antType = Tools.RandomWeighted(_weights);
    }

    [Inject]
    private void Inject(MonoFactory monoFactory, Data_Other dataOther)
    {
        _monoFactory = monoFactory;
        _dataOther = dataOther;
    }
    //public void Init(MB_Maggot mb, PositionInfo gPos)
    //{
    //    _mb = mb;
    //    //Position = gPos;
//
    //    _stageTime = _dataOther.Other.Maggots.stageMinutes * 60;
    //    unitTypePhrases = unitTypePhrases = Data_Ants.Instance.GetData(AntType.Maggot).phrases;
    //    //mb.Init(this, gPos);
    //}
    public void PostLoadRestore()
    {
        // UpdateStage();		// (!) Wrong! Can lead to "InvalidOperationException: Collection was modified; enumeration operation may not execute." error!
        SetStage(); // OK
    }
    public void PostSpawnInit()
    {
        SetStage();
    }
    public void Update()
    {
        UpdateStage();
    }
    public void Dispose()
    {
        if (_mb)
        {
            Object.Destroy(_mb.gameObject);
        }
    }
    public void Say(SpeakerParams speakerParams, UISpeaker window, float delay)
    {
        //window.SetText(GetText(speakerParams));
        //_mb.SpeakerView.Say(window, delay);
    }
    private string GetText(SpeakerParams speakerParams)
    {
        return Tools.RandomItem(false ? Phrases.greetings : unitTypePhrases.idle);
    }
    private static void SetWeights()
    {
        _weights.Clear();

        foreach (var pair in _dataOther.Other.Maggots.probabilities)
            _weights.Add((pair.Key, pair.Value));
    }
    private void UpdateStage()
    {
        var stage = (int) (Age / _stageTime);
        var before = Stage;
        Stage = stage;

        if (Stage > 2)
            Born();
        else if (Stage != before)
            SetStage();
    }
    private void SetStage()
    {
        _mb?.SetStage(Stage);
    }
    private void Born()
    {
        //_monoFactory.Spawn_Ant(Position, _antType);

        Keeper.Destroy(this);

        GameEvents.OnAntTypeBorn?.Invoke(_antType);
    }
    
}