using System;
using BugsFarm.Model.Enum;

namespace BugsFarm.Services
{
    public class ResourceService
    {
        private readonly ResourceContext _resource;

        public ResourceService(ResourceContext resource)
        {
            _resource = resource;
        }

        public int GetResource(EResourceType type)
        {
            switch (type)
            {
                case EResourceType.FoodStock:
                    return _resource.foodStock.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void UpdateResource(EResourceType type, int value)
        {
            switch (type)
            {
                case EResourceType.FoodStock:
                    _resource.ReplaceFoodStock(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}