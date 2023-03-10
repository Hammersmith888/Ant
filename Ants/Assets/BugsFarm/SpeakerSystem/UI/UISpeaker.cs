using BugsFarm.Services.MonoPoolService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BugsFarm.SpeakerSystem
{
    public class UISpeaker : MonoBehaviour, IMonoPoolable
    {
        private RectTransform _rectTransform;
        public GameObject GameObject => gameObject;

        [SerializeField] private TextMeshProUGUI _text;

        private Vector2 FlipVectorPopup => _rectTransform.localScale;
        private Vector2 FlipVectorText => _text.rectTransform.localScale;
        private bool IsHorizontalFlipped => FlipVectorPopup.x < 0;
        private bool IsVerticalFlipped => FlipVectorPopup.y < 0;
        private Camera _gameCamera;

        private bool _canUpdate;
        private float _lifeTime;
        private Transform _target;
        private IMonoPool _monoPool;

        [Inject]
        private void Inject(IMonoPool monoPool)
        {
            _monoPool = monoPool;
        }

        public void Init(Camera gameCamera,
                         Transform target,
                         float lifeTime,
                         string text)
        {
            _lifeTime = lifeTime;
            _text.text = text;
            _target = target;
            _gameCamera = gameCamera;
            _rectTransform = GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            ApplyPosition();
            CheckOutOfScreen();
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            _canUpdate = false;
            _gameCamera = null;
            _target = null;
            _lifeTime = 0;
            ResetScale(_rectTransform);
            ResetScale(_text.rectTransform);

            if (_text)
            {
                _text.text = string.Empty;
            }

            if (gameObject)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnSpawned()
        {
            CheckOutOfScreen();
            _canUpdate = true;
        }

        private void Update()
        {
            if(!_canUpdate) return;
            
            _lifeTime -= Time.deltaTime;
            if (_lifeTime <= 0)
            {
                _monoPool.Despawn(this);
                return;
            }

            ApplyPosition();
            CheckOutOfScreen();
        }

        private void ResetScale(Transform target)
        {
            if(!target) return;
            
            var absScale = target.localScale;
            absScale.x = Mathf.Abs(absScale.x);
            absScale.y = Mathf.Abs(absScale.y);
            target.localScale = absScale;
        }
        private void CheckOutOfScreen()
        {
            if (_rectTransform && _gameCamera)
            {
                var onLeftSide = _gameCamera.transform.InverseTransformPoint(transform.position).x < 0;
                if ((onLeftSide && IsHorizontalFlipped) || (!onLeftSide && !IsHorizontalFlipped))
                {
                    FlipPopup(Vector2.right);
                }
            }
        }

        private void FlipPopup(Vector2 axies)
        {
            if(axies.Max() == 0) return;
            var flipVectorPopup = FlipVectorPopup;
            var flipVectorText = FlipVectorText;
            
            if (Mathf.Abs(axies.x) > 0)
            {
                flipVectorPopup.x *= -1;
                flipVectorText.x *= -1;
            }            
            if (Mathf.Abs(axies.y) > 0)
            {
                flipVectorPopup.y *= -1;
                flipVectorText.y *= -1;
            }
            
            transform.localScale = flipVectorPopup;
            _text.transform.localScale = flipVectorText;
        }
        
        private void ApplyPosition()
        {
            if (!_target) return;
            
            var isVerticalInverted = InNormalRange(Vector2.up, 100);
            if (isVerticalInverted && !IsVerticalFlipped)
            {
                FlipPopup(Vector2.up);
            }
            else if(!isVerticalInverted && IsVerticalFlipped)
            {
                FlipPopup(Vector2.up);
            }
            transform.position = _target.position;
        }

        private bool InNormalRange(Vector2 dir, float maxAngle)
        {
            return Vector2.Angle(_target.up, dir) > maxAngle;
        }
        private void OnDestroy()
        {
            _monoPool?.Destroy(this);
        }
    }
}