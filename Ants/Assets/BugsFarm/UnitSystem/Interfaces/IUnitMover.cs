using System;
using BugsFarm.AstarGraph;
using BugsFarm.Graphic;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public interface IUnitMover : IDisposable, IStorageItem
    {
        Vector2 Normal { get; }
        Vector2 Position { get; }
        Vector3 Rotation { get; }
        LocationLayer Layer { get; }

        /// <summary>
        /// Маска тэгов перемещений
        /// </summary>
        int TraversableTags { get; }
        
        event Action OnComplete;

        void Initialize();
        bool CanReachTarget(params Vector2[] targets);
        bool IsLoseGround();
        void ReachTarget();
        void GoTarget(Vector2 endPoint);
        void SetPosition(Vector2 position);
        void SetRotation(Quaternion rotation);
        void SetPosition(INode node);
        void SetLook(bool left);
        void Stay();
    }
}