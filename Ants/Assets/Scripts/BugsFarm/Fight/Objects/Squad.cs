using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using BugsFarm.Game;
using BugsFarm.Services;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public class Squad : IDisposable
{
    const float PairGap_Player = .3f;
    const float PairGap_Enemy = .6f;

    public Action OnSquadDead;
    public readonly bool IsPlayerSide;
    public readonly List<Unit> units = new List<Unit>();

    public Squad(bool isPlayerSide)
    {
        IsPlayerSide = isPlayerSide;

        GameEvents.OnUnitDied += OnUnitDied;
    }

    public void Dispose()
    {
        GameEvents.OnUnitDied -= OnUnitDied;
    }

    public void Add(AntPresenter playerAnt) => units.Add(new Unit(playerAnt)); // Player
    public void Add(AntType type, int level) => units.Add(new Unit(type, level)); // Enemy	

    public List<AntEntity> Spawn(Transform parent, AntService antService, bool isEnemy)
    {
        var entities = new List<AntEntity>();
        
        foreach (Unit unit in units)
        {
            MB_Unit prefab = Data_Fight.Instance.units[unit.AntType].prefab;
            MB_Unit mb = GameObject.Instantiate(prefab, parent);

            mb.Unit = unit; // (!) BEFORE unit.Init( mb )
            unit.Init(mb);

            var entity = antService?.RegisterEntity(mb, unit.AntType, true, isEnemy);
            var room = mb.GetComponentInParent<BattleRoom>();

            entity.AddRoom(room);
            entity.isPatrol = true;
            entity.transform.Value.position = room.EnemyStart.position;
            
            entities.Add(entity);
        }

        return entities;
    }

    public void SetPositions(Transform pos1, Transform pos2 = null)
    {
        Transform startPos = pos1;
        Vector2 pos = startPos.position;
        int dir = IsPlayerSide ? 1 : -1;
        float squadLength = 0;

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];
            MB_Unit mb = unit.MB_Unit;

            pos += i == 0 ? Vector2.zero : Vector2.right * dir * mb.ExtentBackward;
            Vector2 unitPosStart = pos2 == null ? pos : Vector2.Lerp(pos1.position, pos2.position, Random.value);
            unit.PosStart = unitPosStart;
            pos += Vector2.right * dir * mb.ExtentForward;
            float addon = i == 1 ? (IsPlayerSide ? PairGap_Player : PairGap_Enemy) : 0; // so they standing by pairs
            pos += Vector2.right * dir * addon;
            squadLength += mb.Extent + addon;

            if (pos2)
                mb.SetLookDir(Tools.RandomBool());
        }
    }

    public void Clear()
    {
        foreach (Unit unit in units)
            GameObject.Destroy(unit.MB_Unit.gameObject);

        units.Clear();
    }

    public void AllDo(MethodInfo methodInfo)
    {
        // https://stackoverflow.com/questions/9382216/get-methodinfo-from-a-method-reference-c-sharp
        // https://stackoverflow.com/questions/2202381/reflection-how-to-invoke-method-with-parameters

        foreach (Unit unit in units)
            methodInfo.Invoke(unit, null);
    }

    void OnUnitDied(Unit unit)
    {
        if (
            unit.IsPlayerSide == IsPlayerSide &&
            units.Count(x => x.IsAlive) == 0
        )
            OnSquadDead?.Invoke();
    }
}