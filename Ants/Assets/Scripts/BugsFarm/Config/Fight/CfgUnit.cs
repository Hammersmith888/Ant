using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;

[CreateAssetMenu( fileName = ScrObjs.CfgUnit, menuName = ScrObjs.folder + ScrObjs.CfgUnit, order = ScrObjs.CfgUnit_i)]
public class CfgUnit : ScriptableObject
{
    public MB_Unit prefab;
    public AnimRefs_Ant animations;
    public float SpeedToAnimationTimeScaleMul;
    public float Cooldown;
    public float Speed;
    public float expBase;
    public float mulAttack;
    public float mulHP;
    public float Food;
    public float XPprice;

    [Header("New Fight Balance")] 
    public Vector2Int HP_Range;
    public float HP_C;
    public Vector2Int Attack_Range;
    public float Attack_C;

    public float HP(int level)
    {
        return CalcHP(level);
    }
    public float Attack(int level)
    {
        return CalcAttack(level);
    }
    public float CalcHP(int level)
    {
        return CalcUnitCurve(level, HP_Range, HP_C);
    }
    public float CalcAttack(int level)
    {
        return CalcUnitCurve(level, Attack_Range, Attack_C);
    }
    public float CalcDPS(int level)
    {
        return CalcAttack(level) / Cooldown;
    }
    public float CalcPower(int level)
    {
        return CalcHP(level) * CalcDPS(level);
    }
    public static float CalcExpCurve(float x, Vector2 rangeX, Vector2 rangeY, float c)
    {
        return rangeY.x +                                   // A + B
               (rangeY.y - rangeY.x) / (Mathf.Exp(c) - 1) * // B
               (Mathf.Exp(c * (x - rangeX.x) / (rangeX.y - rangeX.x)) - 1);
    }
    
    private float CalcUnitCurve(int level, Vector2Int range, float C)
    {
        return CalcExpCurve(level, new Vector2(1, Constants.MaxUnitLevel), range, C);
    }
}