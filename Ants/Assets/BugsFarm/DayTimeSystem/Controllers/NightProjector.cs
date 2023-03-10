using System.Linq;
using BugsFarm.FarmCameraSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.DayTimeSystem
{
    public class NightProjector
    {
        private readonly FarmCameraSceneObject _gameCamera;
        private readonly IInstantiator _instantiator;
        private NightRTSceneObject _rtSceneObject;
        private RenderTexture _renderTexture;
        private GameObject _masksContainer;
        private const float _renderScale = 0.5f;
        
        public NightProjector(FarmCameraSceneObject gameCamera, 
                              IInstantiator instantiator)
        {
            _gameCamera = gameCamera;
            _instantiator = instantiator;
        }

        public void Initialize()
        {
            if (_rtSceneObject)
            {
                return;
            }
            
            _rtSceneObject = _instantiator
                .InstantiatePrefabResourceForComponent<NightRTSceneObject>("DayTime/NightRTSceneObject",_gameCamera.transform);
            _masksContainer = _instantiator.CreateEmptyGameObject("NightMasksContainer");
            var maskPrefabs = Resources.LoadAll("DayTime/Masks/");
            foreach (var maskPrefab in maskPrefabs)
            {
                _instantiator.InstantiatePrefab(maskPrefab, _masksContainer.transform);
            }
            
            var width  = Mathf.FloorToInt(Screen.width  * _renderScale);
            var height = Mathf.FloorToInt(Screen.height * _renderScale);
            _renderTexture = new RenderTexture(width, height, 0);
            _rtSceneObject.SetupCamera(_gameCamera.Camera);
            _rtSceneObject.SetRenderTexture(_renderTexture);

            var newMesh = _rtSceneObject.GetMesh();
            newMesh.vertices = newMesh.uv.Select(point => _rtSceneObject.UvToRTSpace(point)).ToArray();
            newMesh.name = "RTMesh";
            _rtSceneObject.SetMesh(newMesh);
        }

        public void Dispose()
        {
            if (!_rtSceneObject)
            {
                return;
            }
            Object.Destroy(_rtSceneObject.gameObject);
            Object.Destroy(_masksContainer);
            _renderTexture.Release();
            _renderTexture = null;
        }
    }
}

