using System;
using RotaryHeart.Lib.SerializableDictionary;


public enum IAPType
{
	None,

	Coins_1,
	Coins_2,
	Coins_3,

	Crystals_1,
	Crystals_2,
	Crystals_3,
}


public class Data_IAPs : MB_Singleton< Data_IAPs >
{
	[Serializable]
	public class TIAPs : SerializableDictionaryBase< IAPType, CfgIAP > {}
	
	public TIAPs	IAPs;
}

