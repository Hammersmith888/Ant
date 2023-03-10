using BugsFarm.Services;
using BugsFarm.Views.Hack;
using Spine.Unity;
using UnityEngine;


public static class Squads
{
    public static readonly Squad Player = new Squad(true);

    public static void Spawn(AntService antService = null)
    {
       Player.Spawn(HackRefsView.Instance.FightView.ParentUnits, antService, false);
    }

    public static void SetMaterial(Squad squad, Material material)
    {
        foreach (Unit unit in squad.units)
            unit.MB_Unit.Material = SetMaterial(unit.MB_Unit.Spine, material);
    }


    public static Material SetMaterial(SkeletonAnimation skeletonAnimation, Material material)
    {
        // http://esotericsoftware.com/forum/Different-materials-for-same-Spine-character-5445

        Material original = skeletonAnimation.GetComponent<MeshRenderer>().sharedMaterial;
        Material individual = new Material(material);
        individual.mainTexture = original.mainTexture;

        //skeletonAnimation.CustomMaterialOverride[ original ]		= individual;

        return individual;
    }

    public static void CreateEnemy(Squad squad, int fightRoomIndex)
    {
        var rooms = Data_Fight.Instance.enemies.roomsConfig;
        int index = Mathf.Min(fightRoomIndex, rooms.Count - 1);

        foreach (var cfgRoomEnemy in rooms[index].enemies)
            squad.Add(cfgRoomEnemy.type, cfgRoomEnemy.level);

        Tools.Shuffle_FisherYates(squad.units);
    }
}