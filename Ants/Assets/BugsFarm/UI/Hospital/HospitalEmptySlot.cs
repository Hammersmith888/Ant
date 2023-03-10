using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class HospitalEmptySlot : MonoBehaviour
    {
        public bool Required => _required.activeSelf;
        [SerializeField] private TextMeshProUGUI _requiredText;
        [SerializeField] private GameObject _required;
        [SerializeField] private GameObject _empty;

        public void SetEmptyActive(bool value)
        {
            _empty.SetActive(value);
        }
        
        public void SetRequiredActive(bool value)
        {
            _required.SetActive(value);
        }

        public void SetRequiredText(string value)
        {
            _requiredText.text = value;
        }
    }
}