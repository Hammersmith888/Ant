using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class UIDevOpsWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _discriptionText;
		
        private const string _discriptionTextKey = "UIDevOps_Discription";
        
        public override void Show()
        {
            base.Show();
			
            _discriptionText.text = LocalizationManager.Localize(_discriptionTextKey);
        }
    }
}
