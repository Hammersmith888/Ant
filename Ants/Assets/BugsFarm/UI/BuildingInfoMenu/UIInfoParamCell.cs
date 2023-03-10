using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIInfoParamCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _paramNameLabel;
        [SerializeField] private TextMeshProUGUI _valueLabel;
        [SerializeField] private TextMeshProUGUI _upgradeValueLabel;
        [SerializeField] private Image _paramIcon;
        
        public void SetName(string nameValue)
        {
            _paramNameLabel.text = nameValue;
        }

        public void SetValue(string value)
        {
            _valueLabel.text = value;
        }

        public void SetUpgradeValue(string value)
        {
            _upgradeValueLabel.text = value;
        }

        public void SetUpgradeActive(bool active)
        {
            _upgradeValueLabel.gameObject.SetActive(active);
        }

        public void SetIconActive(bool active)
        {
            _paramIcon.gameObject.SetActive(active);
        }
        
        public void SetIcon(Sprite sprite)
        {
            if(sprite == null) return;
            _paramIcon.sprite = sprite;
        }
    }
}