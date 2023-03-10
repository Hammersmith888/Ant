using System;
using System.Collections.Generic;
using BugsFarm.Config;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using UnityEditor;
using Object = UnityEngine.Object;


public static class FB_Packer
{
	public static readonly BiDict< Currency, string > CCY		= new BiDict< Currency, string >
	{
		{ Currency.Coins,			"монеты"				},
		{ Currency.Crystals,		"кристаллы"				},
	};


	public static readonly BiDict< OtherAntParams, string > OthAntParams		= new BiDict< OtherAntParams, string >
	{
		{ OtherAntParams.AmmountCarry,			"Складывает в запас"				},
		{ OtherAntParams.WorkAmount,				"Копает за цикл"					},
	};


	public static readonly BiDict< QuestID, string > Quests		= new BiDict< QuestID, string >
	{
		{ QuestID.Buy2Workers,			"Купить двух рабочих"					},
		{ QuestID.Buy2Pikemen,			"Купить двух копейщиков"				},
		{ QuestID.Buy2Archers,			"Купить двух лучниц"					},
		{ QuestID.BuySnail,				"Купить улитку"							},
		{ QuestID.BuySpider,			"Купить паука"							},

		{ QuestID.BuildArrowTarget,		"Построить тренировку лучницы"			},
		{ QuestID.BuildGoldmine,		"Построить золотую шахту"				},
		{ QuestID.PlantFlower,			"Посадить цветок"						},

		{ QuestID.Recycle500,			"Переработать 500 ед мусора"			},
		{ QuestID.Feed5000,				"Скормить насекомым 5000 ед еды"		},
	};


	public static readonly BiDict< StateAnt, string > Tasks		= new BiDict< StateAnt, string >
	{
		{ StateAnt.Rest,			"Отдых"						},
		{ StateAnt.Patrol,			"Патруль"					},
		{ StateAnt.TrainArcher,			"Тренировка"				},
		{ StateAnt.Dig,				"Копает"					},
		{ StateAnt.Goldmine,		"Добывает золото"			},
		{ StateAnt.Walk,			"Гуляет"					},
		{ StateAnt.Sleep,			"Сон"						},
		{ StateAnt.Restock,			"Складывает запасы"			},
		{ StateAnt.RestockFight,	"Складывает запасы в бой"	},
		{ StateAnt.Clean,			"Убирает мусор"				},
		{ StateAnt.Garden,			"Ухаживает за огородом"		},
		{ StateAnt.Build,			"Строит"					},
	};


	static readonly BiDict< IAPType, string > IAPs		= new BiDict< IAPType, string >
	{
		{ IAPType.Coins_1,			"Coins 1"			},
		{ IAPType.Coins_2,			"Coins 2"			},
		{ IAPType.Coins_3,			"Coins 3"			},
		{ IAPType.Crystals_1,		"Crystals 1"		},
		{ IAPType.Crystals_2,		"Crystals 2"		},
		{ IAPType.Crystals_3,		"Crystals 3"		},
	};


	static readonly BiDict< AntType, string > Ants		= new BiDict< AntType, string >
	{
		{ AntType.Worker,			"1 - Рабочий"		},
		{ AntType.Pikeman,			"2 - Копейщик"		},
		{ AntType.Archer,			"3 - Лучник"		},
		{ AntType.Spider,			"4 - Паук"			},
		{ AntType.Snail,			"5 - Улитка"		},
	};



	public static readonly BiDict< AntType, string > UnitsOLD		= new BiDict< AntType, string >
	{
		{ AntType.Worker,			"Муравей рабочий"		},
		{ AntType.Pikeman,			"Муравей копейщик"		},
		{ AntType.Archer,			"Муравей лучник"		},
		{ AntType.Spider,			"Паук"					},
		{ AntType.Snail,			"Улитка"				},

		{ AntType.Worm,				"Червяк"				},
		{ AntType.PotatoBug,		"Колорадский жук"		},
		{ AntType.Cockroach,		"Таракан"				},
	};


	public static readonly BiDict< AntType, string > Units		= new BiDict< AntType, string >
	{
		{ AntType.Worker,			"Рабочий"				},
		{ AntType.Pikeman,			"Копейщик"				},
		{ AntType.Archer,			"Лучница"				},
		{ AntType.Spider,			"Паук"					},
		{ AntType.Snail,			"Улитка"				},

		{ AntType.Worm,				"Червяк"				},
		{ AntType.PotatoBug,		"Колорадский жук"		},
		{ AntType.Cockroach,		"Таракан"				},
	};


	static readonly BiDict< ( ObjType, int ), string > Keys		= new BiDict< ( ObjType, int ), string >
	{
		{ ( ObjType.Bowl,				0						),		"0 - Поилка"						},
		{ ( ObjType.Food,				(int)FoodType.Garden	),		"1 - Огород"						},
		{ ( ObjType.str_Goldmine,		0						),		"2 - Шахта"							},
		{ ( ObjType.Food,				(int)FoodType.DumpsterStock	),		"3 - Мусорный бак"					},
		{ ( ObjType.str_Pikes,			0						),		"4 - Тренировка копейщиков"			},
		{ ( ObjType.str_ArrowTarget,	0						),		"5 - Тренировка лучников"			},
		{ ( ObjType.str_SleepingPod,	0						),		"6 - Спальное место"				},

		{ ( ObjType.Food,	(int)FoodType.SunflowerSeeds		),		"1 - Семечки"						},
		{ ( ObjType.Food,	(int)FoodType.Wheat					),		"2 - Пшеница"						},
		{ ( ObjType.Food,	(int)FoodType.Seeds					),		"3 - Семена льна"					},
		{ ( ObjType.Food,	(int)FoodType.Raspberry				),		"4 - Малина"						},
		{ ( ObjType.Food,	(int)FoodType.Grapes				),		"5 - Виноград"						},
		{ ( ObjType.Food,	(int)FoodType.Huzelnut				),		"6 - Фундук"						},
		{ ( ObjType.Food,	(int)FoodType.Apple					),		"7 - Яблоко"						},
		{ ( ObjType.Food,	(int)FoodType.FoodStock					),		"Склад"								},

		{ ( ObjType.Decoration,		(int)DecorType.Leaf			),		"1 - Лист"							},
		{ ( ObjType.Decoration,		(int)DecorType.Curtain		),		"2 - Занавеска"						},
		{ ( ObjType.Decoration,		(int)DecorType.Grass_2		),		"3 - Папоротник"					},
		{ ( ObjType.Decoration,		(int)DecorType.Grass_1		),		"4 - Хлорофитум"					},
		{ ( ObjType.Decoration,		(int)DecorType.Mushrooms	),		"5 - Грибы"							},
		{ ( ObjType.Decoration,		(int)DecorType.Flower		),		"6 - Ромашка"						},
		{ ( ObjType.Decoration,		(int)DecorType.TNT			),		"7 - Динамит"						},
		{ ( ObjType.Decoration,		(int)DecorType.Barrel		),		"8 - Бочонок"						},
		{ ( ObjType.Decoration,		(int)DecorType.Light		),		"9 - Светильник"					},
	};


	public static readonly Dictionary< ( ObjType, int ), BiDict< int, string > > ObjParams			= new Dictionary< ( ObjType, int ), BiDict< int, string > >
	{
		{ ( ObjType.Bowl,				0									),	new BiDict< int, string >
		{
			{ 0,	"Объем"						},
			{ 1,	"Производит, ед"			},
			{ 2,	"Производит, мин"			},
		}},

		{ ( ObjType.Food,				(int)FoodType.Garden				),	new BiDict< int, string >
		{
			{ 0,	"Урожай"					},
			{ 1,	"Растет, мин"				},
		}},

		{ ( ObjType.str_Goldmine,		0									),	new BiDict< int, string >
		{
			{ 0,	"Вместимость"				},
			{ 1,	"Эффективность"				},
		}},

		{ ( ObjType.Food,				(int)FoodType.DumpsterStock				),	new BiDict< int, string >
		{
			{ 0,	"Объем"						},
			{ 1,	"Переработка, ед"			},
		}},

		{ ( ObjType.str_Pikes,			0									),	new BiDict< int, string >
		{
			{ 0,	"Очки опыта"				},
		}},

		{ ( ObjType.str_ArrowTarget,	0									),	new BiDict< int, string >
		{
			{ 0,	"Очки опыта"				},
		}},

		{ ( ObjType.str_SleepingPod,	0									),	new BiDict< int, string >
		{
			{ 0,	"Спальных мест"				},
		}},
	};


	public static readonly string	UpgradePrice		= "_ Цена";
	public static readonly string	UpgradeTime			= "_ Строится, мин";


	public static void SetDirty( Object obj )
	{
		#if UNITY_EDITOR
		EditorUtility.SetDirty( obj );
		#endif
	}


	public static void Unpack( string json )
	{
		FB_Config config			= (FB_Config)StringSerializationAPI.Deserialize( typeof( FB_Config ), json );


		// Other
		{
			CfgOther cfg = null;// Data_Other.Instance.Other;

			cfg.GameStart			= config.GameStart;				// GameStart

			cfg.Maggots.Set( config.Maggots );						// Maggots
			
			cfg.Queen.Set( config.Queen );							// Queen

			// Quests
			foreach (var pair in cfg.Quests)
			{
				pair.Value.reward		= config.Quests[ Quests[ pair.Key ] ];

				SetDirty( pair.Value );
			}

			// Rooms
			cfg.Rooms.Clear();
			cfg.Rooms.AddRange( config.Rooms );

			SetDirty( cfg );
		}


		// IAP
		foreach (var pair in config.IAP)
		{
			FB_CfgIAP fb		= pair.Value;
			CfgIAP cfg			= Data_IAPs.Instance.IAPs[ IAPs[ pair.Key ] ];

			cfg.IAP				= fb;

			SetDirty( cfg );
		}


		// Ants
		foreach (var pair in config.Ants)
		{
			FB_CfgAnt fb		= pair.Value;
			CfgAnt cfg			= Data_Ants.Instance.GetData( Ants[ pair.Key ] );

			cfg.Set( fb );

			SetDirty( cfg );
		}


		// Decors
		foreach (var pair in config.decorations)
			UnpackObj( pair.Value, ObjType.Decoration, Keys[ pair.Key ].Item2 );


		// Buildings
		foreach (var pair in config.buildings)
			if (Keys[ pair.Key ].Item1 != ObjType.Food)
				UnpackObj( pair.Value, Keys[ pair.Key ].Item1, Keys[ pair.Key ].Item2 );


		// Food
		{
			foreach (var pair in config.food)
			{
				FB_CfgObject fb			= pair.Value;
				FoodType foodType		= (FoodType)Keys[ pair.Key ].Item2;

				UnpackFood( fb, foodType );
			}

			UnpackFood( config.buildings[ Keys[ ( ObjType.Food, (int)FoodType.Garden	)]], FoodType.Garden	);
			UnpackFood( config.buildings[ Keys[ ( ObjType.Food, (int)FoodType.DumpsterStock	)]], FoodType.DumpsterStock	);
		}
	}


	static void UnpackObj( FB_CfgObject fb, ObjType type, int subType )
	{
		CfgObject obj		= Data_Objects.Instance.GetData( type, subType );

		obj.maxCount		= fb.maxCount;
		obj.SetPrice( fb.price );

		if (fb.upgrades != null)
		{
			obj.upgrades.levels.Clear();

			foreach (var ul in fb.upgrades)
				obj.upgrades.levels.Add( new UpgradeLevel( type, subType, ul ) );

			SetDirty( obj.upgrades );
		}

		SetDirty( obj );
	}


	static void UnpackFood( FB_CfgObject fb, FoodType foodType )
	{
		UnpackObj( fb, ObjType.Food, (int)foodType );

		CfgFood food		= Data_Objects.Instance.GetData( foodType );

		// Food stages
		for (int i = 0; i < food.stages.Length && i < fb.stages.Count; i ++)
			food.stages[ i ].quantity		= fb.stages[ i ];

		SetDirty( food );
	}


	public static string Pack()
	{
		FB_Config config			= new FB_Config();


		// Other
		{
			CfgOther cfg			= null; //Data_Other.Instance.Other;

			config.GameStart		= cfg.GameStart;								// GameStart
			config.Maggots			= new FB_CfgMaggots( cfg.Maggots );				// Maggots
			config.Queen			= new FB_CfgQueen( cfg.Queen );					// Queen
			config.Rooms			= new List< FB_CfgRoom >( cfg.Rooms );			// Rooms

			// Quests
			config.Quests			= new Dictionary< string, int >();
			foreach (var pair in cfg.Quests)
				config.Quests.Add( Quests[ pair.Key ], pair.Value.reward );
		}


		// IAP
		{
			config.IAP				= new Dictionary< string, FB_CfgIAP >();

			foreach (var pair in Data_IAPs.Instance.IAPs)
				config.IAP.Add( IAPs[ pair.Key ], pair.Value.IAP );
		}


		// Ants
		{
			config.Ants = new Dictionary< string, FB_CfgAnt >();
			
			var datas = Data_Ants.Instance.GetDatas(AntType.Maggot, AntType.Queen);
			foreach (var configAnt in datas)
			{
				config.Ants.Add(Ants[ configAnt.antType ], new FB_CfgAnt(configAnt.antType));
			}
		}


		// Buildings
		{
			config.buildings		= new Dictionary< string, FB_CfgObject >();

			foreach (CfgBuilding building in Data_Objects.Instance.Buildings)
				if (
						building.type != ObjType.DigGroundStock &&
						building.type != ObjType.Queen
					)
					AddObject( config.buildings, building, 0 );

			AddObject( config.buildings, FoodType.Garden );
			AddObject( config.buildings, FoodType.DumpsterStock );
		}


		// Food
		{
			config.food				= new Dictionary< string, FB_CfgObject >();

			foreach (CfgFood food in Data_Objects.Instance.Food)
				if (
						food.foodType != FoodType.Garden		&&			// Already added in buildings
						food.foodType != FoodType.DumpsterStock		&&			// Already added in buildings
						food.foodType != FoodType.FightStock	&&
						food.foodType != FoodType.PileStock
					)
					AddObject( config.food, food, (int)food.foodType );
		}


		// Decors
		{
			config.decorations		= new Dictionary< string, FB_CfgObject >();

			foreach (CfgDecor decor in Data_Objects.Instance.Decors)
				AddObject( config.decorations, decor, (int)decor.decorType );
		}


		string json					= StringSerializationAPI.Serialize( typeof( FB_Config ), config );
		
		return json;
	}


	static void AddObject(  Dictionary< string, FB_CfgObject > objects, FoodType foodType )
	{
		AddObject( objects, Data_Objects.Instance.GetData( foodType ), (int)foodType );
	}


	static void AddObject( Dictionary< string, FB_CfgObject > objects, CfgObject obj, int subType )
	{
		FB_CfgObject fb			= new FB_CfgObject();

		fb.price				= obj.Price;
		fb.maxCount				= obj.maxCount;


		if (obj.upgrades)
			fb.SetUpgrades( obj.type, subType, obj.upgrades );


		if (obj is CfgFood)
		{
			CfgFood cfgFood		= obj as CfgFood;

			fb.stages			= new List< float >();

			foreach (FoodStage stage in cfgFood.stages)
				fb.stages.Add( stage.quantity );
		}


		string key				= ObjType2Str( obj.type, subType );

		objects.Add( key, fb );
	}


	static string ObjType2Str( ObjType type, int subType )
	{
		var key		= ( type, subType );

		if (!Keys.ForwardContainsKey( key ))
			throw new Exception( $"Key not found: ({ type }, { subType })" );

		return Keys[ key ];

		/*
		case ObjType.DigGround:		return "Куча земли";
		case ObjType.Queen:			return "Царица";

		default:					return "Unknown Decor";
		default:					return "Unknown Food";
		default:					return "None";
		*/
	}
}

