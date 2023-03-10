using BugsFarm.Services.BootstrapService;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.FarmCameraSystem
{
    public class FarmCameraCalibrationCommand : Command
    {
        private readonly FarmCameraSceneObject _cameraSceneObject;
        private readonly CameraConstraintsModel _model;
        private readonly IInstantiator _instantiator;

        public FarmCameraCalibrationCommand(FarmCameraSceneObject cameraSceneObject, 
                                            CameraConstraintsModel model,
                                            IInstantiator instantiator)
        {
            _cameraSceneObject = cameraSceneObject;
            _model = model;
            _instantiator = instantiator;
        }
        public override void Do()
        {
            var calibration = _model.CalibrationPresset;
            var boundaries = _instantiator.InstantiatePrefabForComponent<FarmTriggerBoundaries>(calibration);
            _cameraSceneObject.transform.position = boundaries.transform.position;
            boundaries.TestTrigger();
            Object.Destroy(boundaries.gameObject);
            OnDone();
        }
    }
}