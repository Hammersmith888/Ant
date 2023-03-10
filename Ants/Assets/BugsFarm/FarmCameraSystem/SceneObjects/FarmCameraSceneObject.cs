using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class FarmCameraSceneObject : MonoBehaviour
    {
        public Camera Camera => _camera;
        
        [SerializeField] private Camera _camera;
    }
}