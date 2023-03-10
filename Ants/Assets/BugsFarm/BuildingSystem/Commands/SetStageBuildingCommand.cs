using System;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.CommandService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class SetStageBuildingCommand : ICommand<SetStageBuildingProtocol>
    {
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorge;
        private readonly BuildingStageModelStorage _stageModelsStorage;
        private readonly SpriteStageLoader _spriteStageLoader;

        public SetStageBuildingCommand(BuildingSceneObjectStorage viewStorage,
                                       BuildingModelStorage buildingModelStorage,
                                       BuildingDtoStorage buildingDtoStorge,
                                       BuildingStageModelStorage stageModelsStorage,
                                       SpriteStageLoader spriteStageLoader)
        {
            _viewStorage = viewStorage;
            _buildingModelStorage = buildingModelStorage;
            _buildingDtoStorge = buildingDtoStorge;
            _stageModelsStorage = stageModelsStorage;
            _spriteStageLoader = spriteStageLoader;
        }

        public Task Execute(SetStageBuildingProtocol protocol)
        {
            if (!_buildingDtoStorge.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException();
            }
            
            var buildingDto = _buildingDtoStorge.Get(protocol.Guid);
            if (!_buildingModelStorage.HasEntity(buildingDto.ModelID))
            {
                throw new InvalidOperationException();
            }

            var model = _buildingModelStorage.Get(buildingDto.ModelID);
            if (!_stageModelsStorage.HasEntity(buildingDto.ModelID))
            {
                return Task.CompletedTask;
            }
            
            if (!_viewStorage.HasEntity(buildingDto.Guid))
            {
                return Task.CompletedTask;
            }

            const float offsetFloor = 0.001f; // для поддержки штанов, чтобы FloorToInt не сделал так : 4.99999 в 5.
            var view       = _viewStorage.Get(protocol.Guid);
            var stages     = _stageModelsStorage.Get(model.ModelID);
            var stageCount = stages.Count;
            var progress   = 1f - (protocol.CurrValue / protocol.MaxValue); // чем больше ресурса - тем меньше индекс.
            var stageIndex = Mathf.FloorToInt(Mathf.Clamp((stageCount * progress) - offsetFloor, 0, stageCount));
            // компенсация когда ресурс положили но, максимальный ресурс является большим, а этапов мало.
            // по этому чтобы достич первого этапа выполняем компенсацию и сдвигаем первый этап,
            // визуально на сцене произойдут изменения при добавлении или удалении на малых значениях ресурса.
            if (stageCount > 1 && protocol.CurrValue > 0 && stageIndex == (stageCount - 1))
            {
                stageIndex = Math.Max(0, stageIndex - 1);
            }
            
            if (stages.Path == "Spine")
            {
                // для спайновских анимаций используется индекс скина
                const int skipDefaultIndex = 1;
                view.SetObject(stageIndex + skipDefaultIndex);
            }
            else
            {
                // для спрайтов загружается изображение
                var stage = _spriteStageLoader.Load(stages.Path, model.TypeName, stageIndex);
                view.SetObject(stage); 
            }

            protocol.StageIndexAction?.Invoke(new StageActionProtocol(stageCount-1, stageIndex));

            return Task.CompletedTask;
        }
    }
}