using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using Entitas;
using Entitas.Unity;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Select
{
    public class InputSelectAntSystem : IExecuteSystem
    {
        private readonly BattleContext _battle;
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;

        public InputSelectAntSystem(BattleContext battle,
            AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings)
        {
            _battle = battle;
            _ant = ant;
            _antSettings = antSettings;
        }

        public void Execute()
        {
            if (!_battle.hasFightState || _battle.fightState.Value != EFightState.Fight)
                return;
            if (_battle.isBattleFinish)
                return;

            if (Input.GetMouseButtonUp(0))
            {
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var mousePos2D = new Vector2(mousePos.x, mousePos.y);

                var hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.transform == null)
                {
                    if (_ant.selectedEntity != null)
                        _ant.selectedEntity.isSelected = false;
                    return;
                }

                var mbUnit = hit.transform.parent.GetComponent<MB_Unit>();
                if (mbUnit == null)
                    mbUnit = hit.transform.GetComponent<MB_Unit>();

                if (hit.collider != null && mbUnit != null)
                {
                    var entity = (AntEntity) mbUnit.gameObject.GetEntityLink().entity;
                    if (entity.isDisabled || entity.isDead || entity.isDestroy)
                        return;

                    if (entity.isEnemy)
                    {
                        if (_ant.selectedEntity != null)
                            ChangeTarget(entity);
                    }
                    else
                    {
                        if (_ant.selectedEntity != null)
                            _ant.selectedEntity.isSelected = false;
                        entity.isSelected = true;
                    }
                }
                else if (_ant.selectedEntity != null)
                {
                    _ant.selectedEntity.isSelected = false;
                }
            }
        }

        private void ChangeTarget(AntEntity target)
        {
            var selectedAnt = _ant.selectedEntity;
            if (selectedAnt.target.Uid == target.uid.Value)
                return;

            var destination = target.transform.Value.position;
            if (target.transform.Value.position.x <= selectedAnt.transform.Value.position.x)
                destination.x += _antSettings.enemyPositionOffset + Random.Range(-.1f, .1f);
            else
                destination.x -= _antSettings.enemyPositionOffset + Random.Range(-.1f, .1f);

            selectedAnt.ReplaceTarget(target.transform.Value, target.uid.Value);
        }
    }
}