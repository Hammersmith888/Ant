using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIEmailComponent : MonoBehaviour
    {
        public event UnityAction OnClick
        {
            add => _emailButton.onClick.AddListener(value);
            remove => _emailButton.onClick.RemoveListener(value);
        }

        [SerializeField] private TextMeshProUGUI _messageCounter;
        [SerializeField] private GameObject _coutnerContainer;
        [SerializeField] private Button _emailButton;
		
        public void ChangeCountText(string countText)
        {
            _messageCounter.text = countText;
        }

        public void ActivateCounter(bool active)
        {
            _coutnerContainer.SetActive(active);
        }
    }
}