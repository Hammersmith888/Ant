using System;
using UnityEngine;
using Zenject;

namespace BugsFarm.Installers
{
    [CreateAssetMenu(menuName = "Config/UnitFightSettings", fileName = "UnitFightSettings")]
    public class UnitFightSettingsInstaller : ScriptableObjectInstaller<UnitFightSettingsInstaller>
    {
        public UnitFightSettings unitFight;
        
        
        [Serializable]
        public class UnitFightSettings
        {

            public Data_Fight.UnitFightConfig units;
        }    

        public override void InstallBindings()
        {
            Container.BindInstance(unitFight);
        }

        [Serializable]
        public class UnitFightItem
        {
            public UnitFightLevelItem[] levels;
        }

        [Serializable]
        public class UnitFightLevelItem
        {
            public float hp;
            public float attack;
        }
    }
}