using System;
using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.Game;
using UnityEngine;


public abstract class A_MB_Placeable : Tapable
{
    public int PlaceNum => placeable.PlaceNum;
    public APlaceable Placeable => placeable;
    public Wiki Wiki => _wiki;

    protected SpriteRenderer SpriteRenderer => _spriteRenderer;

    public abstract string ResourceAmount { get; }
    public virtual float ResourceAmount_01 => 0;
    public abstract bool ShowCoin { get; }


#pragma warning disable 0649

    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Wiki _wiki;

    //[SerializeField] BuildProgress _buildProgress;

#pragma warning restore 0649


    protected abstract APlaceable placeable { get; }


    public abstract void Init(APlaceable placeable);


    protected virtual void Update()
    {
        SetBuildProgress();
    }


    protected override void OnTapped()
    {
        GameEvents.OnObjectTap?.Invoke(this);
    }


    public void Setup(Wiki wiki)
    {
        _wiki = wiki;
    }


    public void SetPlacePos()
    {
        var place = PlacesBook.GetPlace(PlaceNum, Placeable.Type, Placeable.SubType);
        var tr = place.transform;
        transform.position = transform.position.SetXY(tr.position);
        transform.localScale = tr.localScale;

        OnSetPlacePos();                                            // MonoBehaviour
        SetBuildProgress();
    }


    protected abstract void OnSetPlacePos();                        // MonoBehaviour


    public void OnPlaceSelected(APlace place)
    {
        //if (place.PlaceNum == PlaceNum)
            //return;

        //RemoveObjects.Instance.Replace(this, place.PlaceNum);
       // GameEvents.OnPlacedObject?.Invoke(placeable);
    }


    public void OnPlaceSelected_Yes(int placeNum)
    {
        OccupiedPlaces.Free(placeable);

        placeable.SetPlace(placeNum);                               // Serializable, MonoBehaviour
        placeable.OnReplaced();                                    // Serializable
    }


    protected void SetBuildProgress()
    {
        //if (!_buildProgress)
        //    return;
//
//
        //bool buildingInProgress = !placeable.IsReady;
//
        //_buildProgress.gameObject.SetActive(buildingInProgress);
//
        //if (buildingInProgress)
        //{
        //    _buildProgress.transform.localScale = Vector3.Scale(
        //                                                                    _buildProgress.transform.localScale.Abs(),
        //                                                                    new Vector3(transform.localScale.x < 0 ? -1 : 1, 1, 1)
        //    );
//
        //    _buildProgress.Set(placeable);
        //}


        //SetTransparency(buildingInProgress);
    }


    protected virtual void SetTransparency(bool buildingInProgress)
    {
        if(!_spriteRenderer) return;
        const float tolerance = .01f;

        float alpha = buildingInProgress ? .5f : 1;

        if (Math.Abs(_spriteRenderer.color.a - alpha) > tolerance)          // Stupid, but effective. Don't want to update SpriteRenderer.color without need.
        {
            _spriteRenderer.color = new Color(1, 1, 1, alpha);
        }
    }
}

