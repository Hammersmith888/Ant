using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct CreateUnitProtocol : IProtocol
    {
        public readonly string ModelID;
        public readonly string Guid;
        public readonly object[] Args;

        public CreateUnitProtocol(string id, bool isModel, params object[] args)
        {
            if (isModel)
            {
                ModelID = id;
                Args = args ?? new object[0];
                Guid = System.Guid.NewGuid().ToString();
            }
            else
            {
                Guid = id;
                ModelID = null;
                Args = args ?? new object[0];
            }
        }
    }
}