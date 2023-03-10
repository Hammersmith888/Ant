using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.RoomSystem
{
    [Serializable]
    public class RoomDto : IStorageItem
    {
        public string ModelID;
        public string Guid;
        
        string IStorageItem.Id => Guid;

        public RoomDto(string modelID, string guid)
        {
            ModelID = modelID;
            Guid = guid;
        }
    }
}