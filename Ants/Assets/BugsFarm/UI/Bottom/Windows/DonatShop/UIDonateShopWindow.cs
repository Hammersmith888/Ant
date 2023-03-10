using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class UIDonateShopWindow : UISimpleWindow
    {        
        [Header("Texts")] 
        [SerializeField] private TextMeshProUGUI _windowHeaderText;
    
        [Header("Prefabs")] 
        [SerializeField] private GameObject _itemCellPrefab; 
        [SerializeField] private GameObject _offerItemPrefab; 
        [SerializeField] private GameObject _itemsContainerPrefab;
        
        [SerializeField] private RectTransform _contentContainer;
    
        private const string _windowHeaderTextKey = "UIDonateShop_Header";
        
        public GameObject ItemCellPrefab => _itemCellPrefab;
        public GameObject OfferItemPrefab => _offerItemPrefab;
        public GameObject ItemsContainerPrefab => _itemsContainerPrefab;
        public RectTransform Content => _contentContainer;
    
        public override void Show()
        {
            base.Show();
        
            _windowHeaderText.text = LocalizationManager.Localize(_windowHeaderTextKey);
        }
    }
}
