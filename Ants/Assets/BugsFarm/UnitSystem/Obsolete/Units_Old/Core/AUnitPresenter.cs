using System;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public abstract class AUnitPresenter : IPostSpawnInitable, IPostLoadRestorable, IDisposable
    { 
        public string Name { get; }
        public abstract AUnitView UnitView { get; }
        protected AUnitPresenter(string name = "")
        {
            Name = string.IsNullOrEmpty(name) ? Names.GetRandom() : name;
        }
        public abstract void SetCollidersAllowed(bool allowed);
        public abstract void PostSpawnInit();
        public abstract void PostLoadRestore();
        public abstract void Dispose();
        public abstract void Update();
    }
}