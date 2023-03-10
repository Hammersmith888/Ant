using System;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
	public class UICommunityWindow : UISimpleWindow
	{
		[Header("Base buttons")]
		[SerializeField] private Button[] _closeButtons;
		[SerializeField] private Button _settingsButton;
		[SerializeField] private Button _leadBordButton;
		[SerializeField] private Button _faceBookButton;
		[SerializeField] private Button _arrowLeftButton;
		[SerializeField] private Button _arrowRightButton;

		[Header("Social")]
		[SerializeField] private UIEmailComponent _emailComponent;
		[SerializeField] private UIListComponent _friendList;

		[Header("Avatar")] 
		[SerializeField] private Image _avatarImage;
		[SerializeField] private TextMeshProUGUI _levelText;
		[SerializeField] private TextMeshProUGUI _nickNameText;
		
		public event EventHandler SettingsEvent;
		public event EventHandler LeadbordEvent;
		public event EventHandler FacebookEvent;
		public event EventHandler ArrowLeftEvent;
		public event EventHandler ArrowRightEvent;
		
		public override void Show()
		{
			foreach (var closeButton in _closeButtons)
			{
				closeButton.onClick.AddListener(Close);
			}
			_settingsButton.onClick.AddListener(SettingsEventHandler);
			_leadBordButton.onClick.AddListener(LeadbordEventHandler);
			_faceBookButton.onClick.AddListener(FacebookEventHandler);
			_arrowLeftButton.onClick.AddListener(ArrowLeftEventHandler);
			_arrowRightButton.onClick.AddListener(ArrowRightEventHandler);
			base.Show();
		}

		public override void Hide()
		{
			foreach (var closeButton in _closeButtons)
			{
				closeButton.onClick.RemoveListener(Close);
			}
			_settingsButton.onClick.RemoveListener(SettingsEventHandler);
			_leadBordButton.onClick.RemoveListener(LeadbordEventHandler);
			_faceBookButton.onClick.RemoveListener(FacebookEventHandler);
			_arrowLeftButton.onClick.RemoveListener(ArrowLeftEventHandler);
			_arrowRightButton.onClick.RemoveListener(ArrowRightEventHandler);
			base.Hide();
		}
		      
		public void SetNickname(string nickname)
		{
			_nickNameText.text = nickname;
		}
		      
		public void SetLevel(string level)
		{
			_levelText.text = level;
		}
		      
		public void SetAvatar(Sprite avatar)
		{
			_avatarImage.sprite = avatar;
		}

		private void SettingsEventHandler()
		{
			SettingsEvent?.Invoke(this, EventArgs.Empty);
		}

		private void LeadbordEventHandler()
		{
			LeadbordEvent?.Invoke(this, EventArgs.Empty);
		}

		private void FacebookEventHandler()
		{
			FacebookEvent?.Invoke(this, EventArgs.Empty);
		}

		private void ArrowLeftEventHandler()
		{
			ArrowLeftEvent?.Invoke(this, EventArgs.Empty);
		}

		private void ArrowRightEventHandler()
		{
			ArrowRightEvent?.Invoke(this, EventArgs.Empty);
		}
	}
}

