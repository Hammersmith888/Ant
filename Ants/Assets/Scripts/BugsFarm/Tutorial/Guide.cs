using System.Collections;
using BugsFarm.Game;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;


public class Guide : APanel
{
	public static Guide Instance { get; private set; }

	public bool GuideEnabled		{ set => _spine.gameObject.SetActive( value ); }

	public const string GreatAnim		= "Great";
	public const string IdeAnim			= "Idle";


#pragma warning disable 0649
	
	public GameObject						_button_TutorialSkip;

	[SerializeField] SkeletonGraphic		_spine;
	[SerializeField] GameObject				_textPanel;
	[SerializeField] Text					_text;

#pragma warning restore 0649


	AnimPlayer		_player;


	protected override void Init( out bool isModal, out bool manualClose )
	{
		isModal			= false;
		manualClose		= false;

		Instance		= Tools.SingletonPattern( this, Instance );

		_player			= new AnimPlayer( _spine );
	}


	void Update()
	{
		if (_player.IsAnimComplete && _player.AnimName == GreatAnim)
		{
			_player.Play( IdeAnim, true );

			GameEvents.OnTutorialGreatAnimDone?.Invoke();
		}
	}


	public void OnTutorialSkip()		=> Tutorial.Instance.SkipTutor();


	public void HideTextPanel()			=> _textPanel.SetActive( false );


	public void Great()					=> _player.Play( GreatAnim, false );


	public void SetText( string text )
	{
		_textPanel.SetActive( true );

		StopAllCoroutines();
		StartCoroutine( TypeText( text ) );
	}


	IEnumerator TypeText( string text )
	{
		for (int i = 0; i <= text.Length; i ++)
		{
			string visible			= text.Substring( 0, i );
			string invisible		= text.Substring( i );

			_text.text				= $"{ visible }<Color=transparent>{ invisible }</Color>";

			yield return null;
		}
	}
}

