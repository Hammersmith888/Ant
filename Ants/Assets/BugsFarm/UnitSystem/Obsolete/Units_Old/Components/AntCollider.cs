using UnityEngine;
using UnityEngine.Serialization;

namespace BugsFarm.UnitSystem.Obsolete
{
	public class AntCollider : MonoBehaviour
	{
	#pragma warning disable 0649

		[FormerlySerializedAs("_mb_Ant")] [SerializeField] AntView		_antView;

	#pragma warning restore 0649


		void OnMouseUp()
		{
			//if ( Tools.IsPointerOverGameObject() || UI_Control.IsModalPanelOpened )
			//	return;

		
			//InfoPanel.Open( _mb_Ant.Ant );
		}
	}
}

