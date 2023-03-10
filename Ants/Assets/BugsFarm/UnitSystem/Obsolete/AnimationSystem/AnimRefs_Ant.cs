using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using Malee.List;
using Spine.Unity;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
	[CreateAssetMenu(
		                fileName	=						ScrObjs.AnimRefs_Ant,
		                menuName	= ScrObjs.folder +		ScrObjs.AnimRefs_Ant,
		                order		=						ScrObjs.AnimRefs_Ant_i
	                )]
	public class AnimRefs_Ant : ScriptableObject
	{
		[Reorderable][SerializeField] private ReferenceList _references;

		public IEnumerable<AnimationReference> GetAll()
		{
			return _references;
		}
	
		public AnimationReferenceAsset GetAnim( AnimKey animation )
		{
			var reference = _references.FirstOrDefault(x => x.AnimationType == animation);
			return reference.IsNullOrDefault() ? null : reference.ReferenceAsset;
		}
		public bool HasAnim( AnimKey animation )
		{
			return _references.Any(x=>x.AnimationType == animation);
		}
		public AnimationReference GetAnimReference( AnimKey animation )
		{
			var reference = _references.FirstOrDefault(x => x.AnimationType == animation);
			return reference.IsNullOrDefault() ? default : reference;
		}
		public float GetTimeScale( AnimKey animation )
		{
			var reference = _references.FirstOrDefault(x => x.AnimationType == animation);
			return reference.IsNullOrDefault() ? 1f : reference.TimeSacale;
		}
		public static AnimKey GetTakeAnim( FoodType foodType )
		{
			switch (foodType)
			{
				case FoodType.Seeds:
				case FoodType.Grapes: return AnimKey.TakeFoodHi;

				case FoodType.Wheat:
				case FoodType.SunflowerSeeds:
				case FoodType.Raspberry:
				case FoodType.FoodStock:
				case FoodType.FightStock:
				case FoodType.Garden: return AnimKey.TakeFoodLow;
				
				default: return AnimKey.TakeFoodMid;
			}
		}
		public static bool IsLoop(AnimKey anim)
		{
			switch (anim)
			{
				case AnimKey.Attack:
				case AnimKey.Attack2:
				case AnimKey.FightShovel1:
				case AnimKey.FightShovel2:
				case AnimKey.FightHit:
				case AnimKey.FightKick:

				case AnimKey.Death:
				case AnimKey.Awake:
				case AnimKey.MineLever:
				case AnimKey.DropMud:
				case AnimKey.GiveFood:
				case AnimKey.TakeFoodLow:
				case AnimKey.TakeFoodMid:
				case AnimKey.TakeFoodHi:
				case AnimKey.TakeGarbage:
				case AnimKey.GarbageDropPile:
				case AnimKey.GarbageDropRecycler:
				case AnimKey.JumpOff:
				case AnimKey.JumpLand:
					return false;

				default:
					return true;
			}
		}
	}

	[Serializable]
	public class ReferenceList: ReorderableArray<AnimationReference> {}

	[Serializable]
	public struct AnimationReference
	{
		[SerializeField] private AnimKey _type;
		[SerializeField] private AnimationReferenceAsset _referenceAsset;
		[SerializeField] private float _timeSacale;
		[SerializeField] private bool  _isLoop;
		
		public AnimKey AnimationType => _type;
		public AnimationReferenceAsset ReferenceAsset => _referenceAsset;
		public float TimeSacale => _timeSacale;
		public bool IsLoop => _isLoop;
	}
}