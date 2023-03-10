using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace BugsFarm.BuildingSystem
{
    public class LarvaBirthTask : BaseTask
    {
        private const AnimKey _birthAnimKey = AnimKey.Birth;
        private readonly IInstantiator _instantiator;
        private readonly AnimatorStorage _animatorStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private const string _birthModelIDStatKey = "stat_birthModelID";

        private ISpineAnimator _animator;
        private UnitBirthModel _birthModel;
        private List<Vector2> _positions;
        private int _birthCount;

        public LarvaBirthTask(IInstantiator instantiator,
                              AnimatorStorage animatorStorage,
                              StatsCollectionStorage statsCollectionStorage)
        {
            _instantiator = instantiator;
            _animatorStorage = animatorStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(args);
            var parentGuid = (string) args[0];
            _birthModel = (UnitBirthModel) args[1];
            _birthCount = (int) args[2];
            _positions = new List<Vector2>((IEnumerable<Vector2>) args[3]);

            if (_birthCount > _positions.Count)
            {
                throw new ArgumentOutOfRangeException($"{this} : {nameof(Execute)} :: Positions count : {_positions.Count}, must be equal or greater to birth count : {_birthCount}");
            }

            _animator = _animatorStorage.Get(parentGuid);
            _animator.OnAnimationComplete += OnAnimationComplete;
            Process();
        }

        private void Process()
        {
            if (Finalized || !IsRunned) return;
            if (_birthCount <= 0)
            {
                Completed();
                return;
            }

            _birthCount--;
            _animator.SetAnim(_birthAnimKey);
        }

        private void Birth()
        {
            if (Finalized || !IsRunned) return;
            var birthUnits = _birthModel.BirthUnitsModelID;
            var randomUnit = birthUnits[Random.Range(0, birthUnits.Length)];
            var position = _positions[0];
            _positions.RemoveAt(0);

            var unitBuildingProtocol = new CreateUnitProtocol(_birthModel.LarvaModelID, true);
            _instantiator.Instantiate<CreateUnitCommand>().Execute(unitBuildingProtocol);

            var statCollection = _statsCollectionStorage.Get(unitBuildingProtocol.Guid);
            statCollection.AddModifier(_birthModelIDStatKey, new StatModBaseAdd(int.Parse(randomUnit)));

            var spawnProtocol = new UnitSpawnProtocol(unitBuildingProtocol.Guid, position);
            _instantiator.Instantiate<UnitSpawnCommand<SpawnFromAlphaTask>>().Execute(spawnProtocol);
            Process();
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if (Finalized || !IsRunned || animKey != _birthAnimKey) return;
            Birth();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
            }

            _animator = null;
            _birthModel = default;
            _positions = null;
            base.OnDisposed();
        }
    }
}