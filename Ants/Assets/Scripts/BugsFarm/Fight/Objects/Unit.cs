using System;
using System.Reflection;
using BugsFarm.Game;
using BugsFarm.Services;
using BugsFarm.UnitSystem.Obsolete;
using BugsFarm.Views.Hack;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public class Unit
{
    public static MethodInfo mi_Update => Tools.GetMethodInfo<Unit>(x => x.Update());
    public static MethodInfo mi_Walk => Tools.GetMethodInfo<Unit>(x => x.Walk());
    public static MethodInfo mi_ExitFromCave => Tools.GetMethodInfo<Unit>(x => x.ExitFromCave());
    public static MethodInfo mi_PrepareToFight => Tools.GetMethodInfo<Unit>(x => x.PrepareToFight());
    public static MethodInfo mi_SelectTarget => Tools.GetMethodInfo<Unit>(x => x.SelectTarget());
    public static MethodInfo mi_Win => Tools.GetMethodInfo<Unit>(x => x.Win());
    public static MethodInfo mi_EnterCave => Tools.GetMethodInfo<Unit>(x => x.EnterCave());


    #region Properties

    public bool IsAlive => _ai_Unit.State != UnitState.Dead;
    public int Level => _level;

    public AI_Unit AI_Unit => _ai_Unit;

    float HP_cur
    {
        get => _HP_cur;
        set
        {
            _HP_cur = value;

            MB_Unit.SetHpBar(value / _HP_max);
        }
    }

    #endregion

    #region NonSerialized

    [NonSerialized] public MB_Unit MB_Unit;

    #endregion

    #region Serialized

    public bool IsPlayerSide;
    public AntType AntType;
    public AntPresenter Ant;
    public Vector2 PosStart;

    AI_Unit _ai_Unit;

    int _level;

    float _HP_cur;
    float _HP_max;

    #endregion


    public Unit(AntPresenter playerAnt) // Player
    {
        Ant = playerAnt;

        ConstructorInit(true, playerAnt.AntType, playerAnt.LevelP1);
    }


    public Unit(AntType type, int level) // Enemy
    {
        ConstructorInit(false, type, level);
    }


    void ConstructorInit(bool isPlayerSide, AntType type, int level)
    {
        IsPlayerSide = isPlayerSide; // (!) BEFORE _ai_Unit init
        AntType = type; // (!) BEFORE _ai_Unit init

        _ai_Unit =
            BattleService.AttackRange(type) > 0
                ? new AI_LongRangeUnit(this)
                : (AI_Unit) new AI_MeleeUnit(this);

        _level = level;

        float HP = Data_Fight.Instance.units[type].HP(level);
        _HP_cur = HP;
        _HP_max = HP;
    }


    public void Init(MB_Unit mb_Unit)
    {
        MB_Unit = mb_Unit;

        _ai_Unit.Init(mb_Unit);
    }


    public void Update() => _ai_Unit.Update();
    public void Walk() => _ai_Unit.TransitionExt(UnitState.Walk);
    public void ExitFromCave() => _ai_Unit.TransitionExt(UnitState.ExitFromCave);
    public void PrepareToFight() => _ai_Unit.TransitionExt(UnitState.PrepareToFight);
    public void SelectTarget() => _ai_Unit.TransitionExt(UnitState.SelectTarget);
    public void Win() => _ai_Unit.TransitionExt(UnitState.Win);

    public void EnterCave()
    {
        // Restore HP
        HP_cur = _HP_max;
        MB_Unit.HideHpBar();

        _ai_Unit.TransitionExt(UnitState.EnterCave);
    }


    public void TakeDamage(float damage)
    {
        SpawnDamageText(damage);

        HP_cur = Mathf.Max(HP_cur - damage, 0);

        if (
            HP_cur <= 0 &&
            _ai_Unit.State != UnitState.Dead
        )
            Die();
    }


    private void SpawnDamageText(float damage)
    {
        Vector3 randomX = Vector3.right * Random.Range(-1f, 1f) *
                          HackRefsView.Instance.BattleSettings.damageRandomShiftX;
        Vector3 randomY = Vector3.up * Random.Range(-1f, 1f) * HackRefsView.Instance.BattleSettings.damageRandomShiftY;
        Vector3 posBgn = MB_Unit.transform.position + randomX +
                         HackRefsView.Instance.BattleSettings.damageOffsetY0 * Vector3.up;
        Vector3 posTgt = MB_Unit.transform.position + randomX +
                         HackRefsView.Instance.BattleSettings.damageOffsetY1 * Vector3.up +
                         Vector3.forward + randomY;

        // Get from Pool
        TMP_Text text = Pools.Instance.Get(PoolType.DamageText,
                null)
            .GetComponent<TMP_Text>();

        // Setup
        text.transform.position = posBgn;
        text.transform.localScale = Vector3.one;
        text.text = damage.ToString("F0");
        text.color = IsPlayerSide
            ? HackRefsView.Instance.BattleSettings.colorDamagePlayer
            : HackRefsView.Instance.BattleSettings.colorDamageEnemy;

        // Tween
        text.transform
            .DOMove(posTgt, 1)
            .SetEase(HackRefsView.Instance.BattleSettings.damageEase)
            .OnComplete(() => Pools.Return(PoolType.DamageText, text.gameObject)) // Return to Pool
            ;
    }


    public void Die()
    {
        HP_cur = 0;

        _ai_Unit.TransitionExt(UnitState.Dead);

        _ai_Unit.MeleeGroupUnit.group.cb_Died(_ai_Unit.MeleeGroupUnit);

        GameEvents.OnUnitDied?.Invoke(this);
    }
}