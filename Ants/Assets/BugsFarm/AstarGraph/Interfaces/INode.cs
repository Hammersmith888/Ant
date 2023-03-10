using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public interface INode
    {
        string Group { get; set; }
        string LayerName { get; set; }
        int LayerIndex { get; set; }
        Vector2 Normal { get; set; }
        Vector2 Position { get; }

        /// <summary>
        /// Тег слоя - индекс в массиве тегов
        /// </summary>
        uint Tag { get; set; }
        uint Penalty { get; set; }
        bool Walkable { get; set; }
        bool Excluded { get; set; }
    }
}