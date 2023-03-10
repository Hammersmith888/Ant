using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class ParamItem : MonoBehaviour
    {
        [SerializeField] private Image _paramIconPlaceHolder;
        [SerializeField] private SlicedFilledImage _paramProgress;
        [SerializeField] private TextMeshProUGUI _progressText;

        public void SetProgress(float progress01)
        {
            _paramProgress.fillAmount = progress01;
        }

        public void SetProgressText(string value)
        {
            _progressText.text = value;
        }

        public void SetIcon(Sprite value)
        {
            _paramIconPlaceHolder.sprite = value;
        }
    }
}