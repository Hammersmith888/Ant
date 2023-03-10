using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class CameraMovementSystem : IExecuteSystem
    {
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;

        public CameraMovementSystem(BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings)
        {
            _battle = battle;
            _battleSettings = battleSettings;
        }

        public void Execute()
        {
            if (!_battle.hasMoveCamera)
                return;

            var to = _battle.moveCamera.To;
            var transform = Camera.main.transform;
            var position = transform.position;

            position = Vector3.Lerp(
                position,
                to,
                (Time.time - _battle.moveCamera.StartTime) * _battleSettings.cameraSpeed);

            transform.position = position;

            if (Vector3.Distance(transform.position, _battle.moveCamera.To) <= 0.05f)
            {
                transform.position = _battle.moveCamera.To;
                _battle.RemoveMoveCamera();
            }
        }
    }
}