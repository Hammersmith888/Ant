using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Services.TypeRegistry
{
    public struct TypeItem : IStorageItem
    {
        public string Id { get; set; }
        public Type Type;
    }
}