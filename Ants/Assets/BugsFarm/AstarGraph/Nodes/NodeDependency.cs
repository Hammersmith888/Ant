namespace BugsFarm.AstarGraph
{
    public class NodeDependency : Node
    {
        public bool DependencyWalkable { get; private set; }
        public int  DependencyID { get; private set; }

        public NodeDependency(NodeData data) : base(data)
        {
            DependencyWalkable = data.DependencyWalkable;
            DependencyID = data.DependencyID;
        }
    }
}