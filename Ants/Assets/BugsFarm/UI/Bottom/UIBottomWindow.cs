using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    [RequireComponent(typeof(UIBaseAnimation))]
    public class UIBottomWindow : UISimpleWindow
    {
        [SerializeField] private Button _shopDonatButton;
        [SerializeField] private Button _shopFarmButton;
        [SerializeField] private Button _shopBugsButton;
        [SerializeField] private Button _battleButton;
        
        [SerializeField] private CanvasGroup _donateGroupe;
        [SerializeField] private CanvasGroup _farmGroupe;
        [SerializeField] private CanvasGroup _myBugsGroupe;
        [SerializeField] private CanvasGroup _battleGroupe;

        [SerializeField] private TextMeshProUGUI _shopText;
        [SerializeField] private TextMeshProUGUI _farmText;
        [SerializeField] private TextMeshProUGUI _bugsText;
        [SerializeField] private TextMeshProUGUI _battleText;

        private const string _shopTextKey = "UIBottom_Shop";
        private const string _farmTextKey = "UIBottom_Farm";
        private const string _bugsTextKey = "UIBottom_MyBugs";
        private const string _battleTextKey = "UIBottom_Battle";
        
        public event EventHandler<string> ButtonClickedEvent;


        public override void Show()
        {
            base.Show();
            
            _shopDonatButton.onClick.AddListener(DonatShopButtonEventHandler);
            _shopFarmButton.onClick.AddListener(FarmShopButtonEventHandler);
            _shopBugsButton.onClick.AddListener(BugsShopButtonEventHandler);
            _battleButton.onClick.AddListener(BattleButtonEventHandler);

            _shopText.text = LocalizationManager.Localize(_shopTextKey);
            _farmText.text = LocalizationManager.Localize(_farmTextKey);
            _bugsText.text = LocalizationManager.Localize(_bugsTextKey);
            _battleText.text = LocalizationManager.Localize(_battleTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            _shopDonatButton.onClick.RemoveListener(DonatShopButtonEventHandler);
            _shopFarmButton.onClick.RemoveListener(FarmShopButtonEventHandler);
            _shopBugsButton.onClick.RemoveListener(BugsShopButtonEventHandler);
            _battleButton.onClick.RemoveListener(BattleButtonEventHandler);
            
            ButtonClickedEvent = null;
        }
        
        public void SetInteractable(string buttonId, bool interactable)
        {
            Button button;
            switch (buttonId)
            {
                case "DonatShop": button = _shopDonatButton;
                    _donateGroupe.alpha = interactable ? 1 : 0.5f; break;
                case "FarmShop": button = _shopFarmButton; 
                    _farmGroupe.alpha = interactable ? 1 : 0.5f; break;
                case "MyBugs": button = _shopBugsButton; 
                    _myBugsGroupe.alpha = interactable ? 1 : 0.5f; break;
                case "Battle": button = _battleButton;
                    _battleGroupe.alpha = interactable ? 1 : 0.5f; break;
                default: throw new ArgumentException($"Button with id : {buttonId}, does not exist");
            }

            button.interactable = interactable;
        }

        private void DonatShopButtonEventHandler()
        {
            ButtonClickedEvent?.Invoke(this, "DonatShop");
        }

        private void FarmShopButtonEventHandler()
        {
            ButtonClickedEvent?.Invoke(this, "FarmShop");
        }

        private void BugsShopButtonEventHandler()
        {
            ButtonClickedEvent?.Invoke(this, "MyBugs");
        }

        private void BattleButtonEventHandler()
        {
            ButtonClickedEvent?.Invoke(this, "Battle");
        }
    }
}