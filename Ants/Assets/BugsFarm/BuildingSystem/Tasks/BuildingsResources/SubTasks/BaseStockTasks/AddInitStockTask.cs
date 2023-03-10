using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AddInitStockTask : AddResourceBootstrapTask
    {
        private PlaceIdStorage _placeIdStorage;
        private IReservedPlaceSystem _reservedPlaceSystem;
        private BuildingDto _selfDto;

        [Inject]
        private void Inject(BuildingDtoStorage dtoStorage,
                            PlaceIdStorage placeIdStorage,
                            IReservedPlaceSystem reservedPlaceSystem)
        {
            _placeIdStorage = placeIdStorage;
            _reservedPlaceSystem = reservedPlaceSystem;
            
            if (!dtoStorage.HasEntity(ResourceController.OwnerGuid))
            {
                Interrupt();
                return;
            }

            _selfDto = dtoStorage.Get(ResourceController.OwnerGuid);
        }

        public override void Execute(params object[] args)
        {
            if (_selfDto.IsNullOrDefault())
            {
                Interrupt();
                return;
            }
            
            if (_selfDto.PlaceNum == PlaceBuildingUtils.OffScreenPlaceNum)
            {
                if (FindPlace(out var placeID))
                {
                    var placeProtocol = new PlaceBuildingProtocol(_selfDto.ModelID, _selfDto.Guid, placeID.PlaceNumber, true);
                    Instantiator.Instantiate<PlaceBuildingCommand>().Execute(placeProtocol);
                }
                else
                {
                    Interrupt();
                    return;
                }
            }
            
            base.Execute(args);
        }

        // Пока сток не инициализирован он находится за пределами игрового поля,
        // позиции для выполнения задачи не будет достигатся юнитами.
        // Находим будущее Место под сток и выбираем в качестве своей позиции.
        public override Vector2[] GetPositions()
        {
            return FindPlace(out var placeId)
                ? new[] {(Vector2)placeId.GetPlace(_selfDto.ModelID).transform.position}
                : base.GetPositions();
        }
        
        private bool FindPlace(out PlaceID placeID)
        {
            if (_selfDto.IsNullOrDefault())
            {
                return placeID = default;
            }
            
            return placeID = _placeIdStorage.GetRandom(x => !_reservedPlaceSystem.HasEntity(x.PlaceNumber) && 
                                                            x.HasPlace(_selfDto.ModelID));
        }

        protected override void OnDisposed()
        {
            _selfDto = null;
            _placeIdStorage = null;
            _reservedPlaceSystem = null;
            base.OnDisposed();
        }
    }
}