using System;
using System.Collections.Generic;

namespace BugsFarm.BuildingSystem
{
    public interface IFreePlaceSystem
    {
        void FreePlace(string modelID, string placeNum, Action<bool> onComplete = null, string excludeGuid = "");
        void FreePlaceInternal(string modelID, string placeNum, string excludeGuid = "");
        bool CanFreePlace(string modelID, string placeNum, out List<string> neighbours, string excludeGuid = "");
    }
}