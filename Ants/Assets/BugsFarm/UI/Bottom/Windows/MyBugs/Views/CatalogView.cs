using UnityEngine;

namespace BugsFarm.UI
{
    public class CatalogView : BaseView
    {
        public CatalogItem PrefabItem => _prefabItem;
        public RectTransform ItemsContainer => _itemsContainer;

        [SerializeField] private CatalogItem _prefabItem;
        [SerializeField] private RectTransform _itemsContainer;
    }
}