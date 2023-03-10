using System.IO;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.Game
{
    public static class GameInit
    {
        //public static FarmServices FarmServices { get; private set; }
        public static string SaveFilePath => SavingSystem.DefaultPath(SaveFileName);
        public static string SaveFilePath_AtLoad => SavingSystem.DefaultPath(SaveFileName_AtLoad);
        public static string SaveFilePath_Last => SavingSystem.DefaultPath(SaveFileName_Last);

        public const int SaveFormatVer = 70;
        private const string SaveFormatVer_Key = "SaveFormatVer";
        private const string SaveFileName = "save.dat";
        private const string SaveFileName_AtLoad = "save_AtLoad.dat";
        private const string SaveFileName_Last = "save_Last.dat";
        
        private static bool _initialized;
        private static MonoFactory _monoFactory;
        private static Data_Other _dataOther;

        public static void Init(Data_Other dataOther)
        {
            _dataOther = dataOther;
        }
        public static void Reset(bool startTutorial)
        {
            //_gameData.Reset();
            Clear();
            //FarmServices.Reset();
            Init(startTutorial);
            GameEvents.OnGameReset?.Invoke();

        }
        public static void Clear()
        {
            _initialized = false;

            //Keeper.Clear();
            Occupied.Clear();
            OccupiedPlaces.Clear();

            //_monoFactory.ResetAntType();
        }
        public static void Load()
        {
            Clear();
            _initialized = true;

            //SimulationOld.Instance.SetGameAge(_gameData.gameAge);
            //GameResources.Unpack(_gameData);
           // Keeper.Unpack(_gameData);
            //if (menu_Quests.Instance != null)
            //    menu_Quests.Instance.InitQuests(); // (!) BEFORE simulation
//
            Simulate();
            //menu_Quests.Instance.Refresh();
            Debug.LogError("SavingSystem : Loaded!");
        }
        public static void SaveExitTime()
        {
            //_gameData.exitTime = Tools.UtcNow();
        }
        public static void Simulate()
        {
            const double DEV_extraTime = 0; // + 24 * 60 * 60
            //SimulationOld.Instance.SimulateFrom(_gameData.exitTime - DEV_extraTime);
        }
        public static void CopySaveFile(string path)
        {
            if (File.Exists(SaveFilePath))
                File.Copy(SaveFilePath, path, true);
        }

        private static void Init(bool startTutorial)
        {
            _initialized = true;

            GameResources.Coins = _dataOther.Other.GameStart.Coins;
            GameResources.Crystals = _dataOther.Other.GameStart.Crystals;

            //SimulationOld.Instance.ResetGameAge();
            //Keeper.Init(_dayNight,_monoFactory);

            // if (startTutorial)
            //     Tutorial.Instance.StartTutor();
        }
        private static void SetVersion() => PlayerPrefs.SetInt(SaveFormatVer_Key, SaveFormatVer);
    }
}

