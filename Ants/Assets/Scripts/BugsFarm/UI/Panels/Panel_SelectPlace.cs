using System;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using UnityEngine;

public class Panel_SelectPlace : APanel
{
    public GameObject buttonCancel;
    public static Panel_SelectPlace Instance { get; private set; }
    private static APlace[] _places;
    private static A_MB_Placeable _mb_Placeable;

    public static void SetPlaces(A_MB_Placeable mb_Placeable)
    {
        _mb_Placeable = mb_Placeable;
        var type = mb_Placeable.Placeable.Type;
        var places = PlacesBook.GetPlaces(type, mb_Placeable.Placeable.SubType);

        SetPlaces(places, mb_Placeable.OnPlaceSelected, type, mb_Placeable.PlaceNum);
    }

    public static void SetPlaces(APlace[] places, Action<APlace> cb_Tapped, ObjType type, int? currentPlaceNum = null)
    {
        _places = places;

        int? queenPlace = Keeper.GetObjects(ObjType.Queen).FirstOrDefault()?.PlaceNum;

        foreach (var place in places)
        {
            //int placeNum = place.PlaceNum;
//
            //if (!queenPlace.HasValue || placeNum != queenPlace)
            //{
            //    bool visible = placeNum != currentPlaceNum;
//
            //    //place.Activate(cb_Tapped, visible);
            //}
        }
    }

    public void OnCancel()
    {
        Close();
    }

    public void OnDestroyed(A_MB_Placeable placeable)
    {
        if (_mb_Placeable == placeable)
        {
            _mb_Placeable = null;

            if (gameObject.activeSelf)
                Close();
        }
    }

    private void SetCollidersAllowed(bool allowed)
    {
        Keeper.Ants.ForEach(x => x.SetCollidersAllowed(allowed));

        foreach (var pair in Keeper.Objects)
            pair.Value.ForEach(x => x.SetColliderAllowed(allowed));
    }

    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = true;
        manualClose = false;

        Instance = Tools.SingletonPattern(this, Instance);
    }

    protected override void OnOpened()
    {
        SetCollidersAllowed(false);
    }

    protected override void OnClosed()
    {
        SetCollidersAllowed(true);

        foreach (var place in _places)
        {
            place.gameObject.SetActive(false);
        }

        _places = null;
    }
}