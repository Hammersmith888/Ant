using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Attack
{
    public class UpdateArrowsSystem : IExecuteSystem
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly IGroup<AntEntity> _group;

        public UpdateArrowsSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings)
        {
            _antSettings = antSettings;
            _group = ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.RangedUnit));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var archerView = entity.archerView.Value;

                foreach (var arrow in archerView.Arrows)
                {
                    if (arrow.IsEnabled)
                    {
                        var position = arrow.transform.localPosition;
                        var eulerAngles = arrow.transform.localEulerAngles;

                        position.x += Time.deltaTime * _antSettings.arrowSpeed;

                        if (!entity.hasTarget)
                        {
                            arrow.gameObject.SetActive(false);
                            continue;
                        }

                        var allDistance = Vector3.Distance(
                            entity.transform.Value.position,
                            entity.target.Transform.position);
                        var arrowDistance = Vector3.Distance(
                            entity.transform.Value.position,
                            arrow.transform.position);
                        var yCoef = _antSettings.arrowCurve.Evaluate(arrowDistance / allDistance);
                        
                        position.y -= Time.deltaTime * _antSettings.arrowGravity;
                        eulerAngles.z -= Time.deltaTime * _antSettings.arrowRotationSpeed;

                        arrow.transform.localPosition = position;
                        arrow.transform.localEulerAngles = eulerAngles;
                    }
                    else if (arrow.FadeTimer < _antSettings.arrowFadeDelay + _antSettings.arrowFadeDuration)
                    {
                        arrow.FadeTimer += Time.deltaTime;
                        if (arrow.FadeTimer >= _antSettings.arrowFadeDelay)
                        {
                            var alpha =
                                (arrow.FadeTimer - _antSettings.arrowFadeDelay) / _antSettings.arrowFadeDuration;

                            var color = arrow.SpriteRenderer.color;
                            color.a = 1f - alpha;

                            arrow.SpriteRenderer.color = color;
                        }
                    }
                }
            }
        }
    }
}