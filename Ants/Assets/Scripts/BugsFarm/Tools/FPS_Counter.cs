using UnityEngine;


public class FPS_Counter : MB_Singleton< FPS_Counter >
{
	public static float FPS		=> Instance ? 1 / Instance._deltaTime : 0;


	float	_deltaTime;


	void Update()
	{
		float weight		= .1f;

		_deltaTime			+= (Time.unscaledDeltaTime - _deltaTime) * weight;

		if (Time.frameCount == 2)
			_deltaTime		= 1 / Time.unscaledDeltaTime;
	}
}

