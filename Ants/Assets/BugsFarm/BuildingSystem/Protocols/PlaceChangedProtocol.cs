using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public struct PlaceChangedProtocol : IProtocol
    {
        public string GroupeId;

        public PlaceChangedProtocol(string groupeId)
        {
            GroupeId = groupeId;
        }
    }
}