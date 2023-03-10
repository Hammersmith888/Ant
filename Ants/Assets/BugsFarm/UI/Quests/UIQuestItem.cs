using System;
using System.Threading.Tasks;
using BugsFarm.Services.SimpleLocalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIQuestItem : MonoBehaviour
    {
        public event Action<UIQuestItem> OnButtonClicked;
        public string Guid => _guid;

        public Vector3 ButtonPosition => buttonPosition.transform.position;
        
        [SerializeField] private Image questImage;
        [SerializeField] private Image progressBarImage;
        
        [SerializeField] private TextMeshProUGUI questTitleTMP;
        [SerializeField] private TextMeshProUGUI questDescriptionTMP;
        [SerializeField] private TextMeshProUGUI questProgressTMP;
        [SerializeField] private TextMeshProUGUI buttonLabelTMP;
        [SerializeField] private TextMeshProUGUI activeButtonRewardTMP;
        [SerializeField] private TextMeshProUGUI inactiveButtonRewardTMP;
        [SerializeField] private TextMeshProUGUI timerTMP;

        [SerializeField] private GameObject timerContainer;
        [SerializeField] private GameObject inactiveButtonGameObject;
        [SerializeField] private Button activeButtonGameObject;
        
        [SerializeField] private RectTransform buttonPosition;
        [SerializeField] private RectTransform slotRectTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        private Sequence _sequence;
        private const string _questSlotTitleKey = "UIQuest_SlotTitle";
        private const string _questButtonLabelKey = "UIQuest_CollectReward";
        private const string _questButtonRewardKey = "UIQuest_RewardButtonTitle";
        private string _guid;

        public void Initialize(string guid)
        {
            _guid = guid;
            questTitleTMP.text = LocalizationManager.Localize(_questSlotTitleKey);
            activeButtonGameObject.onClick.AddListener(NotifyAboutClickedButton);
        }
        public void SetActiveTimer(bool isActive)
        {
            timerContainer.SetActive(isActive);
        }
        private void NotifyAboutClickedButton()
        {
            OnButtonClicked?.Invoke(this);
        }

        public void SetDescriptionText(string text)
        {
            questDescriptionTMP.text = text;
        }

        public void ActivateRewardButton(bool toActivate)
        {
            inactiveButtonGameObject.SetActive(!toActivate);
            activeButtonGameObject.gameObject.SetActive(toActivate);
            
            buttonLabelTMP.text = toActivate ?
                buttonLabelTMP.text = LocalizationManager.Localize(_questButtonLabelKey) :
                buttonLabelTMP.text = LocalizationManager.Localize(_questButtonRewardKey);
        }
        
        public void SetQuestIcon(Sprite icon)
        {
            questImage.sprite = icon;
        }

        public void SetProgressFill(float value01)
        {
            progressBarImage.fillAmount = value01;
        }

        public void SetProgressData(string progressData)
        {
            questProgressTMP.text = progressData;
        }

        public void SetProgressColor(Color color)
        {
            questProgressTMP.color = color;
        }
        
        public void SetRewardValue(string value)
        {
            inactiveButtonRewardTMP.text = value;
            activeButtonRewardTMP.text = value;
        }

        public void CompleteQuest(Action onComplete)
        {
            if(_sequence != null)
                _sequence.Kill();
            _sequence = DOTween.Sequence();
            _sequence.Append(slotRectTransform.DOAnchorPosX(700.0f, 1.0f))
                .SetEase(Ease.InOutSine)
                .Join(canvasGroup.DOFade(0, 1.0f))
                .AppendCallback(() => onComplete());
        }

        public void UpdateTimeLeft(string leftTimeInMinutes)
        {
            timerTMP.text = leftTimeInMinutes.ToString();
        }
    }
}