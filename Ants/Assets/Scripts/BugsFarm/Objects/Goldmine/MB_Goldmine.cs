using System;
using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

public class MB_Goldmine : A_MB_Placeable_T<GoldminePresenter>
{
    public override string ResourceAmount => $"{_placeable.GoldCur}";
    public override float ResourceAmount_01 => (float) _placeable.GoldCur / _placeable.GoldMax;
    public override bool ShowCoin => true;

    public GoldminePresenter Goldmine => _placeable;
    public IPosSide PointLever => _posSideLever;
    public IPosSide PointRock => _posSideRock;
    public IPosSide[] PointsBuidling => _posSidesBuilding;
    public bool IsAnimComplete => _animPlayer.IsAnimComplete;
    public event Action<Event> OnSpineEvent;

#pragma warning disable 0649

    [SerializeField] SkeletonAnimation _spine;
    [SerializeField] SpriteRenderer _rocks;

    [SerializeField] MB_PosSide _posSideLever;
    [SerializeField] MB_PosSide _posSideRock;
    [SerializeField] MB_PosSide[] _posSidesBuilding;

    //[SerializeField] UIGoldmineOld _bubble;

#pragma warning restore 0649

    AnimPlayer _animPlayer;

    public override void Init(APlaceable goldmine)
    {
        base.Init(goldmine);

        _animPlayer = new AnimPlayer(_spine);

        _spine.AnimationState.Event += HandleSpineEvent;
    }

    protected override void OnSetPlacePos()
    {
        SetPlaceTransforms();
    }

    public void Simulate()
    {
        _animPlayer.Simulate();
    }

    public void Idle()
    {
        _animPlayer.Play("Idle", true);
    }

    public void Lever()
    {
        _animPlayer.Play("Opyskanie", false);
    }

    void SetPlaceTransforms()
    {
        var place = PlacesBook.GetPlace<GoldminePlace>(PlaceNum);

        var flipped = transform.localScale.x < 0;

        _rocks.transform.position = place.Rocks.transform.position;
        _posSideRock.Position = place.PointRock.Position;

        //_bubble.transform.localScale =
        //    Vector3.Scale(_bubble.transform.localScale.Abs(), new Vector3(flipped ? -1 : 1, 1, 1));
    }

    protected override void SetTransparency(bool buildingInProgress)
    {
        const float tolerance = .01f;

        var alpha = buildingInProgress ? .5f : 1;

        if (Math.Abs(_spine.skeleton.A - alpha) > tolerance
            ) // Stupid, but effective. Don't want to update SpriteRenderer.color without need.
        {
            _spine.skeleton.A = alpha;
            _rocks.color = new Color(1, 1, 1, alpha);
        }
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        OnSpineEvent?.Invoke(e);
    }
}