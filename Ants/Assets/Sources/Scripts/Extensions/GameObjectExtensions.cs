using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetActive(this List<GameObject> gameObjects, bool isActive)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].SetActive(isActive);
        }
    }
}