using BugsFarm.BuildingSystem;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using UnityEngine;

namespace BugsFarm.ReloadSystem
{
    public class ReloadGameObjectsDestroyer
    {
        private readonly BuildingsContainer _buildingsContainer;
        private readonly UnitsContainer _unitsContainer;
        private readonly UIWorldRoot _uiWorldRoot;

        public ReloadGameObjectsDestroyer(BuildingsContainer buildingsContainer,
                                          UnitsContainer unitsContainer,
                                          UIWorldRoot uiWorldRoot)
        {
            _buildingsContainer = buildingsContainer;
            _unitsContainer = unitsContainer;
            _uiWorldRoot = uiWorldRoot;
        }

        public void DestroyObjectsOnScene()
        {
            GameObject.Destroy(_uiWorldRoot.gameObject);
            GameObject.Destroy(_buildingsContainer.gameObject);
            GameObject.Destroy(_unitsContainer.gameObject);
        }
    }
}