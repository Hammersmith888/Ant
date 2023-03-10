using BugsFarm.Views.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BugsFarm.Views.Fight.Ui
{
    public class SquadSelectItemView : AUiView, IPointerUpHandler, IPointerDownHandler, IDragHandler
    {
        private bool RaycastTarget
        {
            set => iconBackground.raycastTarget = value;
        }

        public AntType AntType
        {
            get => antType;
            set => antType = value;
        }

        public Image Icon => icon;

        [SerializeField] private AntType antType;
        [SerializeField] private Image iconBackground;
        [SerializeField] private Image icon;
        [SerializeField] private Text levelText;
        [SerializeField] private Text foodText;

        public void OnPointerDown(PointerEventData eventData)
        {
            RaycastTarget = false;

            var squadSelectView = GetComponentInParent<UiSquadSelectView>();
            var siblingIndex = transform.GetSiblingIndex();

            transform.SetParent(squadSelectView.DragItemParent);

            squadSelectView.EmptyItem.transform.SetSiblingIndex(siblingIndex - 1);
            squadSelectView.EmptyItem.SetActive(true);

            squadSelectView.SetDoneButtonInteract();

            SetPosToMouse();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            RaycastTarget = true;

            var squadSelectView = GetComponentInParent<UiSquadSelectView>();
            if (!SquadSelectSlots.MoveTo(this))
            {
                transform.SetParent(squadSelectView.Content);
                transform.SetSiblingIndex(squadSelectView.EmptyItem.transform.GetSiblingIndex());
            }

            squadSelectView.EmptyItem.SetActive(false);
            squadSelectView.EmptyItem.transform.SetSiblingIndex(0);
            squadSelectView.SetDoneButtonInteract();
        }

        public void OnDrag(PointerEventData eventData) => SetPosToMouse();

        private void SetPosToMouse() => transform.SetXY(Tools.Mouse_w());
    }
}