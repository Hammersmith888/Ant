using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.Views.Fight;
using BugsFarm.Views.Screen;
using DG.Tweening;
using Entitas;
using Entitas.Unity;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace BugsFarm.Services
{
    public class BattleService : IInitializable, IDisposable
    {
        private readonly ResourceService _resourceService;
        private readonly BattleContext _battle;
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly FightView _fightView;
        private readonly AntService _antService;
        private readonly FightScreen _fightScreen;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public BattleService(
            ResourceService resourceService,
            BattleContext battle,
            AntContext ant,
            BattleSettingsInstaller.BattleSettings battleSettings,
            FightView fightView,
            AntService antService,
            FightScreen fightScreen,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings)
        {
            _resourceService = resourceService;
            _battle = battle;
            _ant = ant;
            _battleSettings = battleSettings;
            _fightView = fightView;
            _antService = antService;
            _fightScreen = fightScreen;
            _unitViewSettings = unitViewSettings;
        }

        public void Initialize()
        {
            _fightScreen.RetreatButton.OnClickAsObservable()
                .Subscribe(_ => Retreat())
                .AddTo(_disposable);
            _fightScreen.BackButton.OnClickAsObservable()
                .Subscribe(_ => Back())
                .AddTo(_disposable);
            _fightScreen.ArmorButton.Button.OnClickAsObservable()
                .Subscribe(_ => OnArmor())
                .AddTo(_disposable);
            _fightScreen.AttackButton.Button.OnClickAsObservable()
                .Subscribe(_ => OnAttackBoost())
                .AddTo(_disposable);
        }

        public void StartBattle(Dictionary<AntType, int> ants)
        {
            _antService.CreateDebugBattleAnts(ants);
            _fightScreen.FightPanel.SetIcons(_ant, _unitViewSettings);

            Observable.Timer(TimeSpan.FromSeconds(0.2f))
                .Subscribe(_ =>
                {
                    Camera.main.transform.position = new Vector3(
                        Camera.main.transform.position.x,
                        Camera.main.transform.position.y,
                        -10f);
                });

            _battle.ReplaceFightState(EFightState.TurnLightOn);

            var group = _battle.GetGroup(BattleMatcher.BattleRoom);
            foreach (var entity in group.GetEntities())
            {
                foreach (var spriteMask in entity.battleRoom.Value.SpriteMasks)
                    spriteMask.enabled = true;
            }
        }

        public void MoveCamera()
        {
            var position = Camera.main.transform.position;
            var destination = position;
            destination.y += _battleSettings.cameraOffset;
            _battle.ReplaceMoveCamera(position, destination, Time.time);
        }

        public static void TweenDarkness(BattleRoom room, float targetDarkness, float duration, float delay,
            Action onComplete = null)
        {
            DOTween.To(
                    () => room.Darkness,
                    x => room.Darkness = x,
                    targetDarkness,
                    duration
                )
                .SetDelay(delay)
                .OnComplete(() => onComplete?.Invoke());
            ;
        }

        public void SetGround()
        {
            var cameraPos_w = Camera.main.transform.position.y;
            var groundPos_w = cameraPos_w;
            var shift_w = cameraPos_w;

            var etalonResolutionWithBuffer = new Vector2(1080 + 400, 1920 + 500);
            const float pixelsPerUnit = 100;
            var etalonResolutionWithBufferHeight_w = etalonResolutionWithBuffer.y / pixelsPerUnit;
            var groundTileHeight_w = etalonResolutionWithBufferHeight_w;
            var shift_V = shift_w / groundTileHeight_w;

            _fightView.Ground.transform.SetY(groundPos_w);

            _battleSettings.materialGround.SetFloat("_Voffset", shift_V);
        }

        public static int AttackRange(AntType type)
        {
            switch (type)
            {
                case AntType.Archer: return 1;
                default: return 0;
            }
        }

        public void SetNextTarget(AntEntity selectedEntity)
        {
            if (selectedEntity.enemiesCount.Value > 0)
            {
                IGroup<AntEntity> group;
                if (selectedEntity.isPlayer)
                {
                    group = _ant.GetGroup(AntMatcher
                        .AllOf(AntMatcher.Enemy, AntMatcher.Target)
                        .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
                }
                else
                {
                    group = _ant.GetGroup(AntMatcher
                        .AllOf(AntMatcher.Player, AntMatcher.Target)
                        .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
                }

                var list = new List<AntEntity>();
                foreach (var entity in group.GetEntities())
                {
                    if (entity.target.Uid == selectedEntity.uid.Value)
                        list.Add(entity);
                }

                var nearestUnit = GetNearestUnit(list, selectedEntity);
                if (nearestUnit == null)
                {
                    //Debug.LogWarning("nearestUnit == null");
                    return;
                }

                selectedEntity.AddTarget(nearestUnit.transform.Value, nearestUnit.uid.Value);
                nearestUnit.ReplaceEnemiesCount(nearestUnit.enemiesCount.Value + 1);
            }
            else
            {
                IGroup<AntEntity> group;
                if (selectedEntity.isPlayer)
                {
                    group = _ant.GetGroup(AntMatcher
                        .AllOf(AntMatcher.Enemy)
                        .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
                }
                else
                {
                    group = _ant.GetGroup(AntMatcher
                        .AllOf(AntMatcher.Player)
                        .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
                }

                var list = group.GetEntities().ToList();
                var dictionary = new Dictionary<int, List<AntEntity>>();
                var minIndex = -1;

                foreach (var entity in list)
                {
                    var enemiesCount = entity.enemiesCount.Value;
                    if (!dictionary.ContainsKey(enemiesCount))
                    {
                        dictionary.Add(enemiesCount, new List<AntEntity> {entity});
                        if (minIndex == -1 || enemiesCount < minIndex)
                            minIndex = enemiesCount;
                    }
                    else
                    {
                        var newList = dictionary[enemiesCount];
                        newList.Add(entity);
                        dictionary[enemiesCount] = newList;
                        if (enemiesCount < minIndex)
                            minIndex = enemiesCount;
                    }
                }

                if (dictionary.Count == 0)
                    throw new Exception("dictionary.Count == 0");

                var unitList = dictionary[minIndex];
                if (unitList.Count == 0)
                    throw new Exception("unitList.Count == 0");

                var nearestUnit = GetNearestUnit(unitList, selectedEntity);
                if (nearestUnit == null)
                    throw new Exception("nearestUnit == null");

                selectedEntity.ReplaceTarget(nearestUnit.transform.Value, nearestUnit.uid.Value);
                nearestUnit.ReplaceEnemiesCount(nearestUnit.enemiesCount.Value + 1);
            }
        }

        private static AntEntity GetNearestUnit(IEnumerable<AntEntity> list, AntEntity playerEntity)
        {
            AntEntity nearestUnit = null;
            var minDistance = Mathf.Infinity;

            foreach (var entity in list)
            {
                if (nearestUnit == null)
                {
                    nearestUnit = entity;
                    var distance = Vector3.Distance(
                        playerEntity.transform.Value.position,
                        nearestUnit.transform.Value.position);
                    minDistance = distance;
                }
                else
                {
                    var distance = Vector3.Distance(
                        playerEntity.transform.Value.position,
                        entity.transform.Value.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestUnit = entity;
                    }
                }
            }

            return nearestUnit;
        }

        private void Retreat()
        {
            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Dead));
            foreach (var entity in playerGroup)
                entity.isRetreat = true;

            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Enemy)
                .NoneOf(AntMatcher.Dead, AntMatcher.Destroy, AntMatcher.Disabled));

            foreach (var entity in enemyGroup)
                entity.isRetreat = true;
        }

        private void Back()
        {
            ClearEntities();
            _battle.ReplaceFightState(EFightState.None);

            Observable.NextFrame()
                .Subscribe(_ => SceneManager.LoadScene("GlobalMap"))
                .AddTo(_disposable);
        }

        private void OnArmor()
        {
            if (_battle.isArmorBoost || _battle.fightState.Value != EFightState.Fight)
                return;

            var group = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            foreach (var entity in group.GetEntities())
                entity.isArmorBoost = true;

            _battle.isArmorBoost = true;
            _battle.ReplaceArmorTime(0f);
        }
        
        private void OnAttackBoost()
        {
            if (_battle.hasAttackBoost || _battle.fightState.Value != EFightState.Fight)
                return;

            var group = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            foreach (var entity in group.GetEntities())
                entity.isAttackBoost = true;

            _battle.ReplaceAttackBoost(0f);
        }

        public void ClearEntities()
        {
            var antGroup = _ant.GetGroup(AntMatcher.AnyOf(AntMatcher.Ant, AntMatcher.Chest));
            foreach (var entity in antGroup.GetEntities())
            {
                if (entity.isPatrol)
                    entity.isPatrol = false;
                if (entity.hasMbUnit)
                    entity.mbUnit.Value.gameObject.Unlink();
                if (entity.hasGameObject)
                    entity.gameObject.Value.Unlink();

                entity.Destroy();
            }

            var roomGroup = _battle.GetGroup(BattleMatcher.AllOf(BattleMatcher.BattleRoom));
            foreach (var entity in roomGroup.GetEntities())
            {
                entity.battleRoom.Value.gameObject.Unlink();
                entity.Destroy();
            }

            _battle.isInit = false;
        }

        public void Dispose() => _disposable?.Dispose();
    }
}