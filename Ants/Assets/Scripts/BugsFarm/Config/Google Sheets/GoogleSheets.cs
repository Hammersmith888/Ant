using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BugsFarm.Config;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using GoogleSheetsToUnity;
using UnityEngine;
using UnityEngine.Events;

public struct WorksheetData
{
    public string spreadsheetId;
    public string worksheetName;
    public string startCell;
    public string endCell;
    public bool containsMergedCells;
}

public class GoogleSheets : MonoBehaviour
{
    public bool loadFarmToBuffer;
    public bool readFarmBuffer;
    public bool loadFightUnitsAndDungeons;
    public bool loadFightXPPrices;

    public GoogleSheetBuffer GoogleSheetFarmBuffer;

    GoogleSheetBuffer.TCells Cells => GoogleSheetFarmBuffer.Cells;

    readonly WorksheetData farm = new WorksheetData
    {
        spreadsheetId = "147u9uekFUrvTAYrRd4rcAnu3FQf1W5dgfGHKMAqsqeA",
        worksheetName = "Значения",
        startCell = "B2",
        endCell = "N137",
        containsMergedCells = true
    };

    readonly WorksheetData fightUnits = new WorksheetData
    {
        spreadsheetId = "10smdysKx684xKNBEmHaDRCXLqyuJU5o6mK8eA1OXHAA",
        worksheetName = "Юниты",
        startCell = "B6",
        endCell = "M14",
        containsMergedCells = true
    };

    readonly WorksheetData fightDungeons = new WorksheetData
    {
        spreadsheetId = "10smdysKx684xKNBEmHaDRCXLqyuJU5o6mK8eA1OXHAA",
        worksheetName = "Этажи",
        startCell = "B2",
        endCell = "K101",
        containsMergedCells = true
    };

    readonly WorksheetData fightXPprices = new WorksheetData
    {
        spreadsheetId = "10smdysKx684xKNBEmHaDRCXLqyuJU5o6mK8eA1OXHAA",
        worksheetName = "Цены XP",
        startCell = "C8",
        endCell = "G12",
        containsMergedCells = true
    };

    BiDict<AntType, string> _codes = new BiDict<AntType, string>();

    //void Start()
    //{
    //	if (loadFarmToBuffer)
    //		LoadGoogleSheet( farm, cb_SaveToBuffer );

    //	if (readFarmBuffer)
    //		ReadFarmBuffer();

    //	if (loadFightUnitsAndDungeons)
    //		LoadGoogleSheet( fightUnits, cb_ReadUnits );

    //	if (loadFightXPPrices)
    //		LoadGoogleSheet( fightXPprices, cb_ReadXPprices );
    //}

    void ReadFarmBuffer()
    {
        ReadInsects();
        ReadFood();
        ReadDecorations();
        ReadObject(ObjType.str_Goldmine, 33, "F", "E");
        ReadObject(ObjType.Food, (int) FoodType.Garden, 40, "E");

        ReadObject(ObjType.str_Pikes, 47);
        ReadObject(ObjType.str_ArrowTarget, 47);

        ReadObject(ObjType.Bowl, 54, "E");
        ReadObject(ObjType.Food, (int) FoodType.DumpsterStock, 61);

        ReadOther();
        ReadWorker();
        ReadRooms();
        ReadQuests();
    }

    void ReadQuests()
    {
        CfgOther cfgOther = null; //Data_Other.Instance.Other;

        for (int row = 25;; row++)
        {
            if (
                !Cells.TryGetValue($"L{row}", out var questStr) ||
                !questStr.Any()
                )
                break;

            if (!FB_Packer.Quests.TryGetValue(questStr, out QuestID questID))
                continue;

            TryParse(row, "M", out int reward);

            CfgQuest cfgQuest = cfgOther.Quests[questID];
            cfgQuest.reward = reward;

            FB_Packer.SetDirty(cfgQuest);
        }

        Debug.Log("Quests loaded");
    }

    void ReadRooms()
    {
        CfgOther cfgOther = null; //Data_Other.Instance.Other;

        for (int row = 14;; row++)
        {
            if (
                !Cells.TryGetValue($"L{row}", out var roomNumStr) ||
                !roomNumStr.Any()
                )
                break;

            int.TryParse(roomNumStr, out int roomNum);

            TryParse(row, "M", out int reward);
            TryParse(row, "N", out int digAmount);

            FB_CfgRoom room = cfgOther.Rooms[roomNum];
            CfgChest chest = cfgOther.Chests[roomNum];
            chest.Coins = reward;
            room.AmountToWork = digAmount;

            cfgOther.Rooms[roomNum] = room;
            cfgOther.Chests[roomNum] = chest;
        }

        FB_Packer.SetDirty(cfgOther);

        Debug.Log("Rooms loaded");
    }

    void ReadWorker()
    {
        CfgAnt cfgWorker = Data_Ants.Instance.GetData(AntType.Worker);

        for (int row = 3;; row++)
        {
            if (
                !Cells.TryGetValue($"L{row}", out var taskName) ||
                !taskName.Any()
                )
                break;

            if (!FB_Packer.Tasks.TryGetValue(taskName, out StateAnt task))
                continue;

            TryParse(row, "M", out int time);

            //cfgWorker.TaskTime[ task ]		= time;
        }

        FB_Packer.SetDirty(cfgWorker);

        Debug.Log("Worker loaded");
    }

    void ReadOther()
    {
        CfgAnt cfgWorker = Data_Ants.Instance.GetData(AntType.Worker);
        CfgOther cfgOther = null; //Data_Other.Instance.Other;
        FB_CfgGameStart gameStart = cfgOther.GameStart;
        FB_CfgQueen queen = cfgOther.Queen;

        TryParse(2, "D", out gameStart.Coins);
        TryParse(3, "D", out queen.PregnancyHours);
        TryParse(4, "D", out float digAmount);

        cfgOther.GameStart = gameStart;
        cfgOther.Queen = queen;
        cfgWorker.other[OtherAntParams.WorkAmount] = digAmount;

        FB_Packer.SetDirty(cfgOther);
        FB_Packer.SetDirty(cfgWorker);

        Debug.Log("Other loaded");
    }

    void LoadGoogleSheet(WorksheetData worksheetData, UnityAction<GstuSpreadSheet> callback)
    {
        SpreadsheetManager.Read(
                                new GSTU_Search(
                                                worksheetData.spreadsheetId,
                                                worksheetData.worksheetName,
                                                worksheetData.startCell,
                                                worksheetData.endCell
                                               ),
                                callback,
                                worksheetData.containsMergedCells
                               );
    }

    void cb_ReadXPprices(GstuSpreadSheet sheet)
    {
        for (int row = 8; row <= 12; row++)
        {
            if (
                !sheet.Cells.TryGetValue($"C{row}", out var gstuCell) ||
                !gstuCell.value.Any()
                )
                continue;

            if (!FB_Packer.Units.TryGetValue(gstuCell.value, out AntType type))
                continue;

            CfgUnit cfgUnit = Data_Fight.Instance.units[type];

            TryParse(sheet, row, "G", out cfgUnit.XPprice);

            FB_Packer.SetDirty(cfgUnit);
        }

        Debug.Log("XP prices loaded");
    }

    void cb_ReadUnits(GstuSpreadSheet sheet)
    {
        _codes.Clear();

        for (int row = 6; row <= 14; row++)
        {
            if (
                !sheet.Cells.TryGetValue($"B{row}", out var gstuCell) ||
                !gstuCell.value.Any()
                )
                continue;

            if (!FB_Packer.Units.TryGetValue(gstuCell.value, out AntType type))
                continue;

            CfgUnit cfgUnit = Data_Fight.Instance.units[type];
            _codes[type] = GetCellValue(sheet, row, "C");

            TryParse(sheet, row, "E", out cfgUnit.Food);
            TryParse(sheet, row, "F", out int HP_Min);
            TryParse(sheet, row, "G", out int HP_Max);
            TryParse(sheet, row, "H", out cfgUnit.HP_C);
            TryParse(sheet, row, "I", out int Attack_Min);
            TryParse(sheet, row, "J", out int Attack_Max);
            TryParse(sheet, row, "K", out cfgUnit.Attack_C);
            TryParse(sheet, row, "L", out cfgUnit.Speed);
            TryParse(sheet, row, "M", out cfgUnit.Cooldown);

            cfgUnit.HP_Range = new Vector2Int(HP_Min, HP_Max);
            cfgUnit.Attack_Range = new Vector2Int(Attack_Min, Attack_Max);

            FB_Packer.SetDirty(cfgUnit);
        }

        Debug.Log("Units loaded");

        LoadGoogleSheet(fightDungeons, cb_Dungeons);
    }

    void cb_Dungeons(GstuSpreadSheet sheet)
    {
        CfgEnemies cfgEnemies = Data_Fight.Instance.enemies;

        //cfgEnemies.rooms.Clear();

        for (int row = 2; row <= 101; row++)
        {
            CfgRoomEnemies room = new CfgRoomEnemies();
            room.enemies = new List<CfgEnemy>();
            TryParse(sheet, row, "C", out room.reward_Coins);
            //cfgEnemies.rooms.Add( room );

            for (char c = 'D'; c <= 'K'; c++)
            {
                string unit = GetCellValue(sheet, row, c.ToString());

                if (!unit.Any())
                    continue;

                string code = Regex.Match(unit, @"[^\d\s]+").Groups[0].Value;
                string levelStr = Regex.Match(unit, @"\d+").Groups[0].Value;
                AntType type = _codes[code];

                int.TryParse(levelStr, out int level);

                AddEnemy(room, type, 1, level);
            }
        }

        FB_Packer.SetDirty(cfgEnemies);

        Debug.Log("Dungeons loaded");
    }

    void AddEnemy(CfgRoomEnemies room, AntType type, int count, int level)
    {
        room.enemies.AddRange(
                              Enumerable.Repeat(
                                                new CfgEnemy {type = type, level = level},
                                                count
                                               )
                             );
    }

    void cb_SaveToBuffer(GstuSpreadSheet sheet)
    {
        GoogleSheetFarmBuffer.Cells.Clear();

        for (char col = 'B'; col <= 'N'; col++)
        for (int row = 2; row <= 137; row++)
        {
            string value = string.Empty;
            string cell = $"{col}{row}";

            if (sheet.Cells.TryGetValue(cell, out var gstuCell))
                value = gstuCell.value;

            GoogleSheetFarmBuffer.Cells[cell] = value;
        }

        FB_Packer.SetDirty(GoogleSheetFarmBuffer);

        Debug.Log("Google Sheet loaded to Buffer");
    }

    void ReadInsects()
    {
        for (int row = 7;; row++)
        {
            if (
                !Cells.TryGetValue($"C{row}", out var strAntType) ||
                !strAntType.Any()
                )
                break;

            if (!FB_Packer.Units.TryGetValue(strAntType, out AntType type))
                continue;

            CfgAnt cfgAnt = Data_Ants.Instance.GetData(type);
            var consumption = cfgAnt.consumption;

            TryParse(row, "D", out cfgAnt.price);                 // Price
            TryParse(row, "E", out float sleepHours);             // Sleep time
            TryParse(row, "F", out consumption.FoodNeedTime);     // Eats each ... minutes
            TryParse(row, "G", out consumption.WaterNeedTime);    // Drinks each ... minutes
            TryParse(row, "H", out consumption.FoodConsumption);  // Food consumption
            TryParse(row, "I", out consumption.WaterConsumption); // Water consumption

            cfgAnt.consumption = consumption;
            //cfgAnt.TaskTime[ StateAnt.Sleep ]		= Mathf.RoundToInt( sleepHours * 60 );

            FB_Packer.SetDirty(cfgAnt);
        }

        Debug.Log("Insects loaded");
    }

    void ReadFood()
    {
        for (int row = 14;; row++)
        {
            if (
                !Cells.TryGetValue($"C{row}", out var foodName) ||
                !foodName.Any()
                )
                break;

            if (!BiDicts.Objects.TryGetValue(foodName, out var typeSubtype))
                continue;

            var cfgFood = (CfgFood) Data_Objects.Instance.GetData(typeSubtype.Item1,typeSubtype.Item2);

            TryParse(row, "D", out int price);    // Price
            TryParse(row, "E", out int quantity); // Quantity

            cfgFood.SetPrice(price); // Price

            // Quantity
            for (int i = 0; i < cfgFood.stages.Length; i++)
                cfgFood.stages[i].SetQuantity(quantity * (1 - (float) i / cfgFood.stages.Length));

            FB_Packer.SetDirty(cfgFood);
        }

        Debug.Log("Food loaded");
    }

    void ReadDecorations()
    {
        for (int row = 23;; row++)
        {
            if (
                !Cells.TryGetValue($"C{row}", out var name) ||
                !name.Any()
                )
                break;

            if (!BiDicts.Objects.TryGetValue(name, out var typeSubtype))
                continue;

            CfgObject cfgObject = Data_Objects.Instance.GetData(typeSubtype.Item1,typeSubtype.Item2);

            TryParse(row, "D", out int price); // Price

            cfgObject.SetPrice(price); // Price

            FB_Packer.SetDirty(cfgObject);
        }

        Debug.Log("Decorations loaded");
    }

    void ReadObject(ObjType type, int startRow, string colParam1 = null, string colParam2 = null)
        =>
            ReadObject(type, 0, startRow, colParam1, colParam2);

    void ReadObject(ObjType type, int subType, int startRow, string colParam1 = null, string colParam2 = null)
    {
        CfgObject cfgObject = Data_Objects.Instance.GetData(type, subType);

        for (int row = startRow;; row++)
        {
            if (
                !Cells.TryGetValue($"C{row}", out var levelStr) ||
                !levelStr.Any()
                )
                break;

            int.TryParse(levelStr, out int level);

            UpgradeLevel ul = cfgObject.upgrades.levels[level - 1];

            TryParse(row, "D", out ul.price); // Price

            if (colParam1 != null) TryParse(row, colParam1, out ul.param1); // Param1
            if (colParam2 != null) TryParse(row, colParam2, out ul.param2); // Param2

            cfgObject.upgrades.levels[level - 1] = ul;

            FB_Packer.SetDirty(cfgObject.upgrades);
        }

        Debug.Log($"{cfgObject.wiki.Header} loaded");
    }

    bool TryParse(int row, string column, out float result)
    {
        string value = GetCellValue(row, column);
        value = value.Replace(",", string.Empty);

        return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    bool TryParse(int row, string column, out int result)
    {
        string value = GetCellValue(row, column);
        value = value.Replace(",", string.Empty);

        return int.TryParse(value, out result);
    }

    string GetCellValue(int row, string column)
        =>
            Cells.TryGetValue($"{column}{row}", out var cell) ? cell : string.Empty;

    bool TryParse(GstuSpreadSheet sheet, int row, string column, out float result)
    {
        string value = GetCellValue(sheet, row, column);
        value = value.Replace(",", string.Empty);

        return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    bool TryParse(GstuSpreadSheet sheet, int row, string column, out int result)
    {
        string value = GetCellValue(sheet, row, column);
        value = value.Replace(",", string.Empty);

        return int.TryParse(value, out result);
    }

    string GetCellValue(GstuSpreadSheet sheet, int row, string column)
        =>
            sheet.Cells.TryGetValue($"{column}{row}", out var cell) ? cell.value : string.Empty;
}