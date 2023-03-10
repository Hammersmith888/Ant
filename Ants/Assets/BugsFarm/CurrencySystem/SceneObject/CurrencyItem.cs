using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.CurrencySystem
{
    public class CurrencyItem : MonoBehaviour
    {
        public Vector3 Position => _currencyImage.transform.position;
        public string ID { get; private set; } = "-1";

        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _currencyImage;

        public void SetID(string id)
        {
            ID = id;
        }
        
        public void SetText(string text)
        {
            if(!_countText) return;
            
            _countText.text = text;
        }   

        public void SetTextColor(Color color)
        {
            if(!_countText) return;
            
            _countText.color = color;
        }        

        public void SetSprite(Sprite sprite)
        {
            if(!_currencyImage) return;
            
            _currencyImage.sprite = sprite;
        }

        public void SetActiveImage(bool active)
        {
            if(!_currencyImage) return;
            _currencyImage.gameObject.SetActive(active);
        }
    }
}