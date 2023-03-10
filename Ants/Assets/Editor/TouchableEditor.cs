using UnityEditor;


// http://answers.unity.com/answers/851816/view.html


[CustomEditor(typeof( Touchable ))]
public class Touchable_Editor : Editor
{
	public override void OnInspectorGUI ()
	{
		// Do nothing
	}
}


/*
// http://answers.unity.com/answers/1165070/view.html

 
namespace UnityEngine.UI
{
	[CustomEditor(typeof( Touchable ))]
	public class TouchableEditor : Editor
	{
		public override void OnInspectorGUI()
		{}
	}
}
*/

