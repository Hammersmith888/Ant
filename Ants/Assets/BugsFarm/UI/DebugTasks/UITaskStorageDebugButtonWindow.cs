using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UITaskStorageDebugButtonWindow : UISimpleWindow
    {
        public event UnityAction OnClick
        {
            add => _button.onClick.AddListener(value);
            remove => _button.onClick.RemoveListener(value);
        }

        [SerializeField] private Button _button;
    }
}