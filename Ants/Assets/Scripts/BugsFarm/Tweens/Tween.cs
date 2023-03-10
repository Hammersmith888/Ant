using System;
using DG.Tweening;
using UnityEngine;

namespace BugsFarm.Tweens
{
    public abstract class Tween : MonoBehaviour
    {
        [Tooltip("Play forward animation on Start")]
        [SerializeField] protected bool _playOnStart = false;

        protected Sequence Sequence { get; private set; }
        /// <summary>
        /// Init automaticly if not called
        /// </summary>
        public void Init()
        {
            if (Sequence.IsNullOrDefault()) Sequence.Kill();
            Sequence = DOTween.Sequence();
        }
        public virtual void Do(Action onComplete = null) => Init();
        public abstract void Rewind(Action onComplete = null);
        public virtual void DoReset() => Sequence = DOTween.Sequence();

        protected virtual void Start()
        {
            if (_playOnStart)
            {
                Do();
            }
        }
        protected virtual void OnInitialize() { }
    }
}