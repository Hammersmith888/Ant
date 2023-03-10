using UnityEngine;


[CreateAssetMenu(
	fileName	=						ScrObjs.Wiki,
	menuName	= ScrObjs.folder +		ScrObjs.Wiki,
	order		=						ScrObjs.Wiki_i
)]
public class Wiki : ScriptableObject
{
	public string	Header				=> _header;
	public string	Description			=> _description;
	public string	wiki				=> _wiki;

	public Sprite	Icon				=> _icon;


#pragma warning disable 0649

	[SerializeField] string		_header;
	[SerializeField] string		_description;
	[SerializeField] string		_wiki;

	[SerializeField] Sprite		_icon;

#pragma warning restore 0649


	public void SetHeader( string text )
	{
		_header		= text;
	}
}

