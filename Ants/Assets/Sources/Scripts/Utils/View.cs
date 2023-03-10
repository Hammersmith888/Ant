using System;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.UI
{

    public class View : MonoBehaviour
    {
        [Header("View")]
        [SerializeField]
        private GameObject content;

        [Header("Start")]
        public bool IsOpenStart = false;
        public bool IsResetPosition = true;

        [Space]
        public UnityEvent OnOpened;
        public UnityEvent OnClosed;

        public virtual GameObject Content
        {
            get => content;
            set => content = value;
        }

        public virtual bool IsOpened
        {
            get => content.activeSelf;
        }

        protected virtual void Awake()
        {
            if (IsResetPosition)
            {
                (transform as RectTransform).anchoredPosition = Vector2.zero;
            }

            Content.SetActive(IsOpenStart);
        }

        [OPS.Obfuscator.Attribute.DoNotRenameAttribute]
        [ContextMenu("Open")]
        public virtual void Open()
        {
            if (IsOpened) { return; }
            Content.SetActive(true);
            OnOpened?.Invoke();
        }

        [OPS.Obfuscator.Attribute.DoNotRenameAttribute]
        [ContextMenu("Close")]
        public virtual void Close()
        {
            if (!IsOpened) { return; }
            Content.SetActive(false);
            OnClosed?.Invoke();
        }

        public virtual void Reset()
        {
            content = transform.GetChild(0).gameObject;
        }
    }
}