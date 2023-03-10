using System;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Services.UIService;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BugsFarm.CurrencyCollectorSystem
{
    public class CurrencyCollectorAnimation : ICurrencyAnimation
    {
        private readonly UIRoot _uiRoot;
        private readonly ISoundSystem _soundSystem;
        private readonly AudioModelStorage _audioModelStorage;
        private const Ease _easeMain = Ease.InQuad;
        private const Ease _easeX1   = Ease.OutSine;
        private const Ease _easeX2   = Ease.InSine;

        private const float _distMin  = 0.35f;
        private const float _distMax  = 100;
        private const float _shiftMin = 10;
        private const float _shiftMax = 100.75f;
        private const float _tMin  = 0.729f;
        private const float _tMax  = 1.08f;
        private const float _scale = 0.8f;

        public CurrencyCollectorAnimation(UIRoot uiRoot, 
                                          ISoundSystem soundSystem,
                                          AudioModelStorage audioModelStorage)
        {
            _uiRoot = uiRoot;
            _soundSystem = soundSystem;
            _audioModelStorage = audioModelStorage;
        }
        
        /// <summary>
        /// Collect currency with animation
        /// </summary>
        /// <param name="startPosition"> in world space</param>
        /// <param name="targetPosition"> in world space</param>
        /// <param name="itemTarget"> target item to animate</param>
        /// <param name="camera">target transformation camera</param>
        /// <param name="onComplete"> after all currency colleted event [can be null]</param>
        public void Animate(Vector2 startPosition, 
                            Vector2 targetPosition, 
                            bool useWorldPosition,
                            ICurrencyAnimationItem itemTarget, 
                            Camera camera,
                            Action onComplete)
        {
            if (itemTarget == null || onComplete == null)
            {
                throw new ArgumentException($"{this} : {nameof(Animate)} :: {nameof(ICurrencyAnimationItem)} : [Has item : {itemTarget != null}], {nameof(Action)} : [Has Callback : {onComplete != null}].");
            }
            
            var vpPosition = camera.WorldToViewportPoint(startPosition);
            var canvasRect = _uiRoot.UICanvas.gameObject.GetComponent<RectTransform>();

            Vector2 wpPosition = startPosition;
            if (useWorldPosition)
            {
                wpPosition = new Vector2(
                    ((vpPosition.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
                    ((vpPosition.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f)));
            }

            var delta = targetPosition - wpPosition;
            var direction = delta.normalized;
            var dist01 = Mathf.InverseLerp(_distMin, _distMax, delta.magnitude);
            var duration = Mathf.Lerp(_tMin, _tMax, dist01);
            var shiftMax = Mathf.Lerp(_shiftMin, _shiftMax, dist01);
            var q = Quaternion.LookRotation(Vector3.forward, direction);

            itemTarget.ParentAnimateTarget.rotation = q;

            if (useWorldPosition)
            {
                itemTarget.ParentAnimateTarget.anchoredPosition = wpPosition;
            }
            else
            {
                itemTarget.ParentAnimateTarget.position = wpPosition;
            }
            
            itemTarget.ChildAnimateTarget.localRotation = Quaternion.Inverse(q);
            itemTarget.ChildAnimateTarget.localScale = Vector3.zero;
            var shift = Random.insideUnitCircle * shiftMax;
            var audioModel = _audioModelStorage.Get("UIAudio");
            var audioClip = audioModel.GetAudioClip("Currency");
            DOTween.Sequence()
                .Append(itemTarget.ChildAnimateTarget.DOLocalMove(shift, duration / 2).SetEase(_easeX1))
                .Append(itemTarget.ChildAnimateTarget.DOLocalMove(Vector3.zero, duration / 2).SetEase(_easeX2))
                .Insert(0, itemTarget.ParentAnimateTarget.DOMove(targetPosition, duration).SetEase(_easeMain))
                .Insert(0, itemTarget.ChildAnimateTarget.DOScale(Vector3.one * _scale, duration / 2))
                .OnComplete(() =>
                {
                    _soundSystem.Play(audioClip);
                    itemTarget.ChildAnimateTarget.DOKill();
                    itemTarget.ParentAnimateTarget.DOKill();
                    onComplete.Invoke();
                }).Play();
        }
    }
}