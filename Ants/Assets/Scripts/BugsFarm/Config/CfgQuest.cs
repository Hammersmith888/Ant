using UnityEngine;


[CreateAssetMenu(
	fileName	=						ScrObjs.CfgQuest,
	menuName	= ScrObjs.folder +		ScrObjs.CfgQuest,
	order		=						ScrObjs.CfgQuest_i
)]
public class CfgQuest : ScriptableObject
{
	public QuestID		id;
	public string		description;
	public Sprite		icon;
	public int			reward;
	public int			stages;
}

