using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace EditorMode
{

class SelectType : ScriptableObject
{
	[MenuItem( "Selection/Select sprites only %q" )]
	static void SelectSpritesOnly()
	{
		Object[] selected			= Selection.GetFiltered(
																typeof( GameObject ),

																SelectionMode.Editable
																// | SelectionMode.TopLevel
		);

		List< Object > objects		= new List< Object >();

		foreach (Object obj in selected)
			if (
					obj is GameObject &&
					((GameObject)obj).GetComponent< SpriteRenderer >()
				)
				objects.Add( obj );


		Selection.objects			= objects.ToArray();
	}
}

}

