using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIScrollRectComponent : MonoBehaviour
    {
        [SerializeField] private DOSettings _slideSettings;
        [SerializeField] private Button _leftArrow;
        [SerializeField] private Button _rightArrow;
        [SerializeField] private RectTransform _content;

        private int _curerntItemIndex = 0;
        private void Move()
        {
            // TODO: realization slide, update items correclty
        }
        private void OnEnable()
        {
            _leftArrow.onClick.AddListener(OnLeftClick);
            _rightArrow.onClick.AddListener(OnRigthClick);
        }
        private void OnDisable()
        {
            _leftArrow.onClick.RemoveListener(OnLeftClick);
            _rightArrow.onClick.RemoveListener(OnRigthClick);
        }
        private void OnDestroy()
        {
            _leftArrow.onClick.RemoveListener(OnLeftClick);
            _rightArrow.onClick.RemoveListener(OnRigthClick);
        }
        private void OnLeftClick()
        {
            if(_curerntItemIndex <= 0) return;
            _curerntItemIndex--;
            Move();
        }		
        private void OnRigthClick()
        {
            if(_curerntItemIndex+1 >= _content.childCount) return;
            _curerntItemIndex++;
            Move();
        }
    }
}