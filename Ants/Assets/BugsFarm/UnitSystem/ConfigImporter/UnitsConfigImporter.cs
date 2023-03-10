using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Services.StatsService;
using BugsFarm.UI;
using BugsFarm.UpgradeSystem;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.UnitSystem.ConfigImporter
{
    public class UnitsConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint = false;
        [ExposeMethodInEditor]
        private void UnitModelsConfigToJson()
        {
            Load("UnitModels", data =>
            {
                var config = new List<UnitModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var typeName = items[0];
                    var modelId = items[1];
                    var isFemale = bool.Parse(items[2]);
                    config.Add(new UnitModel
                    {
                        ModelID = modelId,
                        TypeName = typeName,
                        IsFemale = isFemale
                    });
                }
                ConfigHelper.Save(config.ToArray(),"UnitModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void UnitStatConfigToJson()
        {
            Load("UnitStatModels", data =>
            {
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var config = new Dictionary<string, UnitStatModel>(); 
                var statDecode = new Dictionary<string, string>
                {
                    {"Static", nameof(Stat)},
                    {"Modifiable", nameof(StatModifiable)},
                    {"Attribute", nameof(StatAttribute)},
                    {"Vitality", nameof(StatVital)},
                };
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var modelId = items[1];
                    if (!config.ContainsKey(modelId))
                    {
                        config.Add(modelId, new UnitStatModel{ModelID = modelId, Stats = new StatModel[0]});
                    }

                    var model = config[modelId];
                    var statType = statDecode[items[4]];
                    var param = items[3].Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                    var baseValue = float.Parse(param.Length > 1 ? param[1] : param[0], CultureInfo.InvariantCulture);
                    var initValue = float.Parse(param.Length > 1 ? param[0] : "0", CultureInfo.InvariantCulture);
                    model.Stats = model.Stats.Append(new StatModel
                    {
                        StatID = items[2],
                        StatType = statType,
                        BaseValue = baseValue,
                        InitValue = initValue,
                    }).ToArray();
                    config[modelId] = model;
                }
                
                ConfigHelper.Save(config.Values.ToArray(), "UnitStatModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void UnitShopItemsConfigToJson()
        {
            var config = new List<UnitShopItemModel>
            {
                new UnitShopItemModel {ModelID = "8", BuyLevel = 1, Price = new CurrencyModel{ModelID = "0", Count = 300}},
                new UnitShopItemModel {ModelID = "4", BuyLevel = 1, Price = new CurrencyModel{ModelID = "0", Count = 250}},
                new UnitShopItemModel {ModelID = "0", BuyLevel = 1, Price = new CurrencyModel{ModelID = "0", Count = 200 }},
                new UnitShopItemModel {ModelID = "5", BuyLevel = 4, Price = new CurrencyModel{ModelID = "0", Count = 500 }},
                new UnitShopItemModel {ModelID = "6", BuyLevel = 5, Price = new CurrencyModel{ModelID = "0", Count = 1300  }},
                new UnitShopItemModel {ModelID = "7", BuyLevel = 10, Price = new CurrencyModel{ModelID = "0", Count = 400 }},
                new UnitShopItemModel {ModelID = "2", BuyLevel = 12, Price = new CurrencyModel{ModelID = "0", Count = 2000 }},
                new UnitShopItemModel {ModelID = "3", BuyLevel = 16, Price = new CurrencyModel{ModelID = "0", Count = 2000  }},
                new UnitShopItemModel {ModelID = "1", BuyLevel = 18, Price = new CurrencyModel{ModelID = "0", Count = 1500  }},
            };
            
            ConfigHelper.Save(config.ToArray(),"UnitShopItemModels");
        }

        [ExposeMethodInEditor]
        private void UnitTasksConfigToJson()
        {
            var config = new List<UnitTasksModel>
            {
                new UnitTasksModel
                {
                    ModelID = "0",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(PatrolingTask),
                        nameof(TrainingArcherBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train,
                        UnitAssignTasks.Patrol
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "1",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(TrainingButterflyBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "2",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(TrainingHeavySquadBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "3",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(HospitalBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "4",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(PatrolingTask),
                        nameof(TrainingPikemanBootstrapTask),
                        nameof(PatrolBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train,
                        UnitAssignTasks.Patrol
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "5",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(TrainingHeavySquadBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "6",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(TrainingHeavySquadBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "7",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(SleepUnitBootstrapTask),
                        nameof(PatrolingTask),
                        nameof(TrainingSwordmanBootstrapTask),
                        nameof(PatrolBootstrapTask),

                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Train,
                        UnitAssignTasks.Patrol,
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "8",
                    IsAssignable = true,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(FallUnitTask),
                        nameof(DeathUnitBootstrapTask),
                        nameof(ConsumeUnitTask),
                        nameof(HungryRest),
                        nameof(AddFeedQueenTask),
                        nameof(SleepUnitBootstrapTask),
                        nameof(BuildingBootstrapTask),
                        nameof(GoldmineBootstrapTask),
                        nameof(GardenCareTask),
                        nameof(AddFightStockTask),
                        nameof(AddDumpsterTask),
                        nameof(AddGrabageStockTask),
                        nameof(AddMudStockTask),
                        nameof(GetRoomDigTask),
                        nameof(AddVineBuildTask),
                        nameof(AddHerbsStockTask),
                        nameof(AddFoodStockTask),
                        nameof(TrainingSwordmanBootstrapTask),
                        nameof(TrainingHeavySquadBootstrapTask),
                        nameof(TrainingPikemanBootstrapTask),
                        nameof(RestTask),
                    },
                    AssignTasks = new []
                    {
                        UnitAssignTasks.WakeUp,
                        UnitAssignTasks.Goldmine,
                        UnitAssignTasks.Dig,
                        UnitAssignTasks.Train
                    }
                },
                new UnitTasksModel
                {
                    ModelID = "9",
                    IsAssignable = false,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(AntLarvaGrowthTask),
                        
                        nameof(RestTask),
                    },
                    AssignTasks = Array.Empty<string>()
                },
                new UnitTasksModel
                {
                    ModelID = "10",
                    IsAssignable = false,
                    Tasks = new[]
                    {
                        nameof(SpawnUnitTask),
                        nameof(SpawnFromAlphaTask),
                        nameof(AddDealerFarmTask),
                    },
                    AssignTasks = Array.Empty<string>()
                },
            };
            
            ConfigHelper.Save(config.ToArray(),"UnitTasksModels", _prettyPrint);
        }

        [ExposeMethodInEditor]
        private void UnitUpgradeConfigToJson()
        {
             Load("UnitUpgradeModels", data =>
            {
                const string statkey = "stat_";
                const string priceKey = "price";
                var config = new List<UnitUpgradeModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                var ugrades = new Dictionary<string, Dictionary<string, List<float>>>(); // модель, (стат, значения обновления)
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var modelId = items[0];
                    var key = items[1];
                    items.RemoveRange(0,2); // cleanup for foreach
                    if (key != priceKey)
                    {
                        key = statkey + key;
                    }
                    if (!ugrades.ContainsKey(modelId))
                    {
                        ugrades.Add(modelId, new Dictionary<string, List<float>>());
                    }

                    if (!ugrades[modelId].ContainsKey(key))
                    {
                        ugrades[modelId].Add(key, new List<float>());
                    }

                    var values = ugrades[modelId][key];
                    foreach (var item in items)
                    {
                        if (item == "-")
                        {
                            values.Add(-1);
                            continue;
                        }
                        if (!float.TryParse(item, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                        {
                            Debug.LogError($"{this} : value does not match to float : {value}");
                        }
                        values.Add(value);
                    }
                }

                foreach (var upgrade in ugrades)
                {
                    var modelId = upgrade.Key;
                    var prices = upgrade.Value[priceKey]; // cashe prices
                    var stats = upgrade.Value;
                    stats.Remove(priceKey); // remove prices from stats
                    config.Add(new UnitUpgradeModel
                    {
                        ModelID = modelId,
                        Levels = prices.Select((price, level) => new UpgradeLevelModel
                        {
                            Price = new CurrencyModel{ModelID = "0", Count = (int)price}, 
                            Level = level + 2, 
                            Stats = stats.Where(x=>x.Value[level] >= 0).Select(statUpgrade => new UpgradeStatModel
                            {
                                StatID = statUpgrade.Key, 
                                Value = statUpgrade.Value[level]
                            }).ToArray()
                        }).ToDictionary(x=>x.Level)
                    });
                }

                ConfigHelper.Save(config.ToArray(), "UnitUpgradeModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void UnitsBirthConfigToJson()
        {
            Load("UnitBirthModels", data =>
            {
                var config = new Dictionary<string, UnitBirthModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // remove headers
                foreach (var line in lines)
                {
                    var items = line.Split(',');
                    var modelID = items[0];
                    var larvaIdObj = items[1];
                    var trackingIDObj = items[2];
                    var birthIDObj = items[3];
                    if (!config.ContainsKey(modelID))
                    {
                        var birthModel = new UnitBirthModel
                        {
                            ModelID = modelID, BirthUnitsModelID = new string[0], TrackingUnitsModelID = new string[0]
                        };
                        config.Add(modelID, birthModel);
                    }

                    var model = config[modelID];
                    if (!string.IsNullOrEmpty(larvaIdObj))
                    {
                        model.LarvaModelID = larvaIdObj;
                    }

                    if (!string.IsNullOrEmpty(trackingIDObj))
                    {
                        model.TrackingUnitsModelID = model.TrackingUnitsModelID.Append(trackingIDObj).ToArray();
                    }

                    if (!string.IsNullOrEmpty(birthIDObj))
                    {
                        model.BirthUnitsModelID = model.BirthUnitsModelID.Append(birthIDObj).ToArray();
                    }

                    config[modelID] = model;
                }

                ConfigHelper.Save(config.Values.ToArray(), "UnitBirthModels");
            });
        }

        [ExposeMethodInEditor]
        private void UnitsStageModelsConfigToJson()
        {
            var config = new List<UnitStageModel>
            {
                new UnitStageModel
                {
                    ModelID = "9",
                    Path = "Sprites/Units/AntLarva/",
                    Count = 3
                },
            };
            
            ConfigHelper.Save(config.ToArray(),"UnitStageModels");
        }
    }
}