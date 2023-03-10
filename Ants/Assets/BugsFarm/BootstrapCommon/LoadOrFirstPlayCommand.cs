using BugsFarm.BuildingSystem;
using BugsFarm.ChestSystem;
using BugsFarm.LeafHeapSystem;
using BugsFarm.Quest;
using BugsFarm.RoomSystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.SimulatingSystem;
using BugsFarm.SimulationSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using UnityEngine;
using Zenject;

namespace BugsFarm.BootstrapCommon
{
    public class LoadOrFirstPlayCommand : Command
    {
        public LoadOrFirstPlayCommand(IUser user,
                                      IBootstrap bootstrap,
                                      IInstantiator instantiator,
                                      ISaveManager saveManager)
        {
            var userDataPath = PathConstants.GetUserDataPath(user.Id);
            if (saveManager.HasSaves(userDataPath))
            {
                saveManager.LoadAll(userDataPath);
                Debug.Log($"{this} : Loaded for UserID : {user.Id}");
                bootstrap.AddCommand(instantiator.Instantiate<InitUserCommand>()); // after load data, before all
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadAllRoomsStatsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadAllBuildingsStatsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadAllUnitsStatsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<SimulatingCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadLeafHeapsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadChestsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadRoomsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadBuildingsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadUnitsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostLoadUnitsRipCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostSimulatingCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<PostInitializeCommand>());
                //  bootstrap.AddCommand(instantiator.Instantiate<SimulationCommand>());
                
            }
            else
            {
                Debug.Log($"{this} : First play for UserID : {user.Id}");
                bootstrap.AddCommand(instantiator.Instantiate<InitUserCommand>()); // befor all
                bootstrap.AddCommand(instantiator.Instantiate<InitFirstPlayLeafHeapsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<InitFirstPlayChestsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<InitCreateRoomsCommand>());
                bootstrap.AddCommand(instantiator.Instantiate<InitQuestsCommand>());
            }
        }
    }
}