using System.Collections.Generic;
using BugsFarm.Views.Hack;
using UnityEngine;


public static class Arrows
{
    static HashSet<Arrow> arrows = new HashSet<Arrow>();

    private static Arrow GetFromPool() => Pools.Instance.Get(PoolType.Arrows,
        HackRefsView.Instance.FightView.ParentArrows).GetComponent<Arrow>();


    public static void Spawn(Vector2 startPos, Unit target, float damage)
    {
        Arrow arrow = GetFromPool();
        arrow.Init(startPos, target, damage);
        arrows.Add(arrow);
    }


    public static void Spawn(Vector2 startPos, Vector2 target)
    {
        Arrow arrow = GetFromPool();
        arrow.Init(startPos, target);
        arrows.Add(arrow);
    }


    public static void Destroy(Arrow arrow)
    {
        ReturnToPool(arrow);

        arrows.Remove(arrow);
    }


    public static void Clear()
    {
        foreach (Arrow arrow in arrows)
            ReturnToPool(arrow);

        arrows.Clear();
    }


    static void ReturnToPool(Arrow arrowOld)
    {
        arrowOld.ClearForReturnToPool();

        Pools.Return(PoolType.Arrows, arrowOld.gameObject);
    }
}