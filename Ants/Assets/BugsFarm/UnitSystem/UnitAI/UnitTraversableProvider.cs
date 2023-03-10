using BugsFarm.AstarGraph;
using Pathfinding;

namespace BugsFarm.UnitSystem
{
    public class UnitTraversableProvider : ITraversalProvider
    {
        private readonly string _modelId;

        public UnitTraversableProvider(string modelId)
        {
            _modelId = modelId;
        }
        public bool CanTraverse(Path path, GraphNode graphNode)
        {
            var node = (Node) graphNode;
            return DefaultITraversalProvider.CanTraverse(path, node) && !node.HasUnitLimitation(_modelId);
        }

        public uint GetTraversalCost(Path path, GraphNode node)
        {
            return DefaultITraversalProvider.GetTraversalCost(path, node);
        }
    }
}