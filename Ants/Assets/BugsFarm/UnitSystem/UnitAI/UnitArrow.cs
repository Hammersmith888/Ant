using System;
using BugsFarm.AudioSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.SimulationSystem;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.UnitSystem
{
    public class UnitArrow : MonoBehaviour, IMonoPoolable, ITickable
    {
        
        /// <summary>
        /// Arrow shot in the target.
        /// float arg : accuracy of the shot in 0-1 space - use for calculate the shot demage
        /// </summary>
        public event Action<float> OnArrowTargetReached;
        public GameObject GameObject => gameObject;
        
        [SerializeField] private SpriteRenderer _arrowRenderer;

    #region Default Params
        private readonly Vector2 _defaultMiss = new Vector2(-0.15f, 0.15f);
        private const float _arrowFadeTimeDefault = 0.75f;
        private const float _arrowSpeedDefault = 10f;
        private const float _arrowAccuracyDefault = 0.9f;
    #endregion

        private Vector2 HitPos => (_target ? (Vector2)_target.position : Vector2.zero) + _miss;
        private float Gravity => Physics2D.gravity.y;
        private Vector2 DeltaLine => HitPos - _arrowInitPosition;
        private Vector2 DirLine => DeltaLine.normalized;
        private float DistLine => DeltaLine.magnitude;
        private float TimeRest => DistLine / _speedLine;
        private Vector2 Dir(float vy) => (_speedLine * DirLine + Vector2.up * vy).normalized;
        
        private TickableManager _tickableManager;
        private ISoundSystem _soundSystem;
        private AudioModelStorage _audioModelStorage;
        private IMonoPool _monoPool;
        
        private AudioModel _audioModel;
        private Transform _target;
        private Vector2 _arrowInitPosition;
        private float _accuracy01;
        private float _arrowSpeed;
        private float _fadeTimerMax;
        private float _fadeTimer;
        private bool _isTargetReached;
        
        // calc params
        private Vector2 _miss;
        private float _speedLine;
        private float _timeFull;
        private float _vy0;
        private float _adj;

        private readonly string [] _shootAudioClipNames = {"Shoot1","Shoot2"};
        private readonly string [] _shotAudioClipNames  = {"Shot1", "Shot2"};
        
        [Inject]
        private void Inject(TickableManager tickableManager,
                            ISoundSystem soundSystem,
                            AudioModelStorage audioModelStorage,
                            IMonoPool monoPool)
        {
            _soundSystem = soundSystem;
            _audioModelStorage = audioModelStorage;
            _monoPool = monoPool;
            _tickableManager = tickableManager;
        }

        public void Init(Vector3 initPostion,
                         Transform target,
                         float accuracy01 = _arrowAccuracyDefault,
                         float arrowSpeed = _arrowSpeedDefault,
                         float fadeTimerMax = _arrowFadeTimeDefault)
        {
            _audioModel = _audioModelStorage.Get(GetType().Name);
            _arrowInitPosition = initPostion;
            _accuracy01 = accuracy01;
            _arrowSpeed = arrowSpeed;
            _fadeTimerMax = fadeTimerMax;
            _target = target;
            _miss = _defaultMiss.RangeRandom() * (1f - _accuracy01);
            
            const int a = 4;
            var b = (-4) * (DeltaLine.y * Gravity * DistLine + Mathf.Pow(_arrowSpeed, 2));
            var c = Mathf.Pow(Gravity * DistLine, 2);
            var y = Solve(a, b, c);

            _speedLine = Mathf.Sqrt(y);
            _timeFull = DistLine / _speedLine;
            _vy0 = Gravity * _timeFull / (-2);


            var dir = Dir(_vy0);
            var length = Tools.Size(_arrowRenderer).x;
            _adj = length * dir.y;
            
            SetPosAndRotation(0, 1, dir);
            _soundSystem.Play(transform.position, 
                              _audioModel.GetAudioClip(_shootAudioClipNames[Random.Range(0,_shootAudioClipNames.Length)]));
        }
        public void Tick()
        {
            if (!_target || _isTargetReached)
            {
                FadeIn();
                return;
            }
            
            var dist = _speedLine;

            if (dist > DeltaLine.magnitude)
            {
                _isTargetReached = true;
                transform.rotation *= Quaternion.AngleAxis(Random.Range(-30, 30), Vector3.forward);
                SetPosToTarget();
                var missDistance = Mathf.Min((HitPos - (Vector2) _target.position).magnitude, 1f);
                _soundSystem.Play(_target.position,
                                  _audioModel.GetAudioClip(_shotAudioClipNames[Random.Range(0,_shotAudioClipNames.Length)]));
                OnArrowTargetReached?.Invoke(1f - missDistance);
            }
            else
            {
                _arrowInitPosition += DirLine * dist; // (!) BEFORE other calculations

                var timePassed = _timeFull - TimeRest;
                var t = timePassed;
                var h = _vy0 * t + Gravity * t * t / 2;
                var vy = _vy0 + Gravity * t;

                var dir = Dir(vy);

                SetPosAndRotation(h, TimeRest / _timeFull, dir);
            }
        }
        public void OnDespawned()
        {
            _arrowInitPosition = _miss = Vector2.zero;
            _vy0 = _adj = _timeFull = _speedLine = _fadeTimer = 0;
            _isTargetReached = false;
            OnArrowTargetReached = null;
            _target = null;
            
            if(_arrowRenderer)
            {
                _arrowRenderer.color = Color.white;
            }

            if (transform)
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
            }
            
            if (gameObject)
            {
                gameObject.SetActive(false);
            }
        }
        public void OnSpawned()
        {
           /*if (_simulationSystem.Simulation)
            {
                var missDistance = Mathf.Min((HitPos - (Vector2) _target.position).magnitude, 1f);
                OnArrowTargetReached?.Invoke(1f - missDistance);
                _monoPool.Despawn(this);
                return;
            }*/
            _fadeTimer = 0;
            transform.position = _arrowInitPosition;
            transform.rotation = Quaternion.identity;
            _tickableManager.Add(this);
            gameObject.SetActive(true);
        }
        private void OnDestroy()
        {
            _monoPool?.Destroy(this);
        }

        private void FadeIn()
        {
            _fadeTimer += Time.deltaTime;
            var arrowColor = _arrowRenderer.color;
            arrowColor.a = Mathf.Min(1f, 1f - _fadeTimer);
            _arrowRenderer.color = arrowColor;

            SetPosToTarget();

            if (_fadeTimer > _fadeTimerMax)
            {
                _tickableManager?.Remove(this);
                _monoPool.Despawn(this);
            }
        }
        private float Solve(float a, float b, float c)
        {
            var d = b * b - 4 * a * c;
            var rD = d < 0 ? 0 : Mathf.Sqrt(d);

            var x1 = (-b - rD) / (2 * a);
            var x2 = (-b + rD) / (2 * a);

            return
                x1 <= 0 ? x2 :
                x2 <= 0 ? x1 :
                x1 > x2 ? x1 :
                x2;
        }
        private void SetPosAndRotation(float h, float adj01, Vector2 dir)
        {
            transform.position = _arrowInitPosition + Vector2.up * (h + _adj * adj01);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector2.Perpendicular(dir));
        } 
        private void SetPosToTarget()
        {
            transform.position = HitPos;
        }
    }
}