using BugsFarm.AstarGraph;
using BugsFarm.Config;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;
using Zenject;

public class MonoFactory : MonoBehaviour
{
    [SerializeField] private Transform _rootAnts = null;
    [SerializeField] private Transform _rootObjects = null;
    [SerializeField] private Transform _antsSpawnPoint = null;
    private AntType _type;
    private int _id;
    private DiContainer _container;

    [Inject]
    private void Init(DiContainer container)
    {
        _container = container;
    }

    public APlaceable Spawn(int placeNum, ObjType type, int subType = 0)
    {
        APlaceable placeable;

        switch (type)
        {
            case ObjType.Food:
                var foodType = (FoodType) subType;

                switch (foodType)
                {
                    case FoodType.Garden:
                        placeable = new GardenPresenter(placeNum);
                        break;

                    case FoodType.DumpsterStock:
                        placeable = new DumpsterStockPresenter(placeNum);
                        break;

                    case FoodType.FoodStock:
                        placeable = new FoodStockPresenter(foodType, placeNum);
                        break;
                    case FoodType.FightStock:
                        placeable = new FightStockPresenter(placeNum);
                        break;
                    case FoodType.PileStock:
                        placeable = new PileStockPresenter(placeNum);
                        break;

                    default:
                        placeable = new Food(foodType, placeNum);
                        break;
                }

                break;

            case ObjType.str_Goldmine:
                placeable = new GoldminePresenter(placeNum);
                break;

            case ObjType.str_Pikes:
            case ObjType.str_ArrowTarget:
            case ObjType.str_OutDoorPear:
                placeable = new TrainingEquipment(type, placeNum,1);
                break;
            case ObjType.str_HangingPears:
            case ObjType.str_Swords:
                placeable = new TrainingEquipment(type, placeNum,2);
                break;
            case ObjType.HerbsStock:
                placeable = new HerbsStockPresenter(placeNum);
                break;
            case ObjType.Queen:
                placeable = new QueenPressenter(placeNum);
                break;
            case ObjType.Bowl:
                placeable = new BowlPresenter(placeNum);
                break;
            case ObjType.DigGroundStock:
                placeable = new DigGroundStock(placeNum);
                break;

            default:
                placeable = new Dummy(type, subType, placeNum);
                break;
        }

        PostSpawnInit(placeable);

        return placeable;
    }
    private void PostSpawnInit(APlaceable placeable)
    {
        // Keeper
        Keeper.Book(placeable);

        Create_MB(placeable);

        placeable.PostSpawnInit();
    }
    public void Create_MB(APlaceable placeable)
    {
        // MonoBehaviour
        var data = Data_Objects.Instance.GetData(placeable.Type, placeable.SubType);
        var mbPlaceable = Instantiate(data.prefab, _rootObjects);
        mbPlaceable.name = $"{data.prefab.name} {++_id}";
        mbPlaceable.Init(placeable);
        mbPlaceable.Setup(data.wiki);

        // Serializable
        _container.Inject(mbPlaceable);
        placeable.Init(mbPlaceable);
    }
   //public Maggot Spawn_Maggot(PositionInfo gPos)
   //{
   //    var maggot = new Maggot(false);

   //    Keeper.Book(maggot);
   //    Create_MB(maggot, gPos);
   //    maggot.PostSpawnInit();

   //    GameEvents.OnMaggotSpawned?.Invoke();

   //    return maggot;
   //}
    //public AntPresenter Spawn_Ant(PositionInfo gPos = null, AntType? type = null)
    //{
    //    type = type ?? GetAntType();
    //    var ant = new AntPresenter(type.Value);
//
    //    Keeper.Book(ant);
    //    //Create_MB(ant, gPos ?? new PositionInfo{Position = _antsSpawnPoint.position});
    //    ant.PostSpawnInit();
//
    //    return ant;
    //}
    public void Create_MB(Maggot maggot)
    {
        //Create_MB(maggot, maggot.Position);
    }
    public void Create_MB(AntPresenter ant)
    {
        //Create_MB(ant, null);
    }

    //private void Create_MB(Maggot maggot, PositionInfo gPos)
    //{
    //    // MonoBehaviour
    //    var prefab = Data_Ants.Instance.PrefabMaggot;
    //    var mbMaggot = Instantiate(prefab, Vector3.zero, Quaternion.identity, _rootObjects);
//
    //    // Serializable
    //    _container.Inject(maggot);
    //    //maggot.Init(mbMaggot, gPos);
    //}
   //private void Create_MB(AntPresenter ant, PositionInfo position)
   //{
   //    // MonoBehaviour
   //    var prefab = Data_Ants.Instance.GetData(ant.AntType).prefab;
   //    var mbAnt = Instantiate(prefab, Vector3.zero, Quaternion.identity, _rootAnts);

   //    // Serializable
   //    //ant.Init(_container, mbAnt, position);
   //}
    public void ResetAntType()
    {
        _type = AntType.None;
    }
    private AntType GetAntType()
    {
        _type++;

        if ( _type != AntType.Worker && _type != AntType.Pikeman ) 
            _type = AntType.None + 1;

        return _type;
    }
}