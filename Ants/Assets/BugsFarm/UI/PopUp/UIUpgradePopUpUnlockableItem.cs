using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIUpgradePopUpUnlockableItem : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemTitle;

        public void SetIcon(Sprite sprite)
        {
            itemImage.sprite = sprite;
        }

        public void SetItemTitle(string text)
        {
            itemTitle.text = text;
        }
    }
}