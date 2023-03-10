using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.Config
{
    [Serializable]
    public struct UpgradeLevel
    {
        public int price;
        public int minutes;
        public float param1;
        public float param2;
        public float param3;

        public void SetPrice(int price) => this.price = price;

        public UpgradeLevel(ObjType type, int subType, Dictionary<string, float> fb)
        {
            this = default;

            var key = (type, subType);

            price = Mathf.RoundToInt(fb[FB_Packer.UpgradePrice]);
            minutes = Mathf.RoundToInt(fb[FB_Packer.UpgradeTime]);

            foreach (var pair in fb)
                if (
                    pair.Key != FB_Packer.UpgradePrice &&
                    pair.Key != FB_Packer.UpgradeTime
                    )
                    SetParam(FB_Packer.ObjParams[key][pair.Key], pair.Value);
        }

        private void SetParam(int i, float value)
        {
            switch (i)
            {
                case 0:
                    param1 = value;
                    break;
                case 1:
                    param2 = value;
                    break;
                case 2:
                    param3 = value;
                    break;
            }
        }

        public float[] GetParams()
        {
            return new[] {param1, param2, param3};
        }
    }

    [CreateAssetMenu(
                        fileName = ScrObjs.CfgUpgrades,
                        menuName = ScrObjs.folder + ScrObjs.CfgUpgrades,
                        order = ScrObjs.CfgUpgrades_i
                    )]
    public class CfgUpgrades : ScriptableObject
    {
        public bool showParam1;
        public bool showParam2;
        public bool showParam3;
        public string labelParam1;
        public string labelParam2;
        public string labelParam3;

        public List<UpgradeLevel> levels;
        public bool[] GetParamsAcess()
        {
            return new[] {showParam1, showParam2, showParam3};
        }
        public string[] GetParamsLables()
        {
            return new[] {labelParam1, labelParam2, labelParam3};
        }
    }
}