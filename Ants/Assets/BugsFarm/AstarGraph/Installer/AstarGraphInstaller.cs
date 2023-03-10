using UnityEngine;
using Zenject;

namespace BugsFarm.AstarGraph
{
    public class AstarGraphInstaller : Installer<AstarGraphInstaller>
    {
        public override void InstallBindings()
        {
            // Storages
            Container
                .BindInterfacesAndSelfTo<GraphMaskModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<PathModelStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .Bind<AstarPath>()
                .FromComponentInNewPrefabResource("Graph/Pathfinder")
                .AsSingle();            
            
            Container
                .Bind<PointGraphConfig>()
                .FromScriptableObjectResource("Graph/PointGraphConfig")
                .AsSingle();
            
            Container
                .Bind<CellsArea>()
                .FromComponentInNewPrefabResource("Graph/CellsArea")
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<PointGraph>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<PathOpenableSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}