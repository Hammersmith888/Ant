using System;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


[CreateAssetMenu(
	fileName	=						ScrObjs.Tutorial,
	menuName	= ScrObjs.folder +		ScrObjs.Tutorial,
	order		=						ScrObjs.Tutorial_i
)]
public class TutorialTexts : ScriptableObject
{
	[Serializable]
	public class TTexts : SerializableDictionaryBase< TutorialStageText, string > {}

	public TTexts	texts;
}

