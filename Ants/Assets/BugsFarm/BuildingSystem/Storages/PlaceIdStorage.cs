using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class PlaceIdStorage : Storage<PlaceID>
    {
        public PlaceID GetRandom(Func<PlaceID, bool> predicate)
        {
            return Tools.RandomItem(Get(predicate).ToArray());
        }
        
        public IEnumerable<PlaceID> Get(Func<PlaceID, bool> predicate)
        {
            return Dict.Values.Where(predicate.Invoke);
        }
    }
}
