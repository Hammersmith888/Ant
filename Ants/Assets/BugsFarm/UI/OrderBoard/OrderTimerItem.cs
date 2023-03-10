using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class OrderTimerItem : MonoBehaviour
    {
        [SerializeField] private GameObject _phantomButton;
        [SerializeField] private TextMeshProUGUI _timer;

        public void SetTimerText(string text)
        {
            if (!_timer)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _timer is missing");
                return;
            }

            _timer.text = text;
        }
        
        public void ActiveTimer(bool active)
        {
            gameObject.SetActive(active);
        }
        
        public void ActivePhantomButton(bool active)
        {
            if(!_phantomButton) return;
            _phantomButton.SetActive(active);
        }
    }
}