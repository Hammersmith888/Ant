using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class DeleteUnitSceneObjectCommand : ICommand<DeleteUnitSceneObjectProtocol>
    {

        private readonly UnitSceneObjectStorage _unitViewStorage;
        
        public DeleteUnitSceneObjectCommand(UnitSceneObjectStorage unitViewStorage)
        {     

            _unitViewStorage = unitViewStorage;
        }
        
        public Task Execute(DeleteUnitSceneObjectProtocol protocol)
        {
            if (!_unitViewStorage.HasEntity(protocol.UnitId))
            {
                return Task.CompletedTask;
            }
            var view = _unitViewStorage.Get(protocol.UnitId);
            _unitViewStorage.Remove(protocol.UnitId);
            if (view)
            {
                Object.Destroy(view.gameObject);
            }
            return Task.CompletedTask;
        }
    }
}