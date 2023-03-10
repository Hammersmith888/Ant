using System;
using System.Collections.Generic;
using Malee.List;
using UnityEngine;

[Serializable]
public struct BuildingStage
{
	[SerializeField] private float _quantity;
	[SerializeField] private Sprite _sprite;
	public float Quantity => _quantity;
	public Sprite Sprite => _sprite;
}

[CreateAssetMenu(
	fileName	=						ScrObjs.CfgBuilding,
	menuName	= ScrObjs.folder +		ScrObjs.CfgBuilding,
	order		=						ScrObjs.CfgBuilding_i
)]
public class CfgBuilding : CfgObject
{
	[Serializable]
	private class RBuildingStage : ReorderableArray<BuildingStage>{}

	[Reorderable] [SerializeField] private RBuildingStage _stages = new RBuildingStage();

	public BuildingStage GetStage(int id)
	{
		return _stages[id];
	}
	public IEnumerable<BuildingStage> GetStages()
	{
		return _stages;
	}
}

