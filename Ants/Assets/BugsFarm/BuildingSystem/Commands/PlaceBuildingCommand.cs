using System;
using System.Threading.Tasks;
using BugsFarm.AudioSystem;
using BugsFarm.Services.CommandService;
using UniRx;

namespace BugsFarm.BuildingSystem
{
    public class PlaceBuildingCommand : ICommand<PlaceBuildingProtocol>
    {
        private readonly ISoundSystem _soundSystem;
        private readonly IFreePlaceSystem _freePlaceSystem;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly PlaceIdStorage _placeStorage;
        private readonly BuildingModelStorage _modelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingsContainer _buildingsContainer;
        private readonly IReservedPlaceSystem _reservedPlaceSystem;

        public PlaceBuildingCommand(ISoundSystem soundSystem,
                                    IFreePlaceSystem freePlaceSystem,
                                    AudioModelStorage audioModelStorage,
                                    BuildingSceneObjectStorage buildingSceneObjectStorage,
                                    PlaceIdStorage placeStorage,
                                    BuildingModelStorage modelStorage,
                                    BuildingDtoStorage buildingDtoStorage,
                                    BuildingsContainer buildingsContainer,
                                    IReservedPlaceSystem reservedPlaceSystem)
        {
            _soundSystem = soundSystem;
            _freePlaceSystem = freePlaceSystem;
            _audioModelStorage = audioModelStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _placeStorage = placeStorage;
            _modelStorage = modelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _buildingsContainer = buildingsContainer;
            _reservedPlaceSystem = reservedPlaceSystem;
        }
        public Task Execute(PlaceBuildingProtocol protocol)
        {
            if (!_buildingDtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(BuildingDto)} with [Guid : {protocol.Guid}] does not exist");
            }
            
            var buildingDto = _buildingDtoStorage.Get(protocol.Guid);
            if (!_buildingSceneObjectStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(BuildingSceneObject)} with [Guid : {protocol.Guid}] does not exist");
            }
            
            var view = _buildingSceneObjectStorage.Get(protocol.Guid);
            if (protocol.PlaceNum == PlaceBuildingUtils.OffScreenPlaceNum)
            {
                view.transform.position = _buildingsContainer.Outside.position;
                return Task.CompletedTask;
            }
            
            if (!_placeStorage.HasEntity(protocol.PlaceNum))
            {
                throw new ArgumentException($"{this} : PlaceID with [PlaceNum : {protocol.PlaceNum}] does not exist");
            }
            
            var placeId = _placeStorage.Get(protocol.PlaceNum);
            if (!placeId.HasPlace(protocol.ModelID))
            {
                throw new ArgumentException($"{this} : APlace with [ModelId : {protocol.ModelID}, PlaceNum : {protocol.PlaceNum}] does not exist");
            }

            // free placenum internal
            if (_reservedPlaceSystem.HasEntity(protocol.PlaceNum) && protocol.InternalPlace)
            {
                _freePlaceSystem.FreePlaceInternal(buildingDto.ModelID, buildingDto.PlaceNum, protocol.Guid);
            }
            
            // Free old placenum
            if (_reservedPlaceSystem.HasEntity(buildingDto.PlaceNum))
            {
                _reservedPlaceSystem.Remove(buildingDto.PlaceNum, !protocol.InternalPlace);
            }
            
            var place = placeId.GetPlace(protocol.ModelID);
            buildingDto.PlaceNum = protocol.PlaceNum;
            _reservedPlaceSystem.ReservePlace(protocol.PlaceNum, protocol.Guid, !protocol.InternalPlace);
            
            view.SetPlace(place); 
            view.SetLayer(place.Layer);
            
            if(!protocol.InternalPlace)
            {
                _soundSystem.Play(view.transform.position, GetPlaceAudioClip(buildingDto.ModelID));
            }

            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }

        private string GetPlaceAudioClip(string modelID)
        {
            const string audioModelName = "Buildings";
            var audioModel = _audioModelStorage.Get(audioModelName);
            var model = _modelStorage.Get(modelID);
            switch (model.TypeID)
            {
                case 1:
                case 2:  return audioModel.GetAudioClip("BuildPlace");
                default: return audioModel.GetAudioClip("ObjectsPlace");
            }
        }
    }
}