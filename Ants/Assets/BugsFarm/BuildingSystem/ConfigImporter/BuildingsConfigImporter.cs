using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.CurrencySystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Services.StatsService;
using BugsFarm.UpgradeSystem;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.BuildingSystem.ConfigImporter
{
    public class BuildingsConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint = false;
        [ExposeMethodInEditor]
        private void BuildingModelsConfigToJson()
        {
            Load("BuildingModels", data =>
            {
                var config = new List<BuildingModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var typeName = items[0];
                    var modelId = items[1];
                    var typeId = int.Parse(items[2]);
                    var overlap = bool.Parse(items[3]);
                    config.Add(new BuildingModel
                    {
                        CanOverlap = overlap,
                        ModelID = modelId,
                        TypeID = typeId,
                        TypeName = typeName
                    });
                }
                ConfigHelper.Save(config.ToArray(), "BuildingModels",_prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void HospitalSlotModelsConfigToJson()
        {
            Load("HospitalSlotModels", data =>
            {
                var config = new List<HospitalSlotModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    config.Add(new HospitalSlotModel
                    {
                        ModelId = items[1],
                        Price = new CurrencyModel
                        {
                            ModelID = "0",
                            Count = int.Parse(items[2])
                        }
                    });
                }
                ConfigHelper.Save(config.ToArray(), "HospitalSlotModels",_prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void PrisonSlotModelsConfigToJson()
        {
            Load("PrisonSlotModels", data =>
            {
                var config = new List<PrisonSlotModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    config.Add(new PrisonSlotModel
                    {
                        ModelId = items[1],
                        AssetPath = items[2]
                    });
                }
                ConfigHelper.Save(config.ToArray(), "PrisonSlotModels",_prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void BuildingInfoParamsConfigToJson()
        {
            Load("BuildingInfoParamsModels", data =>
            {
                var config = new Dictionary<string, BuildingInfoParamsModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.None).ToList();
                    var modelId = items[1];
                    var statId = items[2];
                    var localizationId = items[3];
                    var iconName = items[4];
                    var statFormat = items[5];
                    if (!config.ContainsKey(modelId))
                    {
                        config.Add(modelId, new BuildingInfoParamsModel{ModelId = modelId});
                    }

                    var model = config[modelId];
                    model.Items = model.Items ?? new Dictionary<string, BuildingInfoParamModel>();
                    model.Items.Add(statId, new BuildingInfoParamModel
                    {
                        StatId = statId,
                        FormatId = statFormat,
                        LocalizationId = localizationId,
                        IconName = iconName
                    });
                    config[modelId] = model;
                }
                ConfigHelper.Save(config.Values.ToArray(), "BuildingInfoParamsModels",_prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void BuildingStageModelsConfigToJson()
        {
            var config = new[]
            {
                new BuildingStageModel
                {
                    ModelID = "0",
                    Path = "Sprites/Food/FlaxSeeds/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "1",
                    Path = "Sprites/Food/Huzelnut/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "2",
                    Path = "Sprites/Food/Apple/",
                    Count = 4
                },
                new BuildingStageModel
                {
                    ModelID = "3",
                    Path = "Sprites/Food/SunflowerSeeds/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "4",
                    Path = "Sprites/Food/Wheat/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "5",
                    Path = "Sprites/Food/Blackberry/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "6",
                    Path = "Sprites/Food/Grapes/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "7",
                    Path = "Sprites/Food/Sugar/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "8",
                    Path = "Sprites/Food/Corn/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "9",
                    Path = "Sprites/Food/Chocolate/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "10",
                    Path = "Sprites/Food/Meat/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "11",
                    Path = "Sprites/Food/Potato/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "12",
                    Path = "Sprites/Food/Cabbage/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "13",
                    Path = "Sprites/Food/Mushroom/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "14",
                    Path = "Sprites/Food/Burger/",
                    Count = 5
                },
                new BuildingStageModel
                {
                    ModelID = "15",
                    Path = "Sprites/Food/Buckwheat/",
                    Count = 5
                },

                // Dumpster
                new BuildingStageModel
                {
                    ModelID = "40",
                    Path = "Sprites/Buildings/Dumpster/",
                    Count = 4
                },

                // FoodStock
                new BuildingStageModel
                {
                    ModelID = "42",
                    Path = "Sprites/Buildings/FoodStock/",
                    Count = 8
                },

                // Garden
                new BuildingStageModel
                {
                    ModelID = "43",
                    Path = "Spine",
                    Count = 3
                },

                // GrabageStock
                new BuildingStageModel
                {
                    ModelID = "45",
                    Path = "Sprites/Buildings/GrabageStock/",
                    Count = 4
                },

                // HerbsStock
                new BuildingStageModel
                {
                    ModelID = "47",
                    Path = "Sprites/Buildings/HerbsStock/",
                    Count = 4
                },

                // MudStock
                new BuildingStageModel
                {
                    ModelID = "49",
                    Path = "Sprites/Buildings/MudStock/",
                    Count = 5
                },

                // AntHill
                new BuildingStageModel
                {
                    ModelID = "59",
                    Path = "Sprites/Buildings/AntHill/",
                    Count = 3
                },
            };
            ConfigHelper.Save(config, "BuildingStageModels",_prettyPrint);
        }

        [ExposeMethodInEditor]
        private void BuildingParamsConfigToJson()
        {
            Load("BuildingParamModels", data =>
            {
                var config = new Dictionary<string, BuildingParamModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var paramId = items[0];
                    var modelId = items[2];

                    if (!config.ContainsKey(modelId))
                    {
                        config.Add(modelId, new BuildingParamModel
                        {
                            ModelID = modelId, 
                            Params = new string[0]
                        });
                    }

                    var model = config[modelId];
                    model.Params = model.Params.Append(paramId).ToArray();
                    config[modelId] = model;
                }
                ConfigHelper.Save(config.Values.ToArray(), "BuildingParamModels", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void BuildingRestockConfigToJson()
        {
            Load("BuildingRestockModels", data =>
            {
                var config = new List<BuildingRestockModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var precents = items[4].Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    config.Add(new BuildingRestockModel
                    {
                        ModelId = items[1], 
                        ItemId = items[2],
                        Price = new CurrencyModel{ModelID = "0", Count = int.Parse(items[3])},
                        MinPrecent = int.Parse(precents[0]),
                        MaxPrecent = int.Parse(precents[1]),
                    });
                }
                ConfigHelper.Save(config.ToArray(), "BuildingRestockModels", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void BuildingSpeedupConfigToJson()
        {
            Load("BuildingSpeedupModels", data =>
            {
                var config = new List<BuildingSpeedupModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    config.Add(new BuildingSpeedupModel
                    {
                        ModelId = items[1], 
                        Price = new CurrencyModel{ModelID = "0", Count = int.Parse(items[2])},
                    });
                }
                ConfigHelper.Save(config.ToArray(), "BuildingSpeedupModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void BuildingShopItemsConfigToJson()
        {
            Load("BuildingShopItemModels", data =>
            {
                var config = new Dictionary<string, BuildingShopItemModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var typeID = items[0];
                    var modelId = items[2];
                    var currencyCount = items[3];
                    items.RemoveRange(0,4); // cleanup befor read levels
                    if (!int.TryParse(currencyCount, out var currencyValue)) { }

                    if (!config.ContainsKey(modelId))
                    {
                        config.Add(modelId, new BuildingShopItemModel
                        {
                            TypeId = int.Parse(typeID),
                            ModelId = modelId,
                            Price = new CurrencyModel
                            {
                                ModelID = "0",
                                Count = currencyValue
                            },
                            Levels = items
                                .Select(item => item.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries))
                                .Select(param => new ShopItemLevelModel
                                {
                                    Level = int.Parse(param[0]),
                                    Count = int.Parse(param[1])
                                }).ToDictionary(x=>x.Level)
                        });
                    }
                }
                ConfigHelper.Save(config.Values.ToArray(), "BuildingShopItemModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void AntHillTaskConfigToJson()
        {
            Load("AntHillTasks", data =>
            {
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var configs = new List<AntHillTaskModel>(); 
                lines.RemoveAt(0);
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();

                    var taskModel = new AntHillTaskModel()
                    {
                        TaskID = items[1],
                        ReferenceModelID =  items[3].Split(new []{"/"}, StringSplitOptions.RemoveEmptyEntries),
                        ReferenceGroup = items[4],
                        Localization = items[5],
                        Level = int.Parse(items[6]),
                        TaskType = items[7],
                        CompletionGoal = int.Parse(items[8]),
                        RewardPoints = int.Parse(items[9]),
                        TaskIcon = items[10],
                        ConditionName = items[11],
                        ConditionValue = int.Parse(items[12]),
                        ProgressWay = items[13]
                    };
                    configs.Add(taskModel);
                }
                ConfigHelper.Save(configs.ToArray(), "AntHillTaskModels", _prettyPrint);
            });
        }

        
        [ExposeMethodInEditor]
        private void BuildingStatConfigToJson()
        {
            Load("BuildingStatModels", data =>
            {
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var config = new Dictionary<string, BuildingStatModel>(); 
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
                        config.Add(modelId, new BuildingStatModel{ModelID = modelId, Stats = new StatModel[0]});
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
                
                ConfigHelper.Save(config.Values.ToArray(), "BuildingStatModels",_prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void BuildingOpenableConfigToJson()
        {
            Load("BuildingOpenablesModels", data =>
            {
                var config = new Dictionary<string, BuildingOpenablesModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // remove headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var modelID = items[1];
                    var roomId = items[2];
                    var placeId = items[3];
                    if (!config.ContainsKey(roomId))
                    {
                        config.Add(roomId,new BuildingOpenablesModel{RoomID = roomId});
                    }

                    var model = new BuildingOpenableModel {ModelID = modelID, PlaceNum = placeId};
                    var configModel = config[roomId];
                    configModel.Items = configModel.Items?.Append(model).ToArray() ?? new[] {model};
                    config[roomId] = configModel;
                }

                ConfigHelper.Save(config.Values.ToArray(), "BuildingOpenablesModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void BuildingUpgradeConfigToJson()
        {
            Load("BuildingUpgradeModels", data =>
            {
                const string statkey = "stat_";
                const string priceKey = "price";
                var config = new List<BuildingUpgradeModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                var ugrades = new Dictionary<string, Dictionary<string, List<float>>>(); // модель, (стат, значения обновления)
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var modelId = items[1];
                    var key = items[2];
                    items.RemoveRange(0,3); // cleanup for foreach
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
                    config.Add(new BuildingUpgradeModel
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

                ConfigHelper.Save(config.ToArray(), "BuildingUpgradeModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void InventoryItemsConfigToJson()
        {
            var config = new[]
            {
                new InventoryItemModel
                {
                    ItemID = "0",
                },
                new InventoryItemModel
                {
                    ItemID = "1",
                },
                new InventoryItemModel
                {
                    ItemID = "2",
                },
                new InventoryItemModel
                {
                    ItemID = "3",
                },
                new InventoryItemModel
                {
                    ItemID = "4",
                },
                new InventoryItemModel
                {
                    ItemID = "5",
                },
                new InventoryItemModel
                {
                    ItemID = "6",
                },
            };
            ConfigHelper.Save(config, "InventoryItemModels",_prettyPrint);
        }

        [ExposeMethodInEditor]
        private void BuildingsPostLoadPriorityConfigToJson()
        {
            var config = new[]
            {
                "0",  // FlaxSeeds
                "1",  // Hazelnut
                "2",  // Apple
                "3",  // SunflowerSeeds
                "4",  // Wheat
                "5",  // Blackberry
                "6",  // Grapes
                "7",  // Sugar
                "8",  // Corn
                "9",  // Chocolate
                "10", // Meat
                "11", // Potato
                "12", // Cabbage
                "13", // Mushroom
                "14", // Burger
                "15", // Buckwheat
                "16", // Barrel
                "17", // BenchPress
                "18", // Bombs
                "19", // Bows
                "20", // Cannonballs
                "21", // Dynamite
                "22", // Flag1
                "23", // Flag2
                "24", // Flag3
                "25", // Flower1
                "26", // Grass1
                "27", // Grass2
                "28", // HangingPear
                "29", // Lamp1
                "30", // Lamp2
                "31", // Lamp3
                "32", // Leaf
                "33", // Mushrooms
                "34", // SmallCatapult
                "35", // Stones
                "36", // Weight
                "37", // Сurtain
                "38", // ArrowTarget
                "39", // Bowl
                "41", // FightStock
                "43", // Garden
                "44", // Goldmine
                "46", // HangingPears
                "48", // Hospital
                "51", // OutdoorPear
                "52", // Pikes
                "53", // Prison

                "55", // RelaxationRoom
                "56", // Safe
                "57", // SleepingPods
                "58", // Swords

                "42", // FoodStock
                "47", // HerbsStock
                "49", // MudStock
                "45", // GrabageStock
                "40", // Dumpster
                "54", // Queen
                "59", // AntHill
                "50", // OrderBoard
            };
            ConfigHelper.Save(config, "PostLoadBuildingPriorities",_prettyPrint);
        }
        
        [ExposeMethodInEditor]
        private void OrderBoardOrdersConfigToJson()
        {
            Load("OrderItemModels", data =>
            {
                const string unitId = "Unit";
                var orderItems = new Dictionary<string, List<OrderItemModel>>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var headers = lines[0].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                lines.RemoveAt(0); // remove headers
                foreach (var line in lines)
                {
                    var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    var itemId = items[0];
                    if (!int.TryParse(itemId, out _)) break;
                    var orderId = items[1];
                    var isUnit = items[2] == unitId;
                    var modelID = items[4];
                    var localizationId = items[5];
                    localizationId = localizationId == "-" ? null : localizationId;
                    var orderItemParams = new List<OrderItemParam>();
                    var rewardParams = new List<OrderItemParam>();
                    for (var i = 6; i < 12; i++)
                    {
                        var values = items[i].Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                        if (values.Contains("-")) continue;
                        
                        var key = char.ToLower(headers[i][0]) + headers[i].Substring(1);
                        var hasCurrent = values.Length > 1;
                        var current = hasCurrent ? float.Parse(values[0]) : 0;
                        var maximum = hasCurrent ? float.Parse(values[1]) : float.Parse(values[0]);

                        if (key.AnyOff("coins", "crystals"))
                        {
                            rewardParams.Add(new OrderItemParam
                            {
                                Id = itemId,
                                Key = key == "coins" ? "0" : "1",
                                CurrentValue = current, 
                                BaseValue = maximum
                            });
                            continue;
                        }
                        
                        orderItemParams.Add(new OrderItemParam
                        {
                            Id = itemId,
                            Key = key, 
                            CurrentValue = current, 
                            BaseValue = maximum
                        });
                    }

                    if (!orderItems.ContainsKey(orderId))
                    {
                        orderItems.Add(orderId, new List<OrderItemModel>());
                    }
                    
                    orderItems[orderId].Add(new OrderItemModel
                    {
                        ItemId = itemId,
                        ModelID = modelID,
                        IsUnit = isUnit,
                        LocalizationID = localizationId,
                        Params = orderItemParams.ToArray(),
                        Rewards = rewardParams.ToArray(),
                    });
                }

                Load("OrderModels", data1 =>
                {
                    const string specialId = "Special";
                    var config = new List<OrderModel>();
                    var lines1 = data1.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    lines1.RemoveAt(0); // remove headers
                    foreach (var line in lines1)
                    {
                        var items = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                        var orderId = items[0];
                        var orderType = items[1];
                        var level = int.Parse(items[2]);
                        var lifeTime = int.Parse(items[3]);
                        var orderModel = new OrderModel
                        {
                            IsSpecial = orderType == specialId,
                            OrderID = orderId,
                            Level = level,
                            Items = orderItems[orderId].ToArray(),
                            LifeTime = lifeTime
                        };

                        config.Add(orderModel);
                    }

                    ConfigHelper.Save(config.ToArray(), "OrderModels",_prettyPrint);
                });
            });
        }
    }
}