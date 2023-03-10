using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Views.Fight;
using BugsFarm.Views.Screen;
using DG.Tweening;
using Entitas;
using TMPro;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ecs.Sources.Ant.Systems.Battle
{
    public class DamageSystem : ReactiveSystem<AntEntity>, IDisposable
    {
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly FightView _fightView;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly FightScreen _fightScreen;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public DamageSystem(IContext<AntEntity> context,
            BattleSettingsInstaller.BattleSettings battleSettings,
            FightView fightView,
            BattleSettingsInstaller.AntSettings antSettings,
            FightScreen fightScreen) : base(context)
        {
            _battleSettings = battleSettings;
            _fightView = fightView;
            _antSettings = antSettings;
            _fightScreen = fightScreen;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Damage);

        protected override bool Filter(AntEntity entity)
            => entity.hasDamage && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.isArmorBoost)
                {
                    entity.RemoveDamage();
                    return;
                }

                var health = entity.health.Value - entity.damage.Value;

                health = Mathf.Clamp(health, 0, health);
                entity.ReplaceHealth(health);
                SpawnDamageText(entity);

                if (entity.isEnemy && !entity.isAttacking)
                {
                    if (entity.isMoveToTarget || entity.hasMoveToPosition)
                        return;

                    if (entity.antType.Value == AntType.PotatoBug)
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "take_damage", false);
                    else if (entity.antType.Value == AntType.Firefly)
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Damaged", false);
                    else
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Damage", false);
                    
                    Observable.Timer(TimeSpan.FromSeconds(_antSettings.animationDamageDuration))
                        .Subscribe(_ =>
                        {
                            if (entity.isDead)
                                return;

                            if (entity.antType.Value == AntType.CaterpillarBoss)
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "idle", true);
                            else
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "Idle", true);
                        })
                        .AddTo(_disposable);
                }
                else if (entity.isPlayer && !entity.isAttacking)
                {
                    if (entity.antType.Value == AntType.Snail)
                    {
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Damage", false);
                    }
                }

                entity.RemoveDamage();
            }
        }

        private void SpawnDamageText(AntEntity entity)
        {
            var randomX = Vector3.right * (Random.Range(-1f, 1f) * _battleSettings.damageRandomShiftX);
            var randomY = Vector3.up * (Random.Range(-1f, 1f) * _battleSettings.damageRandomShiftY);
            var posBgn = entity.transform.Value.position + randomX + _battleSettings.damageOffsetY0 * Vector3.up;
            var posTgt = entity.transform.Value.position + randomX + _battleSettings.damageOffsetY1 * Vector3.up +
                         Vector3.forward + randomY;

            // Get from Pool
            //var text = Pools.Instance.Get(PoolType.DamageText, _fightScreen.DamagesContainer)
            //    .GetComponent<TMP_Text>();
            TextMeshPro text = null;
            foreach (var item in entity.mbUnit.Value.DamageText)
            {
                if (item.color.a == 0f)
                {
                    text = item;
                    break;
                }
            }

            if (text == null)
                text = entity.mbUnit.Value.DamageText[0];

            // Setup
            text.transform.position = posBgn;
            //text.transform.localScale = Vector3.one;
            text.text = entity.damage.Value.ToString("F0");
            text.color = entity.isPlayer ? _battleSettings.colorDamagePlayer : _battleSettings.colorDamageEnemy;

            //Tween
            text.transform
                .DOMove(posTgt, 1)
                .SetEase(_battleSettings.damageEase)
                .OnComplete(() =>
                {
                    //Pools.Return(PoolType.DamageText, text.gameObject);
                    var color = text.color;
                    color.a = 0f;
                    text.color = color;
                }) // Return to Pool
                ;
        }

        public void Dispose() => _disposable?.Dispose();
    }
}