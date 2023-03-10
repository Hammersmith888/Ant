using System;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Patrol
{
    public class PatrolWaitSystem : IExecuteSystem
    {
        private readonly IGroup<AntEntity> _group;

        public PatrolWaitSystem(AntContext ant)
        {
            _group = ant.GetGroup(AntMatcher.WaitPatrol);
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var waitTime = entity.waitPatrol.Time;

                waitTime += Time.deltaTime;
                if (waitTime < entity.waitPatrol.TimeMax)
                {
                    entity.ReplaceWaitPatrol(waitTime, entity.waitPatrol.TimeMax);
                }
                else
                {
                    entity.RemoveWaitPatrol();
                    if (entity.isPatrol)
                    {
                        entity.isPatrol = false;
                        entity.isPatrol = true;
                    }
                }
            }
        }
    }
}