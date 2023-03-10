using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct CreateBuildingProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly string ModelID;
        public readonly string PlaceNum;
        public readonly bool InternalBuild;

        public CreateBuildingProtocol(string id, string placeNum, bool isModel, bool internalBuild = false)
        {
            if (isModel)
            {
                Guid = System.Guid.NewGuid().ToString();
                ModelID = id;
                PlaceNum = placeNum;
                InternalBuild = internalBuild;
            }
            else
            {
                Guid = id;
                ModelID = null;
                PlaceNum = null;
                InternalBuild = internalBuild;
            }
        }
    }
}