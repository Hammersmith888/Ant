using System;
using System.Collections.Generic;
using System.Linq;

namespace BugsFarm.BuildingSystem
{
    public readonly struct AddResourceProtocol
    {
        public readonly string ItemID;
        public readonly string Guid;
        public readonly Type TaskType;
        public readonly int MaxUnits;
        public readonly string ModelID;
        public readonly IEnumerable<IPosSide> Points;
        public readonly ResourceArgs Args;
        public readonly bool TaskNotify;
        public readonly object[] TaskExtraArgs;

        public AddResourceProtocol(string guid,
                                   string modelID,
                                   string itemID,
                                   int maxUnits,
                                   IEnumerable<IPosSide> points,
                                   Type taskType,
                                   ResourceArgs args,
                                   bool taskNotify,
                                   IEnumerable<object> taskExtraArgs = null)
        {
            Guid = guid;
            ItemID = itemID;
            MaxUnits = maxUnits;
            Points = points;
            TaskType = taskType;
            ModelID = modelID;
            Args = args ?? ResourceArgs.Default();
            TaskNotify = taskNotify;
            TaskExtraArgs = taskExtraArgs?.ToArray() ?? new object[0];
        }
    }
}