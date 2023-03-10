using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.Game;
using UnityEngine;


[Serializable]
public class DigGroundStock : APlacableStock<MB_DigGround>
{
    public  IPosSide Point => _mb.Point;
    public override float QuantityCur { get; protected set; }
    public override float QuantityMax { get; protected set; }
    
    private int _stage = 0;

    public DigGroundStock(int placeNum) : base(ObjType.DigGroundStock, placeNum) { }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        SetRandomPlace(PlaceNum);
        SetSprite();
    }
    public override void PostLoadRestore()
    {
        base.PostLoadRestore();
        SetSprite();
    }
    public override void Reset()
    {
        QuantityCur = 0;
        _stage = 0;

        SetRandomPlace(PlaceNum);
        SetSprite();
    }
    public override void SetQuantity(float quantity)
    {
        QuantityCur = quantity;
    }
    public virtual void SetRandomPlace(int except)
    {
        var places = PlacesBook.GetPlaces(Type, SubType);
        var randomPlaceNums = new List<int>();
        foreach (var aPlace in places)
        {
            //if (aPlace.PlaceNum != except && OccupiedPlaces.IsFree(aPlace.PlaceNum, Type))
            //{
                //randomPlaceNums.Add(aPlace.PlaceNum);
            //}
        }

        if (!randomPlaceNums.Any())
        {
            return;
        }

        var placeNum = Tools.RandomItem(randomPlaceNums);

        Mb.OnPlaceSelected_Yes(placeNum);
    }
    public override float Add(float quantity)
    {
        var old = QuantityCur;
        QuantityCur += quantity;
        _stage = Mathf.Max(_stage, 1);

        SetSprite();
        return old - QuantityCur;
    }
    public override void Restock(){}
    protected virtual void SetSprite()
    {
        var stage = 0;
        //var digPlace = GameInit.FarmServices.Rooms.CurrentWork;
        //if (QuantityCur > 0 && !digPlace.IsNullOrDefault())
        //{
        //    QuantityMax = digPlace.QuantityMax;
        //    var quantity01 = QuantityCur / QuantityMax;
        //    stage = Mathf.CeilToInt(4 * quantity01);
        //    stage = Mathf.Min(stage, 4);
        //}
//
        //_stage = Mathf.Max(_stage, stage);
//
        //_mb.SetSprite(_stage);
    }
}