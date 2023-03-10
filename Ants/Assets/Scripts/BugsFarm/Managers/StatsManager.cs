using System.Collections.Generic;
using BugsFarm.Game;

public static class StatsManager
{
	static Stats Stats		=> Keeper.Stats;


	public static void Init()
	{
		GameEvents.OnAntTypeBought					+= OnAntTypeBought;
		GameEvents.OnObjectBought					+= OnObjectBought;
		GameEvents.OnObjectLevel1BuildComplete		+= OnObjectLevel1BuildComplete;
	}

	static void OnObjectLevel1BuildComplete( APlaceable placeable )
	{
		if (
				placeable.Type == ObjType.Food &&
				placeable.SubType == (int)FoodType.Garden
			)
			Stats.FirstGardenBuildComplete		= true;
	}

	static void OnObjectBought( APlaceable placeable )			=> Stats.boughtObjects		.Increment( ( placeable.Type, placeable.SubType ) );

	static void OnAntTypeBought( AntType type )					=> Stats.boughtAnts			.Increment( type );

	public static int GetCount( ObjType type, int subType )		=> Stats.boughtObjects		.GetValueOrDefault( ( type, subType ) );

	public static int GetCount( AntType type )					=> Stats.boughtAnts			.GetValueOrDefault( type );


#region Extension Methods

	static void Increment< T >( this Dictionary< T, int > dict, T key )
	{
		dict[ key ]		= dict.TryGetValue( key, out int count ) ? count + 1 : 1;
	}


	static TV GetValueOrDefault< TK, TV >( this Dictionary< TK, TV > dict, TK key )
	{
		dict.TryGetValue( key, out TV value );

		return value;
	}

#endregion
}

