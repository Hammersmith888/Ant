using System;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.CommandService;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class UnitSetStageCommand : ICommand<UnitSetStageProtocol>
    {
        private readonly UnitSceneObjectStorage _viewStorage;
        private readonly UnitModelStorage _unitsModelStorage;
        private readonly UnitDtoStorage _unitsDtoStorge;
        private readonly UnitStageModelStorage _stageModelsStorage;
        private readonly SpriteStageLoader _spriteStageLoader;
        private readonly SpineLoader _spineLoader;

        public UnitSetStageCommand(UnitSceneObjectStorage viewStorage,
                                   UnitModelStorage unitsModelStorage,
                                   UnitDtoStorage unitsDtoStorge,
                                   UnitStageModelStorage stageModelsStorage,
                                   SpriteStageLoader spriteStageLoader,
                                   SpineLoader spineLoader)
        {
            _viewStorage = viewStorage;
            _unitsModelStorage = unitsModelStorage;
            _unitsDtoStorge = unitsDtoStorge;
            _stageModelsStorage = stageModelsStorage;
            _spriteStageLoader = spriteStageLoader;
            _spineLoader = spineLoader;
        }

        public Task Execute(UnitSetStageProtocol protocol)
        {
            if (!_unitsDtoStorge.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException();
            }
            
            var dto = _unitsDtoStorge.Get(protocol.Guid);
            if (!_unitsModelStorage.HasEntity(dto.ModelID))
            {
                throw new InvalidOperationException();
            }

            var model = _unitsModelStorage.Get(dto.ModelID);
            if (!_stageModelsStorage.HasEntity(dto.ModelID))
            {
                throw new InvalidOperationException();
            }
            
            if (!_viewStorage.HasEntity(dto.Guid))
            {
                throw new InvalidOperationException();
            }
            
            var stageModel = _stageModelsStorage.Get(model.ModelID);
            var maxStageIndex = stageModel.Count - 1;
            var view = _viewStorage.Get(protocol.Guid);
            var stageIndex = Mathf.Clamp(protocol.StageIndex, 0, maxStageIndex);

            var stageSprite = _spriteStageLoader.Load(stageModel.Path, model.TypeName, stageIndex);
            var stageSpine  = _spineLoader.LoadStage(stageModel.Path, model.TypeName, stageIndex);
            
            view.SetObject(stageSprite, stageSpine);
            return Task.CompletedTask;
        }
    }
}