using UnityEngine;
using Zenject;

namespace BugsFarm.FarmCameraSystem
{
    public class FarmCameraInstaller : Installer<FarmCameraInstaller>
    { 
        public override void InstallBindings()
        {
            Container
                .Bind<CameraConstraintsModel>()
                .FromScriptableObjectResource("FarmCamera/CameraConstraintsModel")
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<CameraCinematicsModel>()
                .FromScriptableObjectResource("FarmCamera/CameraCinematicsModel")
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<CameraVerticalMovementModel>()
                .FromScriptableObjectResource("FarmCamera/CameraVerticalMovementModel")
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<CinematicsResolvedStatesStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<FarmCameraSceneObject>()
                .FromComponentInNewPrefabResource("FarmCamera/FarmCameraSceneObject")
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<FarmCameraController>()
                .AsSingle()
                .NonLazy();
        }
    }
}