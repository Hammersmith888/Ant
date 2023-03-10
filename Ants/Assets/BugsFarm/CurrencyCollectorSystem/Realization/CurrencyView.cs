using TMPro;
using UnityEngine;

namespace BugsFarm.CurrencyCollectorSystem
{
    public class CurrencyView : MonoBehaviour, ICurrencyView
    {
        public string CurrencyID => _currencyID.ToString();
        public Transform Target => _target;
        [SerializeField] private int _currencyID = -1;
        [SerializeField] private Transform _target;
        [SerializeField] private TextMeshProUGUI _curencyText;

        private void OnValidate()
        {
            if (!_target)
            {
                _target = transform;
            }
        }

        public void SetCurrencyText(string text)
        {
            if (!_curencyText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _curencyText is missed");
                return;
            }
            _curencyText.text = text;
        }
    }
}