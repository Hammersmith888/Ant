using BugsFarm.Game;
using UnityEngine;


public class FoodGarbage : Tapable
{
#pragma warning disable 0649

    [SerializeField] MB_Food _food;

#pragma warning restore 0649

    protected override void OnTapped()
    {
        if (_food)
        {
            GameEvents.OnObjectTap?.Invoke(_food);
        }
    }
}

