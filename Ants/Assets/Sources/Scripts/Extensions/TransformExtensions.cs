using System;
using System.Collections.Generic;
using UnityEngine;


public static class TransformExtensions
{

	public static RectTransform GetRectChild(this Transform transform, int childIndex)
	{
		try
		{
			return (RectTransform)transform.GetChild(childIndex);
		}
		catch (Exception e)
		{
			Debug.Log(e);
			throw;
		}

	}

	public static void FindGameObject(this Transform transform, string name, Action<GameObject> gameObjectHandler)
	{
		foreach (Transform child in transform)
		{
			child.FindGameObject(name, gameObjectHandler);
			if (child.gameObject.name.Equals(name))
				gameObjectHandler?.Invoke(child.gameObject);
		}
	}

	public static List<T> GetAllComponentsInChildren<T>(this Transform transform)
	{
		var output = new List<T>();
		foreach (RectTransform child in transform)
		{
			var component = child.GetComponent<T>();
			if (component != null)
			{
				output.Add(component);
			}
			output.AddRange(child.GetAllComponentsInChildren<T>());
		}
		return output;
	}


	public static Transform Clear(this Transform transform)
	{
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		return transform;
	}
}