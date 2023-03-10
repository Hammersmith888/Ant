using System;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIQuestGiftPoint : MonoBehaviour
    {
        public event Action OnCollectButtonClicked;

        [SerializeField] private Button chestButton;
        [SerializeField] private Image giftImage;
        [SerializeField] private Image chestImage;

        public void Initialize()
        {
            chestButton.onClick.AddListener(NotifyButtonClicked);
        }

        private void NotifyButtonClicked()
        {
            OnCollectButtonClicked?.Invoke();
        }

        public void SwitchToGiftState()
        {
            //giftImage.sprite = giftIcon;
            giftImage.gameObject.SetActive(true);
            chestImage.gameObject.SetActive(false);
        }

        public void SwitchToChestState()
        {
            giftImage.gameObject.SetActive(false);
            chestImage.gameObject.SetActive(true);
        }

        public void Dispose()
        {
            chestButton.onClick.RemoveListener(NotifyButtonClicked);
        }

        public void SetActiveIcon(bool isActive)
        {
            chestImage.gameObject.SetActive(isActive);
            giftImage.gameObject.SetActive(isActive);
        }
    }
}