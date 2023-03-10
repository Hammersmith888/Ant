using System;
using System.Collections.Generic;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

public enum AntType
{
    None,

    Worker = 1,
    
    Pikeman = 2,
    Archer = 5,
    Spider = 6,
    Snail = 7,

    Queen = 3,
    Maggot = 4,

    PotatoBug = 8,
    Worm = 9,
    Cockroach = 10,
    
    CaterpillarBoss = 11,
    Swordman = 12,
    Bedbug = 13,
    Fly = 14,
    MolBoss = 15,
    Mosquito = 16,
    Butterfly = 17,
    Firefly = 18,
    LadyBug = 19
}

public enum OtherAntParams
{
    None,

    AmmountCarry,
    WorkAmount,
}

[CreateAssetMenu(
                    fileName = ScrObjs.CfgAnt,
                    menuName = ScrObjs.folder + ScrObjs.CfgAnt,
                    order = ScrObjs.CfgAnt_i
                )]
public class CfgAnt : ScriptableObject
{
    [Serializable]
    public class TOther : SerializableDictionaryBase<OtherAntParams, float>{}

    [Serializable]
    public class DictTasks : SerializableDictionaryBase<StateAnt, int>{}

    [Header("Shop info")]
    public AntType antType;
    public bool isLocked;
    public int unlocksAfter;
    public Wiki wiki;
    public AntView prefab;
    public int price;
    public Currency currency;
    
    [Header("AI stats")]
    public FB_CfgAntConsumption consumption;
    public CfgAIMovement AIMovement = new CfgAIMovement{RotationSpeed = 1f, MovementSpeed = 1f, SurfaceRepeat = false};
    [SerializeField] protected DictTasks _tasks;

    [Header("Other")]
    public TOther other;
    public UnitPhrases phrases;

    public void Set(FB_CfgAnt fb)
    {
        price = fb.Price;
        currency = FB_Packer.CCY.ContainsKey(fb.Price_CCY) ? FB_Packer.CCY[fb.Price_CCY] : Currency.Coins;
        consumption = fb.Consumption;

        //TaskTime.Clear();
        //foreach (var pair2 in fb.TaskTime)
        //    TaskTime.Add(FB_Packer.Tasks[pair2.Key], pair2.Value);

        other.Clear();
        if (fb.Other != null)
            foreach (var pair2 in fb.Other)
                other.Add(FB_Packer.OthAntParams[pair2.Key], pair2.Value);
    }

    /// <summary>
    /// Вернет время задачи в секундах/минутах
    /// </summary>
    /// <param name="task"> Задача</param>
    /// <param name="inMinutes"> Если нужно выходные значения в минутах true, секундах false.</param>
    /// <returns></returns>
    public int GetTaskTime(StateAnt task, bool inMinutes = false)
    {
        if (_tasks.TryGetValue(task, out var value))
        {
            return value * (inMinutes ? 60 : 1);
        }
        return 0;
    }
    public int GetTaskCount()
    {
        return _tasks?.Count ?? 0;
    }
    public IEnumerable<StateAnt> GetTasks()
    {
        return _tasks?.Keys;
    }
    public void AddTask(StateAnt task, int timeSeconds)
    {
        if (_tasks.IsNullOrDefault())
        {
            _tasks = new DictTasks();
        }
        else if(!_tasks.ContainsKey(task))
        {
            _tasks.Add(task,timeSeconds);
        }
        else
        {
            Debug.LogError($"{this} : [{task}] - Задача уже существует!!");
        }
    }
}