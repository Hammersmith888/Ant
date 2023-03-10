using DG.Tweening;
using UnityEngine;


public class TutorArrowAnim : MonoBehaviour
{
#pragma warning disable 0649

    [SerializeField] float		_shift;
    [SerializeField] float		_duration;
    [SerializeField] Ease		_ease;

#pragma warning restore 0649


	void Start()			=> TweenPointTo();

	
	void OnValidate()
	{
		/*
			OnValidate() called even if GameObject is disabled
			We don't want Tween() call before Start()
		*/
		if (!gameObject.activeSelf)
			return;

		TweenPointTo();
	}


	public void TweenDrag( Vector2 bgn, Vector2 end )
	{
		Vector3 end3D		= new Vector3( end.x, end.y, transform.position.z );

		transform.DOKill();
		transform.SetXY( bgn );
        transform
			.DOMove( end3D, 2 )
			.SetEase( Ease.InOutCubic )
			.SetLoops( -1, LoopType.Restart )
		;
	}


	public void TweenPointTo()
	{
		transform.DOKill();
		transform.localPosition		= Vector3.zero;
        transform
			.DOLocalMoveX( _shift, _duration )
			.SetEase( _ease )
			.SetLoops( -1, LoopType.Yoyo )
		;
	}
}

