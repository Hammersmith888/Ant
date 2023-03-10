using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;
using UnityEngine.UI;


public class MyBug : MonoBehaviour
{
#pragma warning disable 0649

    [SerializeField] Image		_icon;
    [SerializeField] Text		_level;

#pragma warning restore 0649

	AntPresenter		_ant;


	public void Set( AntPresenter ant )
	{
		bool active		= ant != null;

		_ant			= ant;

		gameObject.SetActive( active );

		if (active)
		{
			_icon.sprite		= Data_Ants.Instance.GetData( ant.AntType ).wiki.Icon;
			_level.text			= ant.LevelP1.ToString();
		}
	}


	public void OnClick()
	{
		BugInfo.Instance.Set( _ant );
	}
}

