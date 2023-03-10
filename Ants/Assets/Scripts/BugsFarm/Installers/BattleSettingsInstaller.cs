using System;
using BugsFarm.Views.Fight.Ui;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BugsFarm.Installers
{
    [CreateAssetMenu(menuName = "Config/BattleSettings", fileName = "BattleSettings")]
    public class BattleSettingsInstaller : ScriptableObjectInstaller<BattleSettingsInstaller>
    {
        public BattleSettings battle;
        public AntSettings ant;
        public AntDatabase antDatabase;

        [Serializable]
        public class BattleSettings
        {
            public float openChestDelay = 3f;
            public float cameraOffset = 4.5f;
            public float cameraSpeed = 2f;
            public float cameraMoveDelay = 1f;
            public Material chestMaterialNormal;
            public Material chestMaterialFill;
            public int spawnRoomsCount = 3;
            public float[] spawnRoomOffset = {4.6f, 10f};
            public Color fogColor;
            public Material splineMaskFog;
            public Material splineMaskNoFog;
            public Material materialGround;

            public Color foodBarPositive;
            public Color foodBarNegative;

            public Color colorDamagePlayer;
            public Color colorDamageEnemy;
            public Ease damageEase = Ease.OutQuint;

            public float failPanelStartMul = .5f;
            public float failPanelDelay = .3f;
            public float failPanelDelayStep = .05f;
            public float failPanelDuration = .5f;
            public Ease failPanelEase = Ease.OutQuint;

            public float lightOnDuration = 1.5f;
            public float lightOnDelay = 1f;
            public float lightOffDuration = 1.5f;

            public float unitSpeedMul = .4f;

            public float arrowsGravity = -14f;
            public float arrowsSpeed = 11f;

            public float damageOffsetY0 = .5f;
            public float damageOffsetY1 = 2f;
            public float damageRandomShiftX = .25f;
            public float damageRandomShiftY = .25f;

            public float patrolBorderRight = 3.5f;
            public float patrolBorderLeft = -1f;
            public Vector2 patrolWait = new Vector2(0.2f, 0.5f);

            public float armorDuration = 5f;
            public float attackDuration = 5f;
            public float attackRate = 2f;

            public SquadSelectItemView squadSelectItemPrefab;
        }

        [Serializable]
        public class AntSettings
        {
            public float antMoveDeltaStop = .1f;
            public float enemyPositionOffset = .85f;

            public float fadeInDelay = 2f;
            public float fadeInDuration = 1f;
            public float fadeOutDistance = 2.5f;
            public float fadeOutDistanceSeason1 = 2.5f;
            public float fadeOutDistanceSeason2 = 2.5f;
            public float fadeOutDistanceRetreat = 2.5f;

            public float antStartOffset = .6f;

            public float findAttackPositionOffset = 1.2f;
            public float findAttackPositionOffsetCoef = .8f;

            public float animationDamageDuration = 0.5f;

            public float animationScaleExitFromCave = 1.5f;

            public float arrowSpeed = 1f;
            public float archerShootDelay = 0.5f;
            public float archerIdleDelay = 1f;
            public AnimationCurve arrowCurve;
            public float arrowMaxDistance;
            public float arrowFadeDelay = 1f;
            public float arrowFadeDuration = .5f;

            public Vector2 archerShootTimeNoise = new Vector2(.2f, .8f);
            public float arrowGravity;
            public float arrowRotationSpeed;
        }

        [Serializable]
        public class AntDatabase
        {
            public AntVo[] items;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(battle);
            Container.BindInstance(ant);
            Container.BindInstance(antDatabase);
        }
    }

    [Serializable]
    public class AntVo
    {
        public AntType type;
        public Material materialOriginal;
        public Material materialOutline;
        public Material materialFill;
        public Material materialArmor;
        public Material materialAttack;
        public float attackAnimationTime = 1f;
        public float stopTime = 1f;
        public float patrolDistance = 2f;
        public float attackAnimationScale = 1f;
        public float attackDistance = .85f;
    }
}