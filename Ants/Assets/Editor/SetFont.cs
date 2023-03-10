using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(
	fileName	=						ScrObjs.SetFont,
	menuName	= ScrObjs.folder +		ScrObjs.SetFont,
	order		=						ScrObjs.SetFont_i
)]
public class SetFont : ScriptableObject
{
	public Font	font;
}


[CustomEditor(typeof( SetFont ))]
public class SetFontEditor : Editor
{
	void Set( Font font )
	{
		Text[] textComponents			= Resources.FindObjectsOfTypeAll< Text >();

		foreach (Text text in textComponents)
			text.font					= font;
	}


	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();


		SetFont sf		= (SetFont)target;


		if (GUILayout.Button( "Set font" ))
        {
			Set( sf.font );

            Debug.Log( $"font: { sf.font.name }" );
        }		
	}
}

