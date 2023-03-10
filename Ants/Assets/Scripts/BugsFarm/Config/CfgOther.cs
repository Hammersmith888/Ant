using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

[CreateAssetMenu(
	                fileName	=						ScrObjs.CfgOther,
	                menuName	= ScrObjs.folder +		ScrObjs.CfgOther,
	                order		=						ScrObjs.CfgOther_i
                )]
public class CfgOther : ScriptableObject
{
	[Serializable]
	public class TQuests : SerializableDictionaryBase< QuestID, CfgQuest > {};


	public FB_CfgGameStart		GameStart;
	public CfgMaggots			Maggots;
	public FB_CfgQueen			Queen;

	public List< FB_CfgRoom >	Rooms;
	public List<CfgChest>		Chests;

	public TQuests				Quests;
	public List< QuestID >		QuestsOrder;

	public FB_CfgRoom GetRoomData(int roomNumber)
    {
		return Rooms.Find(x => x.RoomNumber == roomNumber);
	}
	public CfgChest GetChestData(int ChestID)
	{
		return Chests.Find(x => x.ID == ChestID);
	}

	//private void OnValidate()
	//{
	//	if(Chests.IsNull() || Chests.Count == 0)
	//	{
	//		Chests = new List<CfgChest>();
	//		int coins = 100;
	//		foreach (var cfgRoom in Rooms)
	//		{
	//			Chests.Add(new CfgChest(){ID = cfgRoom.RoomNumber, Coins = coins *cfgRoom.RoomNumber, Crystals = Tools.RandomBool() ? 1 : 0});
	//		}
			
	//	}
	//}
}

