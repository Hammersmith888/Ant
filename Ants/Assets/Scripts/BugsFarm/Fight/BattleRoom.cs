using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class BattleRoom : MonoBehaviour
{
    [SerializeField] private Transform exitFromCaveStart;
    [SerializeField] private Transform exitFromCaveEnd;
    [SerializeField] private Transform enterToCaveEnd;
    [SerializeField] private Transform enemyStart;
    [SerializeField] private Transform enemyRight;
    [SerializeField] private Transform enemyLeft;
    [SerializeField] private Transform enemyCenter;
    [SerializeField] private SkeletonAnimation chest;
    [SerializeField] private Transform[] enemyStartPoints;
    [SerializeField] private GameObject[] props;
    [SerializeField] private GameObject glass;
    [SerializeField] private SpriteMask[] spriteMasks;

    //
    public SpriteRenderer Sprite;

    public GameObject LevelBG;
    public Text Level;
    public CanvasGroup Logo;

    public Transform EnemiesParent;
    public SpriteRenderer Stock;

    public Transform PosRight;
    public Transform PosExit;

    [NonSerialized] public Material MaterialRoomDark;
    [NonSerialized] public Material MaterialRoom;
    [NonSerialized] public Material MaterialUnitsDark;
    [NonSerialized] public Material MaterialChest;

    [NonSerialized] public Squad Enemies;

    public Transform ExitFromCaveStart => exitFromCaveStart;
    public Transform ExitFromCaveEnd => exitFromCaveEnd;
    public Transform EnterToCaveEnd => enterToCaveEnd;
    public Transform EnemyStart => enemyStart;
    public SkeletonAnimation Chest => chest;
    public Transform EnemyRight => enemyRight;
    public Transform EnemyLeft => enemyLeft;
    public Transform EnemyCenter => enemyCenter;

    private void Update() => Enemies?.AllDo(Unit.mi_Update);

    public void SetMaterials(Material materialRoomDark, Material materialUnitsDark)
    {
        MaterialRoomDark = materialRoomDark;
        MaterialUnitsDark = materialUnitsDark;

        Material materialRoom = new Material(materialRoomDark);
        MaterialRoom = materialRoom;
        Sprite.material = materialRoom;
        Stock.material = materialRoom;

        MaterialChest = Squads.SetMaterial(chest, materialUnitsDark);
    }

    float _darkness;

    public float Darkness
    {
        get => _darkness;
        set
        {
            _darkness = value;

            float desaturate = value * MaterialRoomDark.GetFloat("_Desaturate");
            float darken = value * MaterialRoomDark.GetFloat("_Darken");
            Color fog = MaterialUnitsDark.GetColor("_Color");
            fog.a = value;

            // Room and Stock
            MaterialRoom.SetFloat("_Desaturate", desaturate);
            MaterialRoom.SetFloat("_Darken", darken);

            // Enemies
            if (Enemies != null)
                foreach (Unit unit in Enemies.units)
                    unit.MB_Unit.Material.SetColor("_Color", fog);

            // Chest
            MaterialChest.SetColor("_Color", fog);
        }
    }

    public Transform[] EnemyStartPoints => enemyStartPoints;

    public GameObject[] Props => props;

    public GameObject Glass => glass;

    public SpriteMask[] SpriteMasks => spriteMasks;
}