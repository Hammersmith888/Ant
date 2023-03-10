using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class UIDonatItemCellsContainer : MonoBehaviour
    {
        public string Id { get; private set; }

        public RectTransform Content => _contentContainer;
    
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private TextMeshProUGUI _headerLabel;

        public void SetHeaderText(string text)
        {
            if (!_headerLabel)
            {
                Debug.LogError($" TextMeshProUGUI _headerLabel is missing ");
                return;
            }

            _headerLabel.text = text;
        }
        
        public void SetItemId(string id)
        {
            Id = id;
        }
    }
}
