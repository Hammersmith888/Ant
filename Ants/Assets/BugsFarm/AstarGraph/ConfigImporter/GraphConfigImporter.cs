using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using BugsFarm.Graphic;
using BugsFarm.UnitSystem;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.AstarGraph.ConfigImporter
{

    public class GraphConfigImporter : MonoBehaviour
    {
        [ExposeMethodInEditor]
        private void GraphMaskConfigToJson()
        {
        #region Readonly
            const uint nonTag     = 0;
            const uint groundTag  = 1;
            const uint laddersTag = 2;
            const uint wallsTag   = 3;
            const uint bridgesTag = 4;
            const uint jointsTag  = 5;
            const uint flyTag     = 6;
            const uint grassTag   = 7;
            const uint npcTag     = 8;
            const uint vinesTag   = 9;
            
            var defaultLayer              = 1 << SortingLayers.NameToLayerIndex("Default");
            var uiScreenLayer             = 1 << SortingLayers.NameToLayerIndex("UIScreen");
            var uiWorldLayer              = 1 << SortingLayers.NameToLayerIndex("UIWorld");
            var nightMaskLayer            = 1 << SortingLayers.NameToLayerIndex("NightMask");
            var underNightLayer           = 1 << SortingLayers.NameToLayerIndex("UnderNight");
            var foreAboveGroundLayer      = 1 << SortingLayers.NameToLayerIndex("ForeAboveGround");
            var foreGroundObjectsLayer    = 1 << SortingLayers.NameToLayerIndex("ForeGroundObjects");
            var foreUnderGroundLayer      = 1 << SortingLayers.NameToLayerIndex("ForeUnderGround");
            var foregroundLayer           = 1 << SortingLayers.NameToLayerIndex("Foreground");
            var middleObjectsGroundLayer  = 1 << SortingLayers.NameToLayerIndex("MiddleObjectsGround");
            var middleGroundLayer         = 1 << SortingLayers.NameToLayerIndex("MiddleGround");
            var middleBackgroundLayer     = 1 << SortingLayers.NameToLayerIndex("MiddleBackground");
            var backgroundLayer           = 1 << SortingLayers.NameToLayerIndex("Background");
            var backUnderGroundLayer      = 1 << SortingLayers.NameToLayerIndex("BackUnderGround");
            
            var backgrounLayersMask = foreUnderGroundLayer | backgroundLayer | backUnderGroundLayer | middleBackgroundLayer;
        #endregion
            
            var config = new List<GraphMaskModel>
            {
                new GraphMaskModel {ModelID = nameof(RestTask), Masks = new Dictionary<uint,MaskModel>
                {
                    {nonTag,     new MaskModel{Tag = nonTag,     BitMaskLayers = 0}}, 
                    {laddersTag, new MaskModel{Tag = laddersTag, BitMaskLayers = 0}}, 
                    {jointsTag,  new MaskModel{Tag = jointsTag,  BitMaskLayers = 0}}, 
                    {npcTag,     new MaskModel{Tag = npcTag,     BitMaskLayers = 0}},
                    {vinesTag,   new MaskModel{Tag = vinesTag,   BitMaskLayers = 0}},
                    {groundTag,  new MaskModel{Tag = groundTag,  BitMaskLayers = backgrounLayersMask}},
                    {grassTag,   new MaskModel{Tag = grassTag,   BitMaskLayers = backgrounLayersMask}},
                    {wallsTag,   new MaskModel{Tag = wallsTag,   BitMaskLayers = backgrounLayersMask}},
                    {bridgesTag, new MaskModel{Tag = bridgesTag, BitMaskLayers = backgrounLayersMask}},
                    {flyTag,     new MaskModel{Tag = flyTag,     BitMaskLayers = backgrounLayersMask}},
                }},
                new GraphMaskModel {ModelID = nameof(UnitDeathSystem), Masks = new Dictionary<uint,MaskModel>
                {
                    {nonTag,     new MaskModel{Tag = nonTag,     BitMaskLayers = 0}}, 
                    {laddersTag, new MaskModel{Tag = laddersTag, BitMaskLayers = 0}}, 
                    {jointsTag,  new MaskModel{Tag = jointsTag,  BitMaskLayers = 0}}, 
                    {npcTag,     new MaskModel{Tag = npcTag,     BitMaskLayers = 0}},
                    {vinesTag,   new MaskModel{Tag = vinesTag,   BitMaskLayers = 0}},
                    {flyTag,     new MaskModel{Tag = flyTag,     BitMaskLayers = 0}},
                    {bridgesTag, new MaskModel{Tag = bridgesTag, BitMaskLayers = 0}},
                    {wallsTag,   new MaskModel{Tag = wallsTag,   BitMaskLayers = 0}},
                    {groundTag,  new MaskModel{Tag = groundTag,  BitMaskLayers = backgrounLayersMask}},
                    {grassTag,   new MaskModel{Tag = grassTag,   BitMaskLayers = backgrounLayersMask}},
                }},
                
                new GraphMaskModel {ModelID = nameof(UnitSleepSystem), Masks = new Dictionary<uint,MaskModel>
                {
                    {nonTag,     new MaskModel{Tag = nonTag,     BitMaskLayers = 0}}, 
                    {laddersTag, new MaskModel{Tag = laddersTag, BitMaskLayers = 0}}, 
                    {jointsTag,  new MaskModel{Tag = jointsTag,  BitMaskLayers = 0}}, 
                    {npcTag,     new MaskModel{Tag = npcTag,     BitMaskLayers = 0}},
                    {flyTag,     new MaskModel{Tag = flyTag,     BitMaskLayers = 0}},
                    {bridgesTag, new MaskModel{Tag = bridgesTag, BitMaskLayers = 0}},
                    {groundTag,  new MaskModel{Tag = groundTag,  BitMaskLayers = backgrounLayersMask}},
                    {grassTag,   new MaskModel{Tag = grassTag,   BitMaskLayers = backgrounLayersMask}},
                    {wallsTag,   new MaskModel{Tag = wallsTag,   BitMaskLayers = backgrounLayersMask}},
                    {vinesTag,   new MaskModel{Tag = vinesTag,   BitMaskLayers = backgrounLayersMask}},
                }},
                
                new GraphMaskModel {ModelID = nameof(HerbsStock), Masks = new Dictionary<uint,MaskModel>
                {
                    {nonTag,     new MaskModel{Tag = nonTag,     BitMaskLayers = 0}}, 
                    {groundTag,  new MaskModel{Tag = groundTag,  BitMaskLayers = 0}}, 
                    {laddersTag, new MaskModel{Tag = laddersTag, BitMaskLayers = 0}}, 
                    {wallsTag,   new MaskModel{Tag = wallsTag,   BitMaskLayers = 0}},
                    {bridgesTag, new MaskModel{Tag = bridgesTag, BitMaskLayers = 0}},
                    {jointsTag,  new MaskModel{Tag = jointsTag,  BitMaskLayers = 0}},
                    {flyTag,     new MaskModel{Tag = flyTag,     BitMaskLayers = 0}},
                    {npcTag,     new MaskModel{Tag = npcTag,     BitMaskLayers = 0}},
                    {vinesTag,   new MaskModel{Tag = vinesTag,   BitMaskLayers = 0}},
                    {grassTag,   new MaskModel{Tag = grassTag,   BitMaskLayers = backgrounLayersMask}},
                }},
            };

            for (var i = 0; i < config.Count; i++)
            {
                var graphMaskModel = config[i];
                foreach (var maskModel in graphMaskModel.Masks.Values)
                {
                    graphMaskModel.BitMaskTags |= 1 << (int)maskModel.Tag;
                }
                config[i] = graphMaskModel;
            }
            
            ConfigHelper.Save(config.ToArray(), "GraphMaskModels");
        }
    }
}