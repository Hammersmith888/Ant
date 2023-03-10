using System;
using System.Collections.Generic;


[Serializable]
public struct FB_CfgMaggots
{
	public float								stageMinutes;

	public Dictionary< AntType, float >			probabilities;


	public FB_CfgMaggots( CfgMaggots other )
	{
		stageMinutes		= other.stageMinutes;
		probabilities		= new Dictionary< AntType, float >( other.probabilities );
	}
}

