using System;
using UnityEngine;
using Zenject;

namespace BugsFarm.Installers
{
    [CreateAssetMenu(menuName = "Config/UnitViewSettings", fileName = "UnitViewSettings")]
    public class UnitViewSettingsInstaller : ScriptableObjectInstaller<BattleSettingsInstaller>
    {
        public UnitViewSettings unitView;

        [Serializable]
        public class UnitViewSettings
        {
            public int startRoom = 0;
            public Data_Fight.UnitViewConfig units;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(unitView);
        }

        [Serializable]
        public class UnitViewItem
        {
            public AnimationsConfig animationScale;
            public float walkSpeed = 1.2f;
            public float moveSpeed = 2f;
            public float moveToStartSpeed = 1.8f;
            public float attackDelay = 0.2f;
            public Sprite avatar;
        }

        [Serializable]
        public class AnimationsConfig
        {
            public float walk = 1f;
            public float run = 1f;
            public float idle = 1f;
            public float attack = 1f;
            public float death = 1f;
        }
    }
}