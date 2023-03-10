using UnityEngine;

public static class Vector2Extensions
{

	public static Vector2 AbsoluteValue(this Vector2 value)
	{
		return new Vector2
		{
			x = Mathf.Abs(value.x),
			y = Mathf.Abs(value.y)
		};
	}

	public static Vector3 ToVector3(this Vector2 vector)
	{
		return new Vector3(vector.x, vector.y, 0);
	}
}