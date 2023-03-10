using System;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingPlaceSystemInstaller: Installer<BuildingPlaceSystemInstaller>
    { 
        public override void InstallBindings()
        {
            Container
                .Bind<IReservedPlaceSystem>()
                .To<ReservedPlaceSystem>()
                .AsSingle(); 
            
            Container
                .Bind<PlaceIdStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .Bind<IFreePlaceSystem>()
                .To<FreePlaceSystem>()
                .AsSingle();

            Container
                .Bind(typeof(IPlaceOpenableSystem), typeof(IInitializable), typeof(IDisposable))
                .To<PlaceOpenableSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}