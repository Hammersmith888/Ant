using System;
using System.Collections.Generic;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UI
{
    [Serializable]
    public struct MyBugItemModel : IStorageItem
    {
        string IStorageItem.Id => ModelId;
        public string ModelId;
        public List<string> Stats;
    }
}