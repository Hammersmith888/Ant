using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISwitchButton : ImageSwitcher
    {
        [SerializeField] private Button _switchButton = null;
        public override void Init()
        {
            _switchButton.onClick.AddListener(() => Switch());
        }
    }
}
