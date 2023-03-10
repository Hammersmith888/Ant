using System;
using System.Collections;
using System.Collections.Generic;

public static class IEnumerableExtensions
{
	public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		foreach (var element in enumerable)
		{
			action.Invoke(element);
		}
	}

	public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
	{
		int count = 0;
		foreach (var element in enumerable)
		{
			action?.Invoke(element, count);
			count++;
		}
	}
}