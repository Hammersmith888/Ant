using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.Objects.Stock.Utils;

public class RemoveObjects : MB_Singleton<RemoveObjects>
{
    readonly HashSet<APlaceable> _occupants = new HashSet<APlaceable>();
    readonly List<APlaceable> _digGrounds = new List<APlaceable>();
    readonly List<APlaceable> _stocks = new List<APlaceable>();
    readonly List<int> _excluding = new List<int>();


    bool _isYes;


    public void Replace(A_MB_Placeable mb_Placeable, int target)
    {
        StartCoroutine(CorReplace(target, mb_Placeable));
    }


    public void Buy(int placeNum, CfgObject data)
    {
        StartCoroutine(CorBuy(placeNum, data));
    }


    IEnumerator CorReplace(int target, A_MB_Placeable mb_Placeable)
    {
        if (!CheckOccupants(target, mb_Placeable.Placeable.Type))
            yield return StartCoroutine(RemoveObjectsDialog(target, mb_Placeable.Wiki));

        if (_isYes)
        {
            mb_Placeable.OnPlaceSelected_Yes(target);
            Sounds.PlayPutSound(mb_Placeable.Placeable);
        }
    }


    IEnumerator CorBuy(int placeNum, CfgObject data)
    {
        if (!CheckOccupants(placeNum, data.type))
            yield return StartCoroutine(RemoveObjectsDialog(placeNum, data.wiki));

        if (_isYes)
            Panel_FarmMenu.Instance.Buy(placeNum, data);
    }


    bool CheckOccupants(int placeNum, ObjType type)
    {
        _isYes = true;


        Func<APlaceable, bool> pr_DigGr = x => x.Type == ObjType.DigGroundStock ;
        Func<APlaceable, bool> pr_Stock = x => x.Type == ObjType.Food &&
                                                 ((FoodType)x.SubType == FoodType.FoodStock ||
                                                  (FoodType)x.SubType == FoodType.FightStock ||
                                                  (FoodType)x.SubType == FoodType.PileStock ) &&
                                                ((FoodStockPresenter)x).QuantityCur <= 0 ;

        OccupiedPlaces.GetOccupants(placeNum, type, _occupants);
        _digGrounds.Clear();
        _stocks.Clear();
        _digGrounds.AddRange(_occupants.Where(pr_DigGr));
        _stocks.AddRange(_occupants.Where(pr_Stock));
        _occupants.ExceptWith(_digGrounds);
        _occupants.ExceptWith(_stocks);


        _excluding.Clear();
        _excluding.Add(placeNum);
        if (OccupiedPlaces.NeighborsAffected(placeNum, type))
            _excluding.AddRange(OccupiedPlaces.Neighbors[placeNum]);


        if (_occupants.Count == 0)
        {
            ReplaceRemove_Specials(placeNum);
            return true;
        }

        return false;
    }


    IEnumerator RemoveObjectsDialog(int placeNum, Wiki wiki)
    {
        string occupant =
                                    _occupants.Count == 1 ?
                                    _occupants.First().Header :
                                    Texts.Objects
        ;
        string header = occupant;
        string description = string.Format(Texts.RemoveObjects, $"\"{ occupant }\"", $"\"{ wiki.Header }\"");

        Panel_YesNo.Instance.OpenDialog(header, description);

        yield return StartCoroutine(Panel_YesNo.Instance.WaitUntilClosed());

        _isYes = Panel_YesNo.IsYes;


        if (_isYes)
        {
            ReplaceRemove_Specials(placeNum);
            Remove_Occupants();
        }
    }


    void ReplaceRemove_Specials(int placeNum)
    {
        foreach (DigGroundStock digGround in _digGrounds)
        {
            digGround.Reset();
            digGround.SetRandomPlace(placeNum);
        }


        foreach (Food stock in _stocks)
        {
            if (Stock.TryFindPlace(stock.Type, stock.SubType, out var newPlaceNum, _excluding))
                stock.MB_Food.OnPlaceSelected_Yes(newPlaceNum);
            else
                Keeper.Destroy(stock);
        }
    }


    void Remove_Occupants()
    {
        foreach (APlaceable occupant in _occupants)
            Keeper.Destroy(occupant);
    }
}

