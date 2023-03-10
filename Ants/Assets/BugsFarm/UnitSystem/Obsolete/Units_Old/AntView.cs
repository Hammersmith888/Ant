using Spine.Unity;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public class AntView : AUnitView
    {
        public SkeletonAnimation SpineMain => _spine_Main;
        public SkeletonAnimation SpineClimb => _spine_Climb;
        public AntPresenter Ant { get; private set; }
        public UnitRipSceneObject UnitRipSceneObject => _unitRipSceneObject;
        public Transform ArrowPoint => _arrowPoss;
        //public SpeakerView SpeakerView => _speakerView;
    
        [SerializeField] private Transform _arrowPoss;
        //[SerializeField] private TouchCollider[] _touchColliders;
        [SerializeField] private SkeletonAnimation _spine_Main;
        [SerializeField] private SkeletonAnimation _spine_Climb;
        [SerializeField] private UnitRipSceneObject _unitRipSceneObject;

        [SerializeField] private Collider2D _col_Alive;
        [SerializeField] private Collider2D _col_Dead;
        [SerializeField] private Collider2D _col_Climb;
        //[SerializeField] private SpeakerView _speakerView;
    
        public void Init(AntPresenter presenter)
        {
            Ant = presenter;
            // foreach (var touchCollider in _touchColliders)
            // {
            //     touchCollider.OnTouch += OnTouch;
            // }
        }
        public override void SetCollidersAllowed(bool allowed)
        {
            //_RIP.TouchCollider.SetAllow(!Ant.IsAlive);
            // foreach (var touchCollider in _touchColliders)
            // {
            //     touchCollider.SetAllow(allowed);
            // }
        }

        public override void RefreshColliders()
        {
            _col_Alive.gameObject.SetActive(Ant.IsAlive);
            _col_Dead.gameObject.SetActive(!Ant.IsAlive);
        }

        private void Update()
        {
            Ant.Update();
        }
        private void Awake()
        {
            _spine_Main?.Initialize(true);
            _spine_Climb?.Initialize(true);
        }
        private void OnTouch()
        {
            //if ( Tools.IsPointerOverGameObject() || UI_Control.IsModalPanelOpened )
                //return;
        
           // GameEvents.OnAntTap?.Invoke(Ant);
        }
    }
}

