using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AnimationState = Spine.AnimationState;
using Object = System.Object;

namespace BugsFarm.UI
{
    public class UIUpgradePopUpWindow : UISimpleWindow
    {
        public UIUpgradePopUpUnlockableItem Prefab => prefab;
        public RectTransform ParentTransform => parentTransform;
        
        [SerializeField] private TextMeshProUGUI acceptTMP;
        [SerializeField] private TextMeshProUGUI congratulationsTMP;
        [SerializeField] private TextMeshProUGUI unlockedMessageTMP;
        [SerializeField] private TextMeshProUGUI starLevelTMP;
        [SerializeField] private TextMeshProUGUI newLevelTMP;
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform containerTransform;
        [SerializeField] private RectTransform dotsContainer;
        [SerializeField] private RectTransform scrollContainer;
        [SerializeField] private RectTransform parentTransform;
        [SerializeField] private UIUpgradePopUpUnlockableItem prefab;

        private const string _acceptLocalizationKey = "UIPopUp_Accept";
        private const string _newLevelLocalizationKey = "UIPopUp_NewLevel";
        private const string _unlockedItemsLocalizationKey = "UIPopUp_Unlocked_Message";
        private const string _congratulationsLocalizationKey = "UIPopUp_Congratulations";

        public override void Show()
        {
            base.Show();
            
            closeButton.onClick.AddListener(Close);
            canvasGroup.blocksRaycasts = true;
            skeletonGraphic.AnimationState.SetAnimation(0, "Great", false);
            acceptTMP.text = LocalizationManager.Localize(_acceptLocalizationKey);
            newLevelTMP.text = LocalizationManager.Localize(_newLevelLocalizationKey);
            unlockedMessageTMP.text = LocalizationManager.Localize(_unlockedItemsLocalizationKey);
        }

        public override void Hide()
        {
            base.Hide();
            canvasGroup.blocksRaycasts = false;
            closeButton.onClick.RemoveListener(Close);

        }

        public void SwitchToFullSize()
        {
            dotsContainer.gameObject.SetActive(true);
            containerTransform.sizeDelta = new Vector2(containerTransform.sizeDelta.x, 950.0f);
            unlockedMessageTMP.gameObject.SetActive(true);
            scrollContainer.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerTransform);
        }

        public void SwitchToShortSize()
        {
            dotsContainer.gameObject.SetActive(false);
            containerTransform.sizeDelta = new Vector2(containerTransform.sizeDelta.x, 500.0f);
            unlockedMessageTMP.gameObject.SetActive(false);
            scrollContainer.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerTransform);
        }
        
        public void SetLevel(string level)
        {
            congratulationsTMP.text = String.Format(LocalizationManager.Localize(_congratulationsLocalizationKey), level);
            starLevelTMP.text = level;
        }
    }
}
