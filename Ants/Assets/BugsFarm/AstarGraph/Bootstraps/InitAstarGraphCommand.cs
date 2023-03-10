using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Utility;
using Zenject;

namespace BugsFarm.AstarGraph
{
    public class InitAstarGraphCommand : Command
    {
        private readonly PointGraphConfig _pointGraphConfig;
        private readonly IInstantiator _instantiator;
        private readonly IPointGraph _pointGraph;
        private readonly PathModelStorage _scenePathModelStorage;

        public InitAstarGraphCommand(PointGraphConfig pointGraphConfig,
                                     IInstantiator instantiator,
                                     IPointGraph pointGraph,
                                     PathModelStorage scenePathModelStorage)
        {
            _pointGraphConfig = pointGraphConfig;
            _instantiator = instantiator;
            _pointGraph = pointGraph;
            _scenePathModelStorage = scenePathModelStorage;
        }

        public override void Do()
        {
            _instantiator.Instantiate<InitModelsCommand<GraphMaskModel>>().Do();
            foreach (var model in _pointGraphConfig.Items)
            {
                _scenePathModelStorage.Add(model);
            }

            _pointGraph.Initialize();
            OnDone();
        }
    }
}