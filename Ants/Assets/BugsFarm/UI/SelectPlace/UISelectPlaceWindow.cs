using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISelectPlaceWindow : UISimpleWindow
    {
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;
        
        private const string _cancelButtonTextKey = "UISelectPlace_Cancel";

        public override void Show()
        {
            base.Show();
            
            _cancelButton.onClick.AddListener(Close);
            
            _cancelButtonText.text = LocalizationManager.Localize(_cancelButtonTextKey);
        }
        
        public override void Hide()
        {
            base.Hide();
            
            _cancelButton.onClick.RemoveListener(Close);
        }
    }
}