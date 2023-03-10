using System;
using BugsFarm;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete;

namespace BugsFarm.Game
{
	public enum DeathReason
	{
		None,

		NoFood,
		NoWater,
		Fight,
	}


	public static class GameEvents
	{
	#region Global

		public static Action	OnGameReset;
		public static Action	OnLoaded;
		public static Action	OnRawSimulationEnd;
		public static Action	OnSimulationEnd;
		public static Action	OnTutorialGreatAnimDone;
		public static Action	OnUnitSelected;
		public static Action<ISoundOccupant> OnSoundQueue;

	#endregion
	#region UI
		
		public static Action					PanelOpenAnimDone;
		public static Action					TabOpened;

		public delegate void WinPanelNextStage( WinPanelStage winPanelStage );
		public static WinPanelNextStage OnWinPanelNextStage;

	#endregion
	
	#region Objects

		public delegate void ObjectEvent( APlaceable placeable );
		public delegate void ObjectViewEvent( A_MB_Placeable placeable );

		public static ObjectViewEvent	OnObjectTap;
		public static ObjectEvent	OnObjectBought;
		public static ObjectEvent	OnObjectUpgrade;
		public static ObjectEvent	OnFoodRestock;
		public static ObjectEvent	OnObjectLevel1BuildComplete;
		public static ObjectEvent	OnObjectUpgradeBgn;
		public static ObjectEvent	OnFightStockBecameVisible;
		public static ObjectEvent	OnObjectDestroyed;
		public static ObjectEvent	OnPlacedObject;
	#endregion
	#region Ants

		public delegate void AntEvent( AntPresenter ant );

		public static Action		OnMaggotSpawned;
		public static Action		OnAntSpawned;
		public static AntEvent		OnAntTap;
		public static AntEvent		OnAntDestroyed;
		public static AntEvent		OnAntGotXP;
		public static AntEvent		OnAntTypeLevelUp;


		public delegate void AntTypeEvent( AntType type );

		public static AntTypeEvent	OnAntTypeBought;
		public static AntTypeEvent	OnAntTypeBorn;


		public delegate void AntDeathEvent( AntPresenter ant, DeathReason reason );

		public static AntDeathEvent	OnAntDied;

	#endregion
	#region Fight

		public delegate void BattleEnd( int level, bool isPlayerWin );
		public static BattleEnd		OnBattleEnd;

		public delegate void UnitEvent( Unit unit );
		public static UnitEvent		OnUnitDied;
		public static UnitEvent		OnUnitCaveWalkDone;

	#endregion
	}
}