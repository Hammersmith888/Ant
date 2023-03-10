using System.Linq;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.StorageService;
using BugsFarm.Services.TypeRegistry;
using BugsFarm.Utility;

namespace BugsFarm.BootstrapCommon
{
    public class InitModelsCommand<T> : Command where T : IStorageItem
    {
        private readonly string _fileName;
        private readonly TypeStorage _typeStorage;
        private readonly IStorage<T> _modelsStorage;
        
        public InitModelsCommand(TypeStorage typeStorage, IStorage<T> modelsStorage, string fileName = "")
        {
            _fileName = fileName;
            _typeStorage = typeStorage;
            _modelsStorage = modelsStorage;
        }
        
        public override void Do()
        {
            const string fieldName = "typename";
            var fileName = string.IsNullOrEmpty(_fileName) ? (typeof(T).Name + "s") : _fileName;
            var config = ConfigHelper.Load<T>(fileName);
            var fieldInfo = typeof(T).GetFields().FirstOrDefault(x => x.Name.ToLower() == fieldName);
            var canAddType = fieldInfo != null;
            
            foreach (var model in config)
            {
                _modelsStorage.Add(model);
                if (!canAddType) continue;
                
                var typeName = (string)fieldInfo.GetValue(model) ?? "";
                if (!string.IsNullOrEmpty(typeName) && !_typeStorage.HasEntity(typeName))
                {
                    _typeStorage.Registry(typeName);
                }
            }
            OnDone();
        }
    }
}