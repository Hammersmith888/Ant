using BugsFarm.Services.SimpleLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UILoaderProgressBar : MonoBehaviour
    {
        [SerializeField] private Image _progressFill;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _loadingText;

        private const string _loadingTextKey = "Loading";
        private const char _progressSimbol = '%';

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SetProgress(float progress01)
        {
            _progressFill.fillAmount = progress01;
            _progressText.text = Mathf.RoundToInt(progress01 * 100).ToString() + _progressSimbol;
        }

        public void Show()
        {
            _progressFill.fillAmount = 0;
            _loadingText.text = LocalizationManager.Localize(_loadingTextKey);
            _progressText.text = "0" + _progressSimbol;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}