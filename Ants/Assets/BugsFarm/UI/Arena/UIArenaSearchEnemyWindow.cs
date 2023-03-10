using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI.Arena
{
    public class UIArenaSearchEnemyWindow : UISimpleWindow
    {
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _searchingLabel;
        [SerializeField] private TextMeshProUGUI _cancelButtonLabel;
        
        private const string _headerLabelKey = "UIArenaSearchEnemy_Header";
        private const string _searchingLabelKey = "UIArenaSearchEnemy_SearchEnemy";
        private const string _cancelButtonLabelKey = "UIArenaSearchEnemy_CancelButton";
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            
            _headerLabel.text = LocalizationManager.Localize(_headerLabelKey);
            _searchingLabel.text = LocalizationManager.Localize(_searchingLabelKey);
            _cancelButtonLabel.text = LocalizationManager.Localize(_cancelButtonLabelKey);
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
