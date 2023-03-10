using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UI;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class FreePlaceSystem : IFreePlaceSystem
    {
        private readonly PlaceIdStorage _placeIdStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingParamsModelStorage _buildingParamsModelStorage;
        private readonly BuildingModelStorage _modelStorage;
        private readonly IReservedPlaceSystem _reservedPlaceSystem;
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;

        private const string _deleteParam = "Delete";
        private const string _warningNeedDeleteTextKey = "FreePlace_NeedDeleteItems";
        private const string _errorCannotDeleteTextKey = "FreePlace_CannotDeleteItems";
        private const string _buildingsNameTextKey = "BuildingsName_";
        private Action<bool> _onFreeCompleteAction;
        private List<string> _overlapped;

        public FreePlaceSystem(PlaceIdStorage placeIdStorage,
                               BuildingDtoStorage buildingDtoStorage,
                               BuildingParamsModelStorage buildingParamsModelStorage,
                               BuildingModelStorage modelStorage,
                               IReservedPlaceSystem reservedPlaceSystem,
                               IInstantiator instantiator,
                               IUIService uiService)
        {
            _placeIdStorage = placeIdStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _buildingParamsModelStorage = buildingParamsModelStorage;
            _modelStorage = modelStorage;
            _reservedPlaceSystem = reservedPlaceSystem;
            _instantiator = instantiator;
            _uiService = uiService;
        }

        public void FreePlace(string modelID, string placeNum, Action<bool> onComplete = null, string excludeGuid = "")
        {
            _onFreeCompleteAction = onComplete;
            if (!CanFreePlace(modelID, placeNum, out _overlapped, excludeGuid))
            {
                Failed(_overlapped);
                return;
            }

            if (_overlapped.Any())
            {
                var builder = new StringBuilder();
                var localizedMsg = LocalizationManager.Localize(_warningNeedDeleteTextKey);
                var separateSybol = ',';
                var endSybol = '.';
                builder.Append(localizedMsg + " : ");
                for (var i = 0; i < _overlapped.Count; i++)
                {
                    var overlapGuid = _overlapped[i];
                    var dto = _buildingDtoStorage.Get(overlapGuid);

                    builder.Append(LocalizationManager.Localize(_buildingsNameTextKey + dto.ModelID));
                    builder.Append(i + 1 < _overlapped.Count ? separateSybol : endSybol);
                }

                PrepareWindowYesNo(builder.ToString());
            }
            else
            {
                InternalFreePlace(false);
            }
        }

        /// <summary>
        /// Принудительное освобожение места
        /// </summary>
        public void FreePlaceInternal(string modelID, string placeNum, string excludeGuid = "")
        {
            CanFreePlace(modelID, placeNum, out _overlapped, excludeGuid);
            InternalFreePlace(false);
        }

        public bool CanFreePlace(string modelID, string placeNum, out List<string> neighbours, string excludeGuid = "")
        {
            if (!_placeIdStorage.HasEntity(placeNum))
            {
                throw new ArgumentException($"{this} Place with [ placeNum : {placeNum}] , does not exist");
            }

            var place = _placeIdStorage.Get(placeNum).GetPlace(modelID);
            var buildingModel = _modelStorage.Get(modelID);
            neighbours = new List<string>();

            // проверка по месту назначения
            if (_reservedPlaceSystem.HasEntity(placeNum))
            {
                var reservedGuid = _reservedPlaceSystem.GetPlaceOccupant(placeNum);
                if (excludeGuid != reservedGuid)
                {
                    if (!HasDeleteParam(reservedGuid))
                    {
                        neighbours.Add(reservedGuid);
                        return false;
                    }

                    neighbours.Add(reservedGuid);
                }
            }

            // Если объект не может перекрываться, проверим наличие перекрытий
            if (!buildingModel.CanOverlap)
            {
                var allocHits = new RaycastHit2D[6];
                foreach (var point in place.BoundPoints)
                {
                    var size = Physics2D.RaycastNonAlloc(point, Vector2.zero, allocHits);
                    if (size == 0) continue;
                    for (var i = 0; i < size; i++)
                    {
                        var hit = allocHits[i];
                        if (!hit || !hit.collider) continue;
                        if (!hit.collider.TryGetComponent(out BuildingSceneObject view)) continue;
                        if (view.Id == excludeGuid || neighbours.Contains(view.Id)) continue;

                        if (!HasDeleteParam(view.Id))
                        {
                            neighbours.Clear();
                            neighbours.Add(view.Id);
                            return false;
                        }

                        var overlapModel = _modelStorage.Get(_buildingDtoStorage.Get(view.Id).ModelID);
                        if (!overlapModel.CanOverlap)
                        {
                            neighbours.Add(view.Id);
                        }
                    }
                }
            }

            return true;
        }

        private void PrepareWindowYesNo(string message)
        {
            var window = _uiService.Show<UIYesNoWindow>();
            window.SetText(message);
            window.CloseEvent += OnCloseYesNo;
            window.NoEvent += OnCloseYesNo;
            window.YesEvent += OnYes;
        }

        private void PrepareWindowMessageBox(string message)
        {
            var window = _uiService.Show<UIMessageBoxWindow>();
            window.AcceptEvent += OnAcceptErrorMesage;
            window.SetMessageText(message);
        }

        private void FinalizeMessageBox()
        {
            _uiService.Hide<UIMessageBoxWindow>();
        }

        private void FinalizeYesNo()
        {
            _uiService.Hide<UIYesNoWindow>();
        }

        private void OnAcceptErrorMesage(object sender, EventArgs e)
        {
            FinalizeMessageBox();
            _onFreeCompleteAction?.Invoke(false);
            Clear();
        }

        private void OnCloseYesNo(object sender, EventArgs e)
        {
            FinalizeYesNo();
            _onFreeCompleteAction?.Invoke(false);
            Clear();
        }

        private void OnYes(object sender, EventArgs e)
        {
            FinalizeYesNo();
            InternalFreePlace(false);
        }

        private bool HasDeleteParam(string guid)
        {
            var dto = _buildingDtoStorage.Get(guid);
            var paramModel = _buildingParamsModelStorage.Get(dto.ModelID);
            return paramModel.Params.Any(x => x == _deleteParam);
        }

        private void Failed(IReadOnlyList<string> overalps)
        {
            var message = LocalizationManager.Localize(_errorCannotDeleteTextKey) + " : ";
            const char separateSimbol = ',';
            const char endSimbol = '.';
            for (var i = 0; i < overalps.Count; i++)
            {
                var guid = overalps[i];
                var dto = _buildingDtoStorage.Get(guid);
                var name = LocalizationManager.Localize("BuildingsName_" + dto.ModelID);
                message += name + (i + 1 == overalps.Count ? endSimbol : separateSimbol);
            }

            PrepareWindowMessageBox(message);
        }

        private void InternalFreePlace(bool notify = true)
        {
            foreach (var guid in _overlapped)
            {
                var deleteProtocol = new DeleteBuildingProtocol(guid, notify);
                var deleteCommand = _instantiator.Instantiate<DeleteBuildingCommand>();
                deleteCommand.Execute(deleteProtocol);
            }

            _onFreeCompleteAction?.Invoke(true);
            Clear();
        }

        private void Clear()
        {
            _onFreeCompleteAction = null;
            _overlapped = null;
        }
    }
}