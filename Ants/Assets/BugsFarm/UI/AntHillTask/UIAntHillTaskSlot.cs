using System;
using System.Collections;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIAntHillTaskSlot : MonoBehaviour
    {
        
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI taskTitle;
        [SerializeField] private TextMeshProUGUI progressTitle;
        [SerializeField] private TextMeshProUGUI taskCompletedTitle;
        [SerializeField] private SlicedFilledImage slicedFilledImage;
        [SerializeField] private RectTransform progressRectTransform;

        public void SetIcon(Sprite icon)
        {
            iconImage.sprite = icon;
        }

        public void SetProgressText(string text)
        {
            progressTitle.text = text;
        }
        
        public void SetTitle(string text)
        {
            taskTitle.text = text;
        }

        public void SetProgress(float value)
        {
            slicedFilledImage.fillAmount = value;
        }

        public void SetCompletedTaskText(string text)
        {
            taskCompletedTitle.gameObject.SetActive(true);
            progressRectTransform.gameObject.SetActive(false);
            taskCompletedTitle.text = text;
        }
        public void Dispose()
        {
            iconImage = null;
            taskCompletedTitle.gameObject.SetActive(false);
            progressRectTransform.gameObject.SetActive(true);
            taskTitle.text = String.Empty;
            slicedFilledImage.fillAmount = 1.0f;
        }
    }
}
