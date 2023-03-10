using System;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;

public static class Texts
{
	// new
	public const string		Age						= "Time.Age";
	public const string		Second				    = "Time.Sec";
	public const string		Hour				    = "Time.Hour";
	public const string		Minute					= "Time.Min";
	public const string		Day					    = "Time.Day";

	public const string		Level					= "stat_level";
	public const string		Rest					= nameof(RestTask);
	
	

	public const string		Loading					= "Загрузка {0}%";
	public const string		UnlocksAfter			= "Доступно\nс {0} ур.";
	public const string		GameResetHeader			= "Сброс игры";
	public const string		GameResetText			= "Вы уверены, что хотите начать игру заново?";
	public const string		FightLvl				= "ур.";
	public const string		lvl						= "ур.";
	public const string		RemoveObjects			= "Удалить {0} чтобы поставить {1}?";
	public const string		WantsEat				= "Хочет есть";
	public const string		WantsDrink				= "Хочет пить";
	public const string		WillWantEat				= "Проголодается через";
	public const string		WillWantDrink			= "Захочет пить через";
	public const string		WillWantSleep			= "Заснет через";
	public const string		WantsSleep				= "Хочет спать";
	public const string		Free					= "Бесплатно";
	public const string		Room					= "Комната";
	public const string		Objects					= "Объекты";
	public const string		QuestComplete			= "ВЫПОЛНЕНО";

	public const string		GardenInProcess			= "Огород дает пищу, но ему нужно время чтобы вырасти.";

	public const string		OpenRoom				= "Открыть комнату?";
	public const string		OpenRoomFree			= "Открыть комнату\n\nбесплатно?";
	public const string		OpenRoom1				= "THIS AREA IS CLOSED.";
	public const string		OpenRoom2				= "THIS AREA IS CLOSED.\nPLEASE, WAIT {0} DAYS OR OPEN NOW.";

	public const string		Open					= "ОТКРЫТЬ";
	public const string		Get						= "ЗАБРАТЬ";

	public const string		WakeUp					= "Разбудить";
	public const string		WakeUpAll				= "Разбудить всех";
	public const string		Kick					= "Пнуть";
	public const string		Reassign				= "Перенаправить";


	// public static string UpgradeButton( UpgradeButtonType type )
	// {
	// 	switch (type)
	// 	{
	// 		case UpgradeButtonType.RestockFood:
	// 		case UpgradeButtonType.RestockFightStock:
	// 												return "Пополнить";	
	//
	// 		case UpgradeButtonType.Upgrade:			return "Улучшить";	
	// 		case UpgradeButtonType.Speedup:			return "Ускорить";	
	//
	// 		case UpgradeButtonType.None:
	// 		default:
	// 			return "";
	// 	}
	// }


	public static string AntState2Str( StateAnt state, string name )
	{
		switch (state)
		{
			case StateAnt.Feed:			return "Кормит царицу";
			case StateAnt.Restock:		return "Складывает запасы";
			case StateAnt.RestockFight:	return "Складывает запасы в бой";
			case StateAnt.Goldmine:		return "Добывает золото";
			case StateAnt.Clean:		return "Убирает мусор";
			case StateAnt.Dig:			return "Копает";
			case StateAnt.Garden:		return "Ухаживает за огородом";
			case StateAnt.Build:		return "Строит";

			case StateAnt.TrainArcher:
			case StateAnt.TrainPikeman:
			case StateAnt.TrainWorker:
				return "Тренируется";
			case StateAnt.Patrol:		return "Патрулирует";

			case StateAnt.Walk:			return "Гуляет";

			case StateAnt.Drink:		return "Идет пить";
			case StateAnt.Eat:			return "Идет есть";
			case StateAnt.Sleep:		return "Спит";

			case StateAnt.Dead:			return $"Здесь покоится { name }";

			case StateAnt.Rest:
			case StateAnt.Fall:
			case StateAnt.None:
			default:					return "Отдыхает";
		}
	}


	public static string QueenState2Str( QueenState state )
	{
		switch (state)
		{
			case QueenState.Eat:			return "Кушает";
			case QueenState.GiveBirth:		return "Откладывает яйца";
			case QueenState.Sleep:			return "Спит";

			case QueenState.Rest:
			case QueenState.None:
			default:						return "Отдыхает";
		}
	}


	public static string AgeStr( double seconds, bool noMinSec = false )
	{
		TimeSpan ts			= TimeSpan.FromSeconds( seconds );

		TimeSpan ts_H		= ts.RoundToNearest( TimeSpan.FromHours		( 1 ) );
		TimeSpan ts_M		= ts.RoundToNearest( TimeSpan.FromMinutes	( 1 ) );
		TimeSpan ts_S		= ts.RoundToNearest( TimeSpan.FromSeconds	( 1 ) );

		ts_S				= noMinSec && ts_M.Minutes > 0 ? ts_M : ts_S;

		string str;

		if (ts.Days > 0)			str		= AgeStr( ts_H.Days,		ts_H.Hours,		"д.",		"ч."						);
		else if (ts.Hours > 0)		str		= AgeStr( ts_M.Hours,		ts_M.Minutes,		"ч.",		"мин."						);
		else						str		= AgeStr( ts_S.Minutes,	ts_S.Seconds,		"мин.",		"сек.",		noMinSec		);
		
		return str;
	}


	static string AgeStr( int t1, int t2, string str1, string str2, bool onlyOne = false )
	{
		return
			t1 == 0 ?
			$"{t2} {str2}" :
			$"{t1} {str1}" + (!onlyOne && t2 > 0 ? $" {t2} {str2}" : "")
		;
	}
}

