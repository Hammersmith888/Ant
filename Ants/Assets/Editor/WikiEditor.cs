using UnityEditor;
using UnityEngine;


[CustomEditor(typeof( Wiki ))]
public class WikiEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button( "To lower case" ))
		{
			Wiki wiki		= (Wiki)target;

			wiki.SetHeader(
				wiki	.Header.Substring( 0, 1 )	.ToUpper() +
				wiki		.Header.Substring( 1 )						.ToLower()
			);

			EditorUtility.SetDirty( wiki );
		}
	}
}

