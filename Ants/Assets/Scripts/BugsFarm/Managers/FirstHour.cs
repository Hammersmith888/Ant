using UnityEngine;


public static class FirstHour
{
	private static Data_Other _dataOther;
	public static void Init(Data_Other dataOther)
	{
		_dataOther = dataOther;
	}
	public static int GetSpeedUpPrice( APlaceable placeable )
	{
		if (
				
				placeable.Type == ObjType.Food				&&
				placeable.SubType == (int)FoodType.Garden	&&
				!Keeper.Stats.FirstGardenBuildComplete
			)
			return 0;

		float timeLeft_min		= placeable.TimeLeft / 60;
		float minuteCost		= _dataOther.Other.GameStart.MinuteCost;
		float cost				= timeLeft_min * minuteCost;
		int ceiled				= Mathf.CeilToInt( cost );

		return ceiled;
	}
	public static int GetPrice( CfgObject data, int subType )
	{
		if (
				
				data.type == ObjType.Food			&&
				subType == (int)FoodType.Garden		&&
				StatsManager.GetCount( data.type, subType ) == 0
			)
			return 0;
		
		return data.Price;
	}
	public static int GetPrice( CfgAnt data )
	{
		if (
				
				data.antType == AntType.Worker		&&
				StatsManager.GetCount( data.antType ) == 0
			)
			return 0;
		
		return data.price;
	}
}

