using BugsFarm.Views.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.Views.Fight.Ui
{
    public class BoosterButtonView : AUiView
    {
        [SerializeField] private Image enabledImage;
        [SerializeField] private Button button;

        public Image EnabledImage => enabledImage;
        public Button Button => button;
    }
}