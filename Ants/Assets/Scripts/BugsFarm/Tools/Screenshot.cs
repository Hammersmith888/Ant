using UnityEngine;


namespace BugsFarm
{

public class Screenshot : MonoBehaviour
{
#pragma warning disable 0649

	[SerializeField] string		_folder;

#pragma warning restore 0649

	int _suffix;


    void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		if (Input.GetKeyDown( KeyCode.Space ))
		{
			bool success			= false;

			while (!success)
			{
				string fileName		= $"Screenshot_{_suffix}.png";
				string path			= System.IO.Path.Combine( _folder, fileName );
				success				= !System.IO.File.Exists( path );

				if (success)
					ScreenCapture.CaptureScreenshot( path );

				_suffix ++;
			}
		}
#endif
    }
}

}

