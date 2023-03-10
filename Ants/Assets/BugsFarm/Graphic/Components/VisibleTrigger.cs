using System;
using UnityEngine;

namespace BugsFarm.Graphic
{
    public class VisibleTrigger : MonoBehaviour
    {
        public bool Visible => _becameVisible && isActiveAndEnabled;
        public event Action<bool> OnVisible;
        private bool _becameVisible;
        private void OnEnable()
        {
            // already non visible notified!
            if (_becameVisible)
            {
                OnVisible?.Invoke(true);
            }
        }

        private void OnDisable()
        {
            // already non visible notified!
            if (_becameVisible)
            {
                OnVisible?.Invoke(false);
            }
        }

        private void OnBecameVisible()
        {
            _becameVisible = true;
            OnVisible?.Invoke(true);
        }

        private void OnBecameInvisible()
        {
            _becameVisible = false;
            OnVisible?.Invoke(false);
        }
    }
}