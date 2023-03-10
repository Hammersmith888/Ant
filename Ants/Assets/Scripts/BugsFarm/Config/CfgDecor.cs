using UnityEngine;


public enum DecorType
{
	None,

	Flower,
	Grass_1,
	Grass_2,
	Mushrooms,
	TNT,
	Barrel,
	Light,
	Curtain,
	Leaf,
	BenchPress,
	Bombs,
	Bows,
	CannonBalls,
	HangingPear,
	Light1,
	Light2,
	SmallCatapult,
	Stones,
	Weight
}


[CreateAssetMenu(
	fileName	=						ScrObjs.CfgDecor,
	menuName	= ScrObjs.folder +		ScrObjs.CfgDecor,
	order		=						ScrObjs.CfgDecor_i
)]
public class CfgDecor : CfgObject
{
	public DecorType	decorType;
}

