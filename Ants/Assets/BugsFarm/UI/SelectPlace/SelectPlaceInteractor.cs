using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.UIService;

namespace BugsFarm.UI
{
    public class SelectPlaceInteractor
    {
        private readonly IUIService _uiService;
        private readonly PlaceIdStorage _placeIdStorage;
        private IEnumerable<PlaceID> _placeIds;

        public SelectPlaceInteractor(IUIService uiService, PlaceIdStorage placeIdStorage)
        {
            _uiService = uiService;
            _placeIdStorage = placeIdStorage;
        }

        /// <summary>
        /// callback with selected placeNum
        /// </summary>
        public void SelectPlace(string modelID, Action<string> onSelected = null)
        {
           var window = _uiService.Show<UISelectPlaceWindow>();
           window.CloseEvent += OnClose;
           
           _placeIds = _placeIdStorage.Get().Where(place => place.HasPlace(modelID)).ToArray();
            foreach (var placeID in _placeIds)
            {
                placeID.Activate(modelID, placeNum =>
                {
                    onSelected?.Invoke(placeNum);
                    FinalizeInteractor();
                });
            }
        }

        private void FinalizeInteractor()
        {
            _uiService.Hide<UISelectPlaceWindow>();
            if (_placeIds != null)
            {
                foreach (var placeID in _placeIds)
                {
                    placeID.DeActivate();
                }
                _placeIds = null;
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            FinalizeInteractor();
        }
    }
}