using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.RoomSystem
{
    [Serializable]
    public struct RoomNeighbourModel : IStorageItem
    {
        public string ModelID;
        public string[] Neighbours;
        
        string IStorageItem.Id => ModelID;
    }
}