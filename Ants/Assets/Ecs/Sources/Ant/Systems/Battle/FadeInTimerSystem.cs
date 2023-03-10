using System;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle
{
    public class FadeInTimerSystem : IExecuteSystem
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;
        private readonly IGroup<AntEntity> _group;

        public FadeInTimerSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService)
        {
            _antSettings = antSettings;
            _antService = antService;
            _group = ant.GetGroup(AntMatcher.AllOf(AntMatcher.FadeIn).NoneOf(AntMatcher.Disabled));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var fadeInTime = entity.fadeIn.Time;

                fadeInTime += Time.deltaTime;
                entity.ReplaceFadeIn(fadeInTime);

                if (fadeInTime >= _antSettings.fadeInDelay)
                {
                    var renderer = entity.renderer.Value;
                    var value = (fadeInTime - _antSettings.fadeInDelay) / _antSettings.fadeInDuration;

                    value = 1f - value;
                    if (value <= 0.05f)
                    {
                        renderer.material.SetFloat("_FillPhase", 0f);
                        entity.RemoveFadeIn();

                        if (entity.hasAntType)
                        {
                            var antVo = _antService.GetAntVo(entity.antType.Value);
                            entity.skeletonAnimation.Value.CustomMaterialOverride.Remove(antVo.materialOriginal);
                        }
                    }
                    else
                    {
                        renderer.material.SetFloat("_FillPhase", value);
                    }
                }
            }
        }
    }
}