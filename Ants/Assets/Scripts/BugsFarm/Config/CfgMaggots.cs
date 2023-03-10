using System;
using RotaryHeart.Lib.SerializableDictionary;


[Serializable]
public struct CfgMaggots
{
	[Serializable]
	public class TProbabilities : SerializableDictionaryBase< AntType, float > {}


	public float				stageMinutes;

	public TProbabilities		probabilities;


	public void Set( FB_CfgMaggots fb )
	{
		stageMinutes		= fb.stageMinutes;

		probabilities.Clear();
		foreach (var pair in fb.probabilities)
			probabilities.Add( pair.Key, pair.Value );
	}
}

