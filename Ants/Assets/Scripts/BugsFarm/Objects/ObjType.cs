using System.Linq;

public enum ObjType
{
	None					= 0,

	// str_Garden			= 1,
	// str_Dumpster			= 3,

	str_Goldmine			= 2,
	str_Pikes				= 4,
	str_ArrowTarget			= 5,
	str_SleepingPod			= 6,
	str_OutDoorPear			= 13,
	str_Swords   			= 14,
	str_HangingPears		= 15,
	str_RelaxationRoom		= 16,
	str_Prison				= 17,	
	str_OrderBoard			= 18,
	str_Hospital			= 19,
	str_Safe			    = 20,


	Food					= 8,
	Decoration				= 9,

	Bowl					= 7,
	DigGroundStock			= 10,
	Queen					= 11,
	HerbsStock	     		= 12,
}

public static class ObjectClassificationUtils
{
	public static bool AnyOff<T>(this T obj, params T[] mathes)
	{
		return mathes.Contains(obj);
	}
	public static bool AllOff<T>(this T obj, params T[] mathes)
	{
		return mathes.All(x => Equals(obj,x));
	}
}

