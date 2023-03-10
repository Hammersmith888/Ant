using TMPro;
using UnityEngine;

namespace BugsFarm.UI
{
    public class UIBugInfoStatCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _statText;

        public void SetText(string value)
        {
            _statText.text = value;
        }
    }
}