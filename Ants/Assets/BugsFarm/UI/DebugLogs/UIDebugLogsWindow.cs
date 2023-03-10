using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIDebugLogsWindow : UISimpleWindow
    {
        public event UnityAction OnClick
        {
            add => _button.onClick.AddListener(value);
            remove => _button.onClick.RemoveListener(value);
        }

        public GameObject Prefab => _prefab;
        public Color OriginalColor { get; private set; }
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private GameObject _prefab;

        protected override void Awake()
        {
            base.Awake();
            OriginalColor = _text.color;
        }

        public void SetTextColor(Color color)
        {
            if (!_text)
            {
                Debug.LogError($"{this} : Text does not exist");
                return;
            }

            _text.color = color;
        }
    }
}