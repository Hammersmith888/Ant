using BugsFarm.Services.CommandService;

namespace BugsFarm.Services.SceneEntity
{
    // do not move enum parts - used in Unity inspector!
    public enum SceneObjectType
    {
        Unit,
        AntRip,
        Building,
        Rooms,
        Chests,
        OrderBoard,
        AntHill,
        Safe,
        Hospital,
        Bowl,
        AllResurrection
    }
    
    public struct InteractionProtocol : IProtocol
    {
        public string Guid;
        public SceneObjectType ObjectType;
        
        public InteractionProtocol(string guid, SceneObjectType objectType)
        {
            Guid = guid;
            ObjectType = objectType;
        }
    }
}