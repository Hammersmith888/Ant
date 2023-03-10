using System;
using System.Collections.Generic;

namespace BugsFarm.BuildingSystem
{
    public readonly struct GetResourceProtocol
    {
        public readonly string ItemID;
        public readonly string Guid;
        public readonly string ModelID;
        public readonly Type TaskType;
        public readonly int MaxUnits;
        public readonly IEnumerable<IPosSide> Points;
        public readonly ResourceArgs Args;

        public GetResourceProtocol(string guid, 
                                   string modelID,
                                   string itemID, 
                                   int maxUnits, 
                                   IEnumerable<IPosSide> points, 
                                   Type taskType,
                                   ResourceArgs args)
        {
            Guid = guid;
            ItemID = itemID;
            MaxUnits = maxUnits;
            Points = points;
            TaskType = taskType;
            ModelID = modelID;
            Args = args ?? ResourceArgs.Default();
        }
    }
}