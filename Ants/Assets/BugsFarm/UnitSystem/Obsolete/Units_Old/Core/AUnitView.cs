using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public abstract class AUnitView : MonoBehaviour
    { 
        public GameObject SeflContainer => _selfContainer;
        [SerializeField] protected GameObject _selfContainer;
        public abstract void SetCollidersAllowed(bool allowed);
        public abstract void RefreshColliders();
    }
}