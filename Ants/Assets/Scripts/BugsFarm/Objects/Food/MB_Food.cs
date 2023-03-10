using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using UnityEngine;


public class MB_Food : A_MB_Consumable
{
    public FoodGarbage Garbage => _garbage;


#pragma warning disable 0649

    [SerializeField] FoodGarbage _garbage;

#pragma warning restore 0649


    Food _food;


    protected string ResourceAmountConsumable => base.ResourceAmount;
    public override string ResourceAmount => _food.IsGarbage ? $"{ Mathf.RoundToInt(_food.QuantityCur) } ед." : ResourceAmountConsumable;

    protected override AConsumable consumable => _food;
    public override void Init(APlaceable food)
    {
        _food = (Food)food;
    }


    public virtual void SetSprite(Sprite sprite)
    {
        SpriteRenderer.sprite = sprite;
    }


    public void SetGarbage()
    {
        // TouchCollider.SetAllow(false);

        SpriteRenderer.gameObject.SetActive(false);
        _garbage.gameObject.SetActive(true);
    }


    protected override void OnSetPlacePos()
    {
        APlace place = PlacesBook.GetPlace(PlaceNum, Placeable.Type, Placeable.SubType);
        transform.rotation = place.transform.rotation;

        base.OnSetPlacePos();
    }


    // public override void SetColliderAllowed(bool allowed)
    // {
    //     // base.SetColliderAllowed(allowed);
    //     //
    //     // _garbage?.SetColliderAllowed(allowed);
    // }
}

