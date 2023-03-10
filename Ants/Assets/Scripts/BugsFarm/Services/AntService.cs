using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Views.Fight;
using Ecs.Sources.Ant.Utils;
using Entitas.Unity;
using Spine.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BugsFarm.Services
{
    public class AntService
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly FightView _fightView;
        private readonly BattleSettingsInstaller.AntDatabase _antDatabase;
        private readonly BattleContext _battle;
        private readonly UnitFightSettingsInstaller.UnitFightSettings _unitFightSettings;

        public AntService(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            FightView fightView,
            BattleSettingsInstaller.AntDatabase antDatabase,
            BattleContext battle,
            UnitFightSettingsInstaller.UnitFightSettings unitFightSettings)
        {
            _ant = ant;
            _antSettings = antSettings;
            _fightView = fightView;
            _antDatabase = antDatabase;
            _battle = battle;
            _unitFightSettings = unitFightSettings;
        }

        public AntEntity RegisterEntity(MB_Unit mb, AntType antType, bool inBattle, bool isEnemy)
        {
            var entity = _ant.CreateAnt();

            if (isEnemy)
            {
                entity.isEnemy = true;
                entity.isDisabled = true;
            }
            else
            {
                entity.isPlayer = true;

                var group = _ant.GetGroup(AntMatcher.AllOf(AntMatcher.Player, AntMatcher.InBattle));
                entity.AddId(group.count);
            }

            entity.AddMbUnit(mb);
            entity.AddAntType(antType);
            if (antType == AntType.Archer)
            {
                var archerView = mb.GetComponent<ArcherView>();

                entity.AddArcherView(archerView);
                entity.isRangedUnit = true;
            }

            entity.isInBattle = inBattle;

            entity.AddLevel(0);
            var hp = 10f;
            if (_unitFightSettings.units.ContainsKey(entity.antType.Value))
            {
                var fightItemConfig = _unitFightSettings.units[entity.antType.Value]
                    .levels[entity.level.Value];
                hp = fightItemConfig.hp;
            }
            else
            {
                UnityEngine.Debug.Log($"[{nameof(AntService)}] {entity.antType.Value} not found in config");
            }

            var antVo = GetAntVo(entity.antType.Value);
            entity.AddHealth(hp);
            entity.AddHealthMax(hp);

            var renderer = mb.GetComponentInChildren<Renderer>();
            entity.AddRenderer(renderer);
            entity.AddTransform(mb.transform);

            var skeletonAnimation = mb.GetComponentInChildren<SkeletonAnimation>();
            entity.AddSkeletonAnimation(skeletonAnimation);

            if (isEnemy && _battle.season.Value != 2)
            {
                skeletonAnimation.CustomMaterialOverride.Add(antVo.materialOriginal, antVo.materialFill);
                entity.AddFadeIn(0f);
            }

            entity.AddEnemiesCount(0);

            mb.gameObject.Link(entity, _ant);

            return entity;
        }

        public void CreateDebugBattleAnts(Dictionary<AntType, int> ants)
        {
            foreach (var item in ants)
            {
                for (var i = 0; i < item.Value; i++)
                {
                    var prefab = Data_Fight.Instance.units[item.Key].prefab;
                    var mb = Object.Instantiate(prefab, _fightView.ParentUnits);

                    RegisterEntity(mb, item.Key, true, false);
                }
            }
        }

        public AntVo GetAntVo(AntType type)
        {
            foreach (var antVo in _antDatabase.items)
            {
                if (antVo.type == type)
                    return antVo;
            }

            throw new Exception($"not found {type}");
        }
    }
}