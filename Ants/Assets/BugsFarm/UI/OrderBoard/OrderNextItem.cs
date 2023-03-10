using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class OrderNextItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _description;
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
        
        public void SetDescriptionText(string text)
        {
            if(!_description) return;
            _description.text = text;
        }
        
        public void ActiveDescriptionText(bool active)
        {
            if(!_description) return;
            _description.gameObject.SetActive(active);
        }
    }
}