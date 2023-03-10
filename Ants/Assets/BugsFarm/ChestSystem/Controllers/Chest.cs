using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.ChestSystem
{
    public class Chest : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly IInstantiator _instantiator;
        private readonly ChestModelStorage _modelStorage;
        private readonly ChestDtoStorage _dtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly ChestSceneObjectStorage _viewStorage;
        private string ChestTypeNameKey => GetType().Name + "Type_";

        private const string _contentTakenStatKey = "stat_contentTaken";
        private const string _contentShowedStatKey = "stat_contentShowed";
        private const AnimKey _idleAnimKey = AnimKey.Idle;

        private StatModifiable _contentTakenStat;
        private StatModifiable _contentShowedStat;
        private bool _finalized;
        private bool _production;
        private ISpineAnimator _animator;

        public Chest(string guid,
                     IInstantiator instantiator,
                     ChestModelStorage modelStorage,
                     ChestDtoStorage dtoStorage,
                     StatsCollectionStorage statsCollectionStorage,
                     ChestSceneObjectStorage viewStorage)
        {
            _instantiator = instantiator;
            _modelStorage = modelStorage;
            _dtoStorage = dtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _viewStorage = viewStorage;
            Id = guid;
        }

        public void Initialize()
        {
            var statCollection = _statsCollectionStorage.Get(Id);
            _contentTakenStat = statCollection.Get<StatModifiable>(_contentTakenStatKey);
            _contentShowedStat = statCollection.Get<StatModifiable>(_contentShowedStatKey);
            _contentShowedStat.OnValueChanged += OnContentChanged;
            _contentTakenStat.OnValueChanged += OnContentChanged;
            Production();
        }

        public void Dispose()
        {
            if (_finalized) return;

            _finalized = true;
            _contentShowedStat.OnValueChanged -= OnContentChanged;
            _contentTakenStat.OnValueChanged -= OnContentChanged;
            _contentTakenStat = null;
            _contentShowedStat = null;
            var removeAnimatorProtocol = new RemoveAnimatorProtocol(Id);
            _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(removeAnimatorProtocol);
        }

        private void Production()
        {
            if (_finalized)
                return;

            var showed = _contentShowedStat.Value > 0;
            var taken = _contentTakenStat.Value >= 1;

            if (showed && !taken && !_production)
            {
                _production = true;
                var buildSceneObjectProtocol = new CreateChestSceneObjectPrtotocol(Id);
                _instantiator.Instantiate<CreateChestSceneObjectCommand>().Execute(buildSceneObjectProtocol);

                var view = _viewStorage.Get(Id);
                var dto = _dtoStorage.Get(Id);
                var model = _modelStorage.Get(dto.ModelID);
                var animModelID = ChestTypeNameKey + model.TypeID;
                var createAnimatorProtocol =
                    new CreateAnimatorProtocol(Id, animModelID, res => _animator = res, view.MainSkeleton);
                _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(createAnimatorProtocol);

                _animator.SetAnim(_idleAnimKey);

                view.ChangeVisible(true);
                view.ChangeInteractable(true);
            }
            else if (taken && !_finalized)
            {
                var protocol = new DeleteChestProtocol(Id);
                _instantiator.Instantiate<DeleteChestCommand>().Execute(protocol);
            }
        }

        private void OnContentChanged(object sender, EventArgs e)
        {
            Production();
        }
    }
}