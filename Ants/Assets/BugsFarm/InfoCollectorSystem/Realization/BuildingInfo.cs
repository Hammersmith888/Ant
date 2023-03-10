using System;

namespace BugsFarm.InfoCollectorSystem
{
    public class BuildingInfo : IBuildingInfo, IDisposable
    {
        public string Id { get; }
        public float Progress { get; private set; }
        public string Info { get; private set; }
        public string Description { get; private set; }

        private readonly BuildingInfoStorage _buildingInfoStorage;
        private bool _finalized;
        public BuildingInfo(string id, BuildingInfoStorage buildingInfoStorage)
        {
            Id = id;
            _buildingInfoStorage = buildingInfoStorage;
            buildingInfoStorage.Add(this);
        }

        public void Dispose()
        {
            if(_finalized) return;
            
            _finalized = true;
            _buildingInfoStorage.Remove(this);
        }

        internal void SetInfo(float progress, string format ="", string desc = "")
        {
            if(_finalized) return;
            Info = format;
            Progress = progress;
            Description = desc;
        }
    }
}