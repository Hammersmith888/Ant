

public static class BiDicts
{
	public static readonly BiDict< ( ObjType, int ), string > Objects		= new BiDict< ( ObjType, int ), string >
	{
		{ ( ObjType.Bowl,				0						),		"Поилка"						},
		{ ( ObjType.Food,				(int)FoodType.Garden	),		"Огород"						},
		{ ( ObjType.str_Goldmine,		0						),		"Шахта"							},
		{ ( ObjType.Food,				(int)FoodType.DumpsterStock	),		"Мусорный бак"					},
		{ ( ObjType.str_Pikes,			0						),		"Тренировка копейщиков"			},
		{ ( ObjType.str_ArrowTarget,	0						),		"Тренировка лучников"			},
		{ ( ObjType.str_SleepingPod,	0						),		"Спальное место"				},

		{ ( ObjType.Food,	(int)FoodType.SunflowerSeeds		),		"Семечки"						},
		{ ( ObjType.Food,	(int)FoodType.Wheat					),		"Колосок"						},
		{ ( ObjType.Food,	(int)FoodType.Seeds					),		"Семена льна"					},
		{ ( ObjType.Food,	(int)FoodType.Raspberry				),		"Малина"						},
		{ ( ObjType.Food,	(int)FoodType.Grapes				),		"Виноград"						},
		{ ( ObjType.Food,	(int)FoodType.Huzelnut				),		"Фундук"						},
		{ ( ObjType.Food,	(int)FoodType.Apple					),		"Яблоко"						},
		{ ( ObjType.Food,	(int)FoodType.FoodStock					),		"Склад"							},

		{ ( ObjType.Decoration,		(int)DecorType.Leaf			),		"Лист"							},
		{ ( ObjType.Decoration,		(int)DecorType.Curtain		),		"Занавеска"						},
		{ ( ObjType.Decoration,		(int)DecorType.Grass_2		),		"Папоротник"					},
		{ ( ObjType.Decoration,		(int)DecorType.Grass_1		),		"Хлорофитум"					},
		{ ( ObjType.Decoration,		(int)DecorType.Mushrooms	),		"Грибы"							},
		{ ( ObjType.Decoration,		(int)DecorType.Flower		),		"Цветок"						},
		{ ( ObjType.Decoration,		(int)DecorType.TNT			),		"Динамит"						},
		{ ( ObjType.Decoration,		(int)DecorType.Barrel		),		"Бочонок"						},
		{ ( ObjType.Decoration,		(int)DecorType.Light		),		"Светильник"					},
	};
}

