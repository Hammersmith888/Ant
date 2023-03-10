using UnityEngine;


public static class Constants
{
	public const float		AutoSaveTime				= 5 * 60;		// each 5 min

	public const float		Ant_Speed					= 1;
	public const float		Snail_Speed					= .5f / 3;
	public const float		Spider_Speed				= .5f;
	public const float		DumpsterRecycleTime			= 60 * 60 * 1;


// Tutorial
	public const int		QueenPlace					= 10;
	public const int		QueenLightPlace				= 21;
	public const int		BowlPlace					= 1;
	public const int		DigGroundPlace				= 36;
	public const int		SafePlace				    = 15;
	public const int		RelaxationRoomPlace			= 16;
	public const int		TutorGoldminePlace			= 11;
	public const int		TutorSunflowerSeedsPlace	= 32;
	public const int		TutorFightStockPlace		= 13;


// Fight
	public const int		MaxUnitLevel				= 25;
	public const int		RestockFightStockPrice		= 200;
	public const int		RestockFightStockAmount		= 100;


// Resources
	public const float		Water_TAPable				= 10;
	public const float		FreshFoodTimeMins			= 1;


// Input
	public const float		HoldTimeToReplace			= 1;


// AI
	public const float		DrinkTime					= 5;
	public const float		EatTime						= 5;
	public static Vector2	PatrolStayTime				= new Vector2( 5, 10 );
	public const float		Ant_FallSpeed				= 5f;


	// AI, Training
	public const int		StartTrainingnAfter			= 8;
	public const int		TransformationAfter			= 8;


// AI, Clean
	public const float		GarbageQuantity				= 1;


// AI, Queen
	public const float		QueenFoodConsumptionTime	= 5;


// AI, Digging, VineBuilding
	public const int		Dig_MaxWorkers				= 10;
	public static Vector2	Dig_DigTime					= new Vector2( 6, 12 );
	public static Vector2	BuildVineTime				= new Vector2( 6, 12 );


// AI (tech)
	public const float		RandomRoad_MinDist			= .5f;
	public const float		RandomRoad_MinDist_Obj		= 1;

	public const float		SpeedMul_GapMin				= .5f;
	public const float		SpeedMul_GapMax				= 1;

	public const float		LadderCheckDist				= .75f;
	public const float		LadderMinDist				= 1;

	public const float		MinSpeedMul					= .1f;


// Technical
	public const float		JointMinDist				= .2f;

	public const float		Polyline_SegmentLength		= .1f;
	public const float		Polyline_Resolution			= 10;
}

