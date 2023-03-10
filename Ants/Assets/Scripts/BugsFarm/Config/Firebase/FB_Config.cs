using System.Collections.Generic;


public class FB_Config
{
	public Dictionary< string, FB_CfgAnt >			Ants;

	public FB_CfgGameStart							GameStart;
	public Dictionary< string, FB_CfgIAP >			IAP;
	public FB_CfgMaggots							Maggots;
	public FB_CfgQueen								Queen;
	public Dictionary< string, int >				Quests;
	public List< FB_CfgRoom >						Rooms;

	public Dictionary< string, FB_CfgObject >		buildings;
	public Dictionary< string, FB_CfgObject >		decorations;
	public Dictionary< string, FB_CfgObject >		food;
}

