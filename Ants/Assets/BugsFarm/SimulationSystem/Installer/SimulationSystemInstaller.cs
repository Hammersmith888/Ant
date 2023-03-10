using System;
using Zenject;

namespace BugsFarm.SimulationSystem
{
    public class SimulationSystemInstaller : Installer<SimulationSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(ITickableManagerInternal),
                           typeof(ITickableManager),
                           typeof(ITickable),
                           typeof(IDisposable))
                .To<SimulationTickableManger>()
                .AsSingle();

           Container.Bind(typeof(ILateTickable),
                           typeof(ISimulationSystem))
                .To<SimulationSystem>()
                .AsSingle();


        }
    }
}