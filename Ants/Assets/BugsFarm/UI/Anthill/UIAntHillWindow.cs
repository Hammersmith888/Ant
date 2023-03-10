using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
	public class UIAntHillWindow : UISimpleWindow
	{
		[SerializeField] private Button _taskListButton;
		[SerializeField] private Button _upgradeButton;
		[SerializeField] private Button[] _closeButtons;
		
		[SerializeField] private Image _iconImage;
		[SerializeField] private SlicedFilledImage _progressFill;
		[SerializeField] private TextMeshProUGUI _headerText;
		[SerializeField] private TextMeshProUGUI _discriptionText;
		[SerializeField] private TextMeshProUGUI _bugsAmountInfoText;
		[SerializeField] private TextMeshProUGUI _daysLivedInfoText;
		[SerializeField] private TextMeshProUGUI _mealLeftInfoText;
		[SerializeField] private TextMeshProUGUI _progressText;
		[SerializeField] private TextMeshProUGUI _upgradeButtonText;

		[SerializeField] private CanvasGroup _upgradeCanvasCanvasGroup;
		
		private const string _bugsAmountInfoTextKey = "UIAnthill_Info_Bugs_Amount";
		private const string _daysLivedInfoTextKey = "UIAnthill_Info_Days_Lived";
		private const string _mealLeftInfoTextKey = "UIAnthill_Info_Meal_Left";
		private const string _upgradeButtonTextKey = "UIAnthill_UpgradeButton";
		
		public event EventHandler TaskListEvent;
		public event EventHandler UpgradeEvent;
		public event EventHandler PrisonEvent; // TODO : remove develop tests

		public override void Show()
		{
			base.Show();
			
			foreach (var closeButton in _closeButtons)
			{
				closeButton.onClick.AddListener(Close);
			}
			_taskListButton.onClick.AddListener(TaskListEventHandler);
			_upgradeButton.onClick.AddListener(UpgradeEventHandler);
			
			_upgradeButtonText.text = LocalizationManager.Localize(_upgradeButtonTextKey);
		}
		
		public override void Hide()
		{
			base.Hide();
			
			foreach (var closeButton in _closeButtons)
			{
				closeButton.onClick.RemoveListener(Close);
			}
			_taskListButton.onClick.RemoveListener(TaskListEventHandler);
			_upgradeButton.onClick.RemoveListener(UpgradeEventHandler);
			
			TaskListEvent = null;
			UpgradeEvent = null;
			PrisonEvent = null;
		}
		
		public void SetUnitsCount(string unitsCount)
		{
			_bugsAmountInfoText.text = string.Format(LocalizationManager.Localize(_bugsAmountInfoTextKey), unitsCount);
		}

		public void SetDaysPassedCount(string daysPassedCount)
		{
			_daysLivedInfoText.text = string.Format(LocalizationManager.Localize(_daysLivedInfoTextKey), daysPassedCount);
		}

		public void SetMealLeftCount(string mealLeftDays)
		{
			_mealLeftInfoText.text = string.Format(LocalizationManager.Localize(_mealLeftInfoTextKey), mealLeftDays);
		}

		public void SetUpgradeIcon(bool isActive)
		{
			_upgradeCanvasCanvasGroup.alpha = isActive ? 1.0f : 0.5f;
			_upgradeCanvasCanvasGroup.interactable = isActive;
		}
		
		public void SetIcon(Sprite icon)
		{
			if (!_iconImage)
			{
				Debug.LogError($"{this} : Image _iconImage is missing");
				return;
			}

			_iconImage.sprite = icon;
		}
		
		
		
		public void SetProgress(float progress01)
		{
			if (!_progressFill)
			{
				Debug.LogError($"{this} : Image _progressFill is missing");
				return;
			}
			
			_progressFill.fillAmount = progress01;
		}
		
		public void SetProgressText(string text)
		{
			if (!_progressText)
			{
				Debug.LogError($"{this} : TextMeshProUGUI _progressText is missing");
				return;
			}

			_progressText.text = text;
		}
		
		public void SetHeaderText(string text)
		{
			_headerText.text = text;
		}
		
		public void SetDescriptionText(string text)
		{
			_discriptionText.text = text;
		}
		
		private void TaskListEventHandler()
		{
			TaskListEvent?.Invoke(this, EventArgs.Empty);
		}
		
		private void UpgradeEventHandler()
		{
			UpgradeEvent?.Invoke(this, EventArgs.Empty);
		}
	}
}



