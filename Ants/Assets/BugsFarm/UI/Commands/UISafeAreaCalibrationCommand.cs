using BugsFarm.FarmCameraSystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISafeAreaCalibrationCommand : Command
    {
        private readonly UIRoot _uiRoot;
        private readonly FarmCameraController _cameraController;

        public UISafeAreaCalibrationCommand(UIRoot uiRoot, FarmCameraController cameraController)
        {
            _uiRoot = uiRoot;
            _cameraController = cameraController;
        }
        public override void Do()
        {
            _uiRoot.UICamera.orthographicSize = _cameraController.Camera.orthographicSize;
            var canvasScaler = _uiRoot.UICanvas.GetComponent<CanvasScaler>();
            _uiRoot.SafeArea.offsetMax = new Vector2(0, -Tools.UnsafeTop_Ref(canvasScaler));
            OnDone();
        }
    }
}