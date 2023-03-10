using System;
using System.Collections.Generic;
using BugsFarm.Game;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;


[Flags]
public enum SendTo
{
	None		= 0,

	Facebook	= 1,
	DevToDev	= 2,
	Firebase	= 4,
	Tenjin		= 8,

	All			= 15,
}
internal static class SendToExt
{
	public static bool Includes( this SendTo sendTo, SendTo flag )		=> (sendTo & flag) > 0;
}
public enum PurchaseGroup
{
	None,

	Building,
	Food,
	Decor,
	Insect,
}


namespace BugsFarm
{

public class Analytics : MonoBehaviour
{
	public static readonly Dictionary< (ObjType, int), (PurchaseGroup, string) > Names		= new Dictionary< (ObjType, int), (PurchaseGroup, string) >
	{
		{ ( ObjType.Bowl,				0						),		(PurchaseGroup.Building, "drinker"				)},
		{ ( ObjType.Food,				(int)FoodType.Garden	),		(PurchaseGroup.Building, "garden"				)},
		{ ( ObjType.str_Goldmine,		0						),		(PurchaseGroup.Building, "mine"					)},
		{ ( ObjType.Food,				(int)FoodType.DumpsterStock	),	(PurchaseGroup.Building, "trash_can"			)},
		{ ( ObjType.str_Pikes,			0						),		(PurchaseGroup.Building, "gym"					)},
		{ ( ObjType.str_ArrowTarget,	0						),		(PurchaseGroup.Building, "target"				)},
		{ ( ObjType.str_SleepingPod,	0						),		(PurchaseGroup.Building, "sleeping_bag"			)},

		{ ( ObjType.Food,	(int)FoodType.SunflowerSeeds		),		(PurchaseGroup.Food, "seeds"					)},
		{ ( ObjType.Food,	(int)FoodType.Wheat					),		(PurchaseGroup.Food, "spikelet"					)},
		{ ( ObjType.Food,	(int)FoodType.Seeds					),		(PurchaseGroup.Food, "flax"						)},
		{ ( ObjType.Food,	(int)FoodType.Raspberry				),		(PurchaseGroup.Food, "raspberries"				)},
		{ ( ObjType.Food,	(int)FoodType.Grapes				),		(PurchaseGroup.Food, "grapes"					)},
		{ ( ObjType.Food,	(int)FoodType.Huzelnut				),		(PurchaseGroup.Food, "nuts"						)},
		{ ( ObjType.Food,	(int)FoodType.Apple					),		(PurchaseGroup.Food, "apple"					)},
		{ ( ObjType.Food,	(int)FoodType.FoodStock				),	    (PurchaseGroup.Food, "fight_stock"				)},

		{ ( ObjType.Decoration,		(int)DecorType.Leaf			),		(PurchaseGroup.Decor, "leaf"					)},
		{ ( ObjType.Decoration,		(int)DecorType.Curtain		),		(PurchaseGroup.Decor, "curtain"					)},
		{ ( ObjType.Decoration,		(int)DecorType.Grass_2		),		(PurchaseGroup.Decor, "fern"					)},
		{ ( ObjType.Decoration,		(int)DecorType.Grass_1		),		(PurchaseGroup.Decor, "grass"					)},
		{ ( ObjType.Decoration,		(int)DecorType.Mushrooms	),		(PurchaseGroup.Decor, "mushrooms"				)},
		{ ( ObjType.Decoration,		(int)DecorType.Flower		),		(PurchaseGroup.Decor, "flower"					)},
		{ ( ObjType.Decoration,		(int)DecorType.TNT			),		(PurchaseGroup.Decor, "fireworks"				)},
		{ ( ObjType.Decoration,		(int)DecorType.Barrel		),		(PurchaseGroup.Decor, "barrel"					)},
		{ ( ObjType.Decoration,		(int)DecorType.Light		),		(PurchaseGroup.Decor, "lamp"					)},
	};


	private void Start()
	{
		DontDestroyOnLoad( gameObject );

		GameEvents.OnObjectBought		+= OnObjectBought;
		GameEvents.OnObjectUpgrade		+= OnObjectUpgrade;

		GameEvents.OnAntTypeBought		+= OnAntBought;
		GameEvents.OnAntTypeBorn		+= OnAntBorn;
		GameEvents.OnAntTypeLevelUp		+= OnAntTypeLevelUp;
		GameEvents.OnAntDied			+= (ant, reason) => OnAntDied( reason );
		GameEvents.OnUnitDied			+= unit => OnAntDied( DeathReason.Fight );
		GameEvents.OnBattleEnd			+= OnBattleEnd;
	}
	
	public static void SessionStart()
	{
		const string keyFirstLaunch		= "FirstLaunch";
		string balance = "A";//Loader.IsConfigA ? "A" : "B";
		string tenjinSuffix				= "_" + balance;
		var parameters					= new Dictionary<string, object>
		{
			{ "Balance", balance }
		};

		// first_open
		if (!PlayerPrefs.HasKey( keyFirstLaunch ))
		{
			PlayerPrefs.SetInt( keyFirstLaunch, 1 );
			CustomEvent( true, "first_open", parameters );
			CustomEvent( true, "first_open" + tenjinSuffix, sendTo: SendTo.Tenjin );
			CustomEvent( true, "CustomFirstOpen", parameters, SendTo.Firebase );
		}

		// session_start
		CustomEvent( true, "session_start", parameters );
		CustomEvent( true, "session_start" + tenjinSuffix, sendTo: SendTo.Tenjin );
		CustomEvent( true, "CustomSessionStart", parameters, SendTo.Firebase );
	}
	public static void UaPpAcepted()
	{
		CustomEvent(true, "privacy_accept", sendTo: SendTo.DevToDev);
	}
	public static void GameReset( int gameResetsCount )
	{
		CustomEvent(
			true,
			"restart_game", new Dictionary<string, object>
			{
				{ "Amount", gameResetsCount },
			}
		);
	}
	public static void TutorialStart()
	{
		Dev2Dev.TutorialStart();																			// DevToDev
		CustomEvent( true, "tutorial_start", sendTo: SendTo.Facebook );			// Facebook
	}
	public static void TutorialSkip()
	{
		Dev2Dev.TutorialSkip();																				// DevToDev
		CustomEvent( true, "tutorial_skip", sendTo: SendTo.Facebook );			// Facebook
	}
	public static void TutorialComplete()
	{
		Dev2Dev.TutorialComplete();																			// DevToDev
		Facebook.TutorialComplete();																		// Facebook
		// Firebase.TutorialComplete();																		// Firebase
		CustomEvent( true, "tutorial_complete", sendTo: SendTo.Tenjin );			// Tenjin
	}
	public static void TutorialStep( int tutorialStep )
	{
		Dev2Dev.TutorialStep(tutorialStep);
	}
	public static void NoResources( bool noWater )
	{
		CustomEvent(
			false,
			"no_resources", new Dictionary<string, object>
			{
				{ "Name", noWater ? "water" : "food" },
			}
		);
	}
	public static void RoomOpen( int roomNum )
	{
		CustomEvent(
			false,
			"room_open", new Dictionary<string, object>
			{
				{ "Name", roomNum },
			}
		);

		if (roomNum == 4)
			CustomEvent( true, "room4_open", sendTo: SendTo.Firebase | SendTo.Tenjin );
	}
	public static void InAppPurchase( AntType type, int price, Currency currency )
	{
		Dev2Dev.InAppPurchase(type, price, currency);
	}
	public static void InAppPurchase( APlaceable placeable, int price, Currency currency )
	{
		Dev2Dev.InAppPurchase(placeable, price, currency);
	}
	public static void TaskComplete( QuestID quest )
	{
		CustomEvent(
			false,
			"task_complete", new Dictionary<string, object>
			{
				{ "Name", quest.ToString() },
			}
		);
	}
	public static void AllTasksDone()
	{
		CustomEvent(false, "all_tasks_done");
	}
	public static void BuyCoins( IAPType type )
	{
		CustomEvent(
		false,
			"buy_coins", new Dictionary<string, object>
			{
				{ "Tarif", type - IAPType.Coins_1 + 1 },
			}
		);
	}
	public static void RealPayment( string productId, string transactionId, string receipt, decimal price, string inAppCurrencyISOCode )
	{
		float price_f		= (float)price;
		double price_d		= (double)price;

		Dev2Dev.RealPayment( productId, transactionId, price_f, inAppCurrencyISOCode );						// DevToDev
		
		Facebook.RealPayment( productId, price, inAppCurrencyISOCode );										// Facebook
		
		// Firebase.RealPayment( productId, transactionId, price_d, inAppCurrencyISOCode );					// Firebase

		// Tenjin.Instance.CompletedPurchase( productId, inAppCurrencyISOCode, 1, price_d );			// Tenjin
	}
	
	private static void OnBattleEnd( int level, bool isWin )
	{
		CustomEvent(
		            false,
		            "level_done", new Dictionary<string, object>
		            {
			            { "Amount", level },
			            { "Result", isWin ? "win" : "lose" },
		            }
		           );

		Dev2Dev.LevelComplete( level );

		if (level == 15 && isWin)
			CustomEvent( true, "lvl15_open", sendTo: SendTo.Firebase | SendTo.Tenjin );
	}
	private static void OnObjectBought( APlaceable placeable )
	{
		if (!Names.TryGetValue( ( placeable.Type, placeable.SubType ), out (PurchaseGroup @class, string name) tuple ))
			return;

		string eventName;
		switch (tuple.@class)
		{
			case PurchaseGroup.Building: eventName		= "built_building";		break;
			case PurchaseGroup.Food:     eventName		= "buy_food";			break;
			case PurchaseGroup.Decor:    eventName		= "buy_decor";			break;
			default:
				return;
		}

		CustomEvent(
		            false,
		            eventName, new Dictionary<string, object>
		            {
			            { "Name", tuple.name },
		            },
		            SendTo.Facebook
		           );
	}
	private static void OnObjectUpgrade( APlaceable placeable )
	{
		if (!Names.TryGetValue( ( placeable.Type, placeable.SubType ), out (PurchaseGroup @class, string name) tuple ))
			return;

		CustomEvent(
		            false,
		            "boost_building", new Dictionary<string, object>
		            {
			            { "Name", tuple.name },
			            { "lvl", placeable.Level + 1 },
		            }
		           );
	}
	private static void OnAntBought( AntType type )
	{
		CustomEvent(
		            false,
		            "buy_bug", new Dictionary<string, object>
		            {
			            { "Name", type.ToString() },
		            },
		            SendTo.Facebook
		           );
	}
	private static void OnAntBorn( AntType type )
	{
		CustomEvent(
		            false,
		            "new_bug", new Dictionary<string, object>
		            {
			            { "Name", type.ToString() },
		            }
		           );
	}
	private void OnAntTypeLevelUp( AntPresenter ant )
	{
		CustomEvent(
		            false,
		            "unit_level_up", new Dictionary<string, object>
		            {
			            { "Name", ant.AntType.ToString() },
			            { "lvl", ant.LevelP1 },
		            }
		           );
	}
	private void OnAntDied( DeathReason reason )
	{
		CustomEvent(
		            false,
		            "death", new Dictionary<string, object>
		            {
			            { "type", reason.ToString() },
		            }
		           );
	}
	private static void CustomEvent(bool allowedDuringTutorial,
									string eventName,
									Dictionary< string, object > parameters = null,
									SendTo sendTo = SendTo.DevToDev | SendTo.Facebook)
	{
		if ( (Tutorial.Instance?.IsActive ?? false) && !allowedDuringTutorial)
		{
			return;
		}

		if (sendTo.Includes( SendTo.DevToDev ))			Dev2Dev.CustomEvent( eventName, parameters );

		if (sendTo.Includes( SendTo.Facebook ))			Facebook.CustomEvent( eventName, parameters );

		//if (sendTo.Includes( SendTo.Firebase ))			Firebase.CustomEvent( eventName, parameters );

		//if (sendTo.Includes( SendTo.Tenjin ))			Tenjin.CustomEvent( eventName );
	}
}

}

