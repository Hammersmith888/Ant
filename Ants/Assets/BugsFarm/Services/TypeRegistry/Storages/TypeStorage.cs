using System;
using System.Linq;
using System.Reflection;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.Services.TypeRegistry
{
    public class TypeStorage : Storage<TypeItem>
    {
        private readonly TypeInfo[] _types;
        public TypeStorage()
        {
            _types = Assembly.GetExecutingAssembly().DefinedTypes.ToArray();
        }
        
        public void Registry(params Type[] types)
        {
            if (types.Length == 0)
            {
                return;
            }
            
            Registry(types.Select(x => x?.Name ?? "").ToArray());
        }
        public void Registry(params string[] typeNames)
        {
            if (typeNames.Length == 0)
            {
                return;
            }
            
            foreach (var typeName in typeNames)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    Debug.LogError("Type is empty.");
                    continue;
                }
                
                var type = _types.FirstOrDefault(x => x.Name == typeName);
                if (type == null)
                {
                    Debug.LogError($"Type with TypeName : {typeName}, does not exist.");
                    continue;
                }
                
                Add(new TypeItem {Id = typeName, Type = type});
            }   
        }
    }
}