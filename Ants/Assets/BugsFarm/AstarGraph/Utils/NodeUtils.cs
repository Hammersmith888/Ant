using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Pathfinding;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public static class NodeUtils
    {
        public static bool IsConnector(this GraphNode node)
        {
            return node is NodeConnector;
        }
        public static bool IsJoint(this GraphNode node)
        {
            return node is NodeJoint;
        }

        public static int GetTagID(string tagName)
        {
            if (!AstarPath.active)
            {
                Debug.LogError($"{nameof(NodeUtils)} :  {nameof(GetTagID)} :: AstarPath not initialized.");
                return -1;
            }
            var tagNames = AstarPath.active.GetTagNames();
            return tagNames.IndexOf(tagName);
        }
        public static string GetTagName(uint tagId)
        {
            if (!AstarPath.active)
            {
                Debug.LogError($"{nameof(NodeUtils)} :  {nameof(GetTagName)} :: AstarPath not initialized.");
                return string.Empty;
            }
            
            var tagNames = AstarPath.active.GetTagNames();
            return tagNames[tagId];
        }
        /// <summary>
        /// Convert bitwise mask to enumerable masks.
        /// Where a bit equal 1 or 0, and retun a bit position : index + 1
        /// </summary>
        public static IEnumerable<uint> ConvertBitwiseMask(this int bitwiseMask, int index = 0, bool invert = false)
        {
            if (index > 31 || index < 0)
            {
                Debug.LogError("Index must be greater than 0 or less than 32");
                yield break;
            }
            var tags = Convert.ToString(bitwiseMask, 2);
            var bitSearch = invert ? '0' : '1';
            for (;index < tags.Length; index++)
            {
                if (tags[index] != bitSearch) continue;
                yield return (uint)index+1;
            }
        }
        
        public static bool IncludeBitwiseMask(this int bitwiseMask, int value, bool valueIsMask = false)
        {
            return bitwiseMask == -1 || (bitwiseMask & (valueIsMask ? value : 1 << value)) != 0;
        }
    }
}