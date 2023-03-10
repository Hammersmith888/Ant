using System;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


[CreateAssetMenu(
	fileName	=						ScrObjs.GoogleSheetBuffer,
	menuName	= ScrObjs.folder +		ScrObjs.GoogleSheetBuffer,
	order		=						ScrObjs.GoogleSheetBuffer_i
)]
public class GoogleSheetBuffer : ScriptableObject
{
	[Serializable]
	public class TCells : SerializableDictionaryBase< string, string > {}

	public TCells Cells;
}

