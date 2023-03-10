using System;
using System.Collections.Generic;


[Serializable]
public class Stats
{
	public int			LastReviewProposalHours;

	public DateTime		gameStartUTC;
	public float		dayTimeAddon;

	public float		foodEaten				{ get; private set; }
	public float		garbageRecycled			{ get; private set; }

	public bool			FirstGardenBuildComplete;

	public Dictionary< AntType, int >			boughtAnts				= new Dictionary< AntType, int >();
	public Dictionary< ( ObjType, int ), int >	boughtObjects			= new Dictionary< ( ObjType, int ), int >();


	public void Reset()
	{
		dayTimeAddon			= 0;

		foodEaten				= 0;
		garbageRecycled			= 0;

		FirstGardenBuildComplete		= false;

		boughtAnts		.Clear();
		boughtObjects	.Clear();
	}


	public void cb_FoodEaten( float amount )
	{
		foodEaten				+= amount;

		Quests.cb_FoodEaten( foodEaten );
	}


	public void cb_GarbageRecycled( float amount )
	{
		garbageRecycled			+= amount;

		Quests.cb_GarbageRecycled( garbageRecycled );
	}
}

