using System;
using System.Collections.Generic;


[Serializable]
public struct EggsRatio
{
	public int		eggs;
	public float	ratio;
}


[Serializable]
public struct FB_CfgQueen
{
	public int						Price;
	public FB_CfgAntConsumption		Consumption;
	public float					PregnancyHours;
	public float					SleepHours;

	public List< EggsRatio >		Eggs;


	public FB_CfgQueen( FB_CfgQueen other )
	{
		this			= default;
		Eggs			= new List< EggsRatio >();

		Set( other );
	}


	public void Set( FB_CfgQueen other )
	{
		var saved		= Eggs;
		this			= other;
		Eggs			= saved;

		Eggs.Clear();
		foreach (EggsRatio er in other.Eggs)
			Eggs.Add( er );
	}
}

