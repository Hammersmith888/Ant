using UnityEngine;
using UnityEngine.UI;


public class Panel_Info : APanel
{
	public static Panel_Info Instance { get; private set; }


#pragma warning disable 0649

    [SerializeField] Text	_text;

#pragma warning restore 0649


	protected override void Init( out bool isModal, out bool manualClose )
	{
		isModal			= false;
		manualClose		= false;

		Instance		= Tools.SingletonPattern( this, Instance );
	}


	public void OnClose()
	{
		Close();
	}


	public void SetText( string text )
	{
		_text.text		= text;
	}
}

