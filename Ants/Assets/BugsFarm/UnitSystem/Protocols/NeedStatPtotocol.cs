using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct NeedStatPtotocol : IProtocol
    {
        public readonly string Guid;
        public readonly string NeedPrefix;
        public readonly string ResourceStatKey;
        public readonly string NoNeedTimeStatKey;
        public readonly string NeedTimeStatKey;
        public readonly Action<string> OnNeed;

        public NeedStatPtotocol(string guid,
                                string needPrefix,
                                string resourceStatKey,
                                string noNeedTimeStatKey,
                                string needTimeStatKey,
                                Action<string> onNeed = null)
        {
            Guid = guid;
            NeedPrefix = needPrefix;
            ResourceStatKey = resourceStatKey;
            NoNeedTimeStatKey = noNeedTimeStatKey;
            NeedTimeStatKey = needTimeStatKey;
            OnNeed = onNeed;
        }
    }
}