using System;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Zenject;

namespace BugsFarm.FarmCameraSystem
{
    public class Camera2DSystem : IInitializable, IDisposable, ICamera2DSystem
    {
        public bool Enabled => _proCamera2D.enabled;
        public bool FollowedVertical => _proCamera2D.FollowVertical;
        public bool FollowedHorizontal => _proCamera2D.FollowHorizontal;
        
        private readonly Camera _camera;
        
        private ProCamera2D _proCamera2D;
        public Camera2DSystem(Camera camera)
        {
            _camera = camera;
        }
        public void Initialize()
        {
            _proCamera2D = _camera.GetComponent<ProCamera2D>();
        }
        public void Dispose()
        {
            _proCamera2D = null;
        }
        public void Reset()
        {
            _proCamera2D.Reset();
        }
        public void ResetMovement()
        {
            _proCamera2D.ResetMovement();
        }
        public void Enable(bool enable)
        {
            _proCamera2D.enabled = enable;
        }
        public void FollowVertical(bool allow)
        {
            _proCamera2D.FollowVertical = allow;
        }

        public void FollowHorizontal(bool allow)
        {
            _proCamera2D.FollowHorizontal = allow;
        }
    }
}