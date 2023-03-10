using BugsFarm.Services.UIService;
using UnityEngine;

namespace BugsFarm.UI
{
    public class UILoader : UISimpleWindow
    {
        [SerializeField] private UILoaderProgressBar _progressBar;

        public void ShowProgress(bool reset = true)
        {
            if (reset)
            {
                _progressBar.SetProgress(0);
            }
            _progressBar.Show();
        }

        public void HideProgress()
        {
            _progressBar.Hide();
        }

        public void SetProgress(float progress01)
        {
            _progressBar.SetProgress(progress01);
        }
    }
}