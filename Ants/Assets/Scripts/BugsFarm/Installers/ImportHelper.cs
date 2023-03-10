using System;
using System.Globalization;
using UnityEngine;

namespace BugsFarm.Installers
{
    public class ImportHelper : MonoBehaviour
    {
        [SerializeField] private UnitFightSettingsInstaller unitFightSettingsInstaller;

        [ExposeMethodInEditor]
        private void Import()
        {
            var data = (TextAsset) Resources.Load("data");
            var text = data.text;
            var array = text.Split('\n');
            //UnityEngine.Debug.Log($"[{nameof(ImportHelper)}] {array.Length}");

            var index = 0;
            foreach (var line in array)
            {
                ParseLine(line, index);
                index++;
            }
        }

        private void ParseLine(string line, int level)
        {
            var array = line.Split(',');
            //UnityEngine.Debug.Log($"[{nameof(ImportHelper)}] {array.Length}");

            var index = 0;
            foreach (var item in array)
            {
                var type = AntType.None;
                switch (index)
                {
                    case 0:
                    case 8:
                        type = AntType.Worker;
                        break;
                    case 1:
                    case 9:
                        type = AntType.Pikeman;
                        break;
                    case 2:
                    case 10:
                        type = AntType.Archer;
                        break;
                    case 3:
                    case 11:
                        type = AntType.Snail;
                        break;
                    case 4:
                    case 12:
                        type = AntType.Spider;
                        break;
                    case 5:
                    case 13:
                        type = AntType.Worm;
                        break;
                    case 6:
                    case 14:
                        type = AntType.PotatoBug;
                        break;
                    case 7:
                    case 15:
                        type = AntType.Cockroach;
                        break;
                }

                var unitItem = unitFightSettingsInstaller.unitFight.units[type];
                var unitLevel = unitItem.levels[level];
                if (index <= 7)
                    unitLevel.hp = float.Parse(item);
                else
                    try
                    {
                        unitLevel.attack = float.Parse(item, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.Log($"[{nameof(ImportHelper)}] {item}");
                        throw;
                    }
                    

                index++;
            }
        }
    }
}