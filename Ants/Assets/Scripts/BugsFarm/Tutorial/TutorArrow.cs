using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using UnityEngine;


public class TutorArrow : MB_Singleton<TutorArrow>
{
#pragma warning disable 0649

    [SerializeField] TutorArrowAnim _tutorArrowAnim;

#pragma warning restore 0649


    public void PointTo(ObjType type, int subType, int placeNum)
    =>
        PointTo(PlacesBook.GetPlace(placeNum, type, subType));


    public void PointTo(APlace place)
    =>
        PointTo(place.transform);


    public void PointTo(Transform transform, bool flipX = false, bool flipY = false)
    =>
        PointTo(transform.position, flipX, flipY);


    public void PointTo(Vector2 pos, bool flipX = false, bool flipY = false)
    {
        int FlipMul(bool flip) => flip ? -1 : 1;

        _tutorArrowAnim.gameObject.SetActive(true);
        _tutorArrowAnim.TweenPointTo();

        transform.SetXY(pos);
        transform.localScale = new Vector3(FlipMul(flipX), FlipMul(flipY), 1);

        float localScaleY = Mathf.Abs(_tutorArrowAnim.transform.localScale.y) * FlipMul(flipY);
        _tutorArrowAnim.transform.localScale = _tutorArrowAnim.transform.localScale.SetY(localScaleY);
    }


    public void TweenDrag(Vector2 bgn, Vector2 end)
    {
        _tutorArrowAnim.gameObject.SetActive(true);
        _tutorArrowAnim.TweenDrag(bgn, end);
    }


    public void Hide() => _tutorArrowAnim.gameObject.SetActive(false);
}

