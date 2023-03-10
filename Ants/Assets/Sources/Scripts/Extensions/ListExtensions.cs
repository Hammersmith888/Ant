using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static bool TryFind<T>(this List<T> list, Predicate<T> predicate, out T output)
    {
        int index = list.FindIndex(predicate);
        if (index != -1)
        {
            output = list[index];
            return true;
        }
        output = default;
        return false;
    }
}
