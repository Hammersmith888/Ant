using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using Ecs.Sources.Ant.Utils;
using Ecs.Sources.Battle.Utils;
using Entitas;
using Entitas.Unity;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class SpawnRoomSystem : ReactiveSystem<BattleEntity>
    {
        private readonly BattleContext _battle;
        private readonly BattleRooms _battleRooms;
        private readonly AntService _antService;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly AntContext _ant;

        public SpawnRoomSystem(
            BattleContext battle,
            BattleRooms battleRooms,
            AntService antService,
            BattleSettingsInstaller.BattleSettings battleSettings,
            AntContext ant) : base(battle)
        {
            _battle = battle;
            _battleRooms = battleRooms;
            _antService = antService;
            _battleSettings = battleSettings;
            _ant = ant;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.SpawnRoom);

        protected override bool Filter(BattleEntity entity)
            => entity.isSpawnRoom;

        private int _lowestRoomLevel;

        protected override void Execute(List<BattleEntity> entities)
        {
            _battle.isSpawnRoom = false;

            var roomGroup = _battle.GetGroup(BattleMatcher.Room);
            var spawnRoomsCount = roomGroup.count == 0 ? _battleSettings.spawnRoomsCount : 1;

            for (var i = 0; i < spawnRoomsCount; i++)
                CreateRoom();
        }

        private void CreateRoom()
        {
            var roomEntity = _battle.CreateRoom();
            var roomGroup = _battle.GetGroup(BattleMatcher.Room);
            roomEntity.AddRoomIndex(_battle.lastRoomIndex.Value + roomGroup.count - 1);

            if (_battle.lastRoomIndex.Value == roomEntity.roomIndex.Value)
                roomEntity.isCurrentRoom = true;

            var position = _battleRooms.PosRoom1.position;
            var spawnRoomOffset = _battleSettings.spawnRoomOffset[_battle.season.Value - 1];
            position.y += spawnRoomOffset * (roomGroup.count - 1);

            var roomPrefab = _battle.season.Value == 2 ? _battleRooms.RoomNewPrefab : _battleRooms.RoomPrefab;
            var room = Object.Instantiate(roomPrefab, position, Quaternion.identity,
                _battleRooms.transform);

            roomEntity.AddBattleRoom(room);
            room.gameObject.Link(roomEntity, _battle);

            // Level
            var roomIndex = roomEntity.roomIndex.Value + 1;
            room.Level.text = $"{Texts.FightLvl} {roomIndex}";

            if (_battle.season.Value == 2)
            {
                if (roomIndex > 0 && roomIndex % 2 == 0)
                    room.Props[1].SetActive(true);
                else
                    room.Props[0].SetActive(true);
            }

            var materialUnits = new Material(_battleSettings.splineMaskFog);
            var enemies = new Squad(false);
            Squads.CreateEnemy(enemies, roomIndex);

            // chest entity
            var chestEntity = _ant.CreateAnt();
            var renderer = room.Chest.GetComponentInChildren<Renderer>();

            chestEntity.isChest = true;
            chestEntity.AddRenderer(renderer);
            chestEntity.AddChestRoomIndex(roomIndex - 1);
            chestEntity.AddGameObject(room.Chest.gameObject);
            room.Chest.gameObject.Link(chestEntity, _ant);

            if (roomEntity.isCurrentRoom)
            {
                if (_battle.season.Value == 2)
                    room.Glass.SetActive(false);

                // спаун нижнего жучка
                enemies.Spawn(room.EnemiesParent, _antService, true);
                enemies.SetPositions(room.PosRight);
            }
            else
            {
                // спаун верхнего жучка
                enemies.Spawn(room.EnemiesParent, _antService, true);
                enemies.SetPositions(room.ExitFromCaveEnd, room.PosRight);
            }

            Squads.SetMaterial(enemies, materialUnits);
            enemies.AllDo(Unit.mi_Walk);
            room.Enemies = enemies;

            // Darkness FX
            room.SetMaterials(_battleRooms.Darkness,
                _battleSettings.splineMaskFog);
            room.Darkness = 1;

            room.Stock.gameObject.SetActive(false);
            room.Chest.CustomMaterialOverride.Add(
                _battleSettings.chestMaterialNormal,
                _battleSettings.chestMaterialFill);
        }
    }
}