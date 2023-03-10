
using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct PlaceBuildingProtocol : IProtocol
    {
        public readonly string ModelID;
        public readonly string Guid;
        public readonly string PlaceNum;
        public readonly bool InternalPlace;

        public PlaceBuildingProtocol(string modelId, string guid, string placeNum, bool internalPlace = false)
        {
            PlaceNum = placeNum;
            InternalPlace = internalPlace;
            ModelID = modelId;
            Guid = guid;
        }
    }
}