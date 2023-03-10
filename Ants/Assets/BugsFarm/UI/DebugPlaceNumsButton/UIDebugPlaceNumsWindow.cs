using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIDebugPlaceNumsWindow : UISimpleWindow
    {
        public event UnityAction OnClick
        {
            add => _button.onClick.AddListener(value);
            remove => _button.onClick.RemoveListener(value);
        }

        public Text Prefab => _prefab;
        public Color OriginalColor { get; private set; }

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _button;
        [SerializeField] private Text _prefab;

        private void Awake()
        {
            base.Awake();
            OriginalColor = _text.color;
        }

        public void SetTextColor(Color color)
        {
            if (!_text)
            {
                Debug.LogError($"{this} : Text doest not exist");
                return;
            }

            _text.color = color;
        }
    }
}