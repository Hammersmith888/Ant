using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class NodeData
    {
        public NodeData()
        {
            Default();
        }
        public NodeData(INode copyFrom)
        {
            if (copyFrom == null)
            {
                Default();
                return;
            }

            Normal = copyFrom.Normal;
            Group = copyFrom.Group;
            Layer = copyFrom.LayerName;

            Position = copyFrom.Position;
            Penalty = copyFrom.Penalty;
            Tag = copyFrom.Tag;
            Walkable = copyFrom.Walkable;
            Excluded = copyFrom.Excluded;
            FromNode = copyFrom;
        }
        private void Default()
        {
            Normal = Vector2.zero;
            Group = "-1";
            Layer = "Default";

            Position = Vector2.zero;
            Penalty = 0;
            Tag = 0;
            Walkable = true;
            DependencyWalkable = false;
            DependencyID = -1;
            FromNode = null;
        }

        public INode FromNode { get; set; }
        public Vector2 Normal { get; set; }
        public string Group { get; set; }
        public string Layer { get; set; }
        public AstarPath Astar => AstarPath.active;
        public Vector2 Position { get; set; }
        public uint Tag { get; set; }
        public uint Penalty { get; set; }
        public bool Walkable { get; set; }
        public bool DependencyWalkable { get; set; }
        public bool Excluded { get; set; }
        public int  DependencyID { get; set; }
    }
}
