using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public interface IPosSide
    {
        bool LookLeft { get;}
        Vector2 Position { get; }
    }
}
