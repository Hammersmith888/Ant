using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIFarmShopWindow : UISimpleWindow
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private UIFarmShopCellView _cellViewPrefab;
        [SerializeField] private UIFarmShopTabElement[] _tabs;
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private TextMeshProUGUI _headerText;
        
        private const string _headerTextKey = "UIFarmShop_Header";
        
        public RectTransform Content => _content;
        public UIFarmShopCellView CellViewPrefab => _cellViewPrefab;
        public UIFarmShopTabElement[] ShopTabs => _tabs;

        public override void Show()
        {
            base.Show();

            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            
            _headerText.text = LocalizationManager.Localize(_headerTextKey);
        }

        public override void Hide()
        {
            base.Hide();

            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }
    }
}