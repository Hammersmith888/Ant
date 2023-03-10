using System;
using UnityEngine;


public enum PoolType
{
	None,

	Bubbles,
	Coins,
	Arrows,
	DamageText,
	ArrowFight
}


[Serializable]
public class Pool
{
	public PoolType		type;
	public GameObject	prefab;
	public int			size;
}

