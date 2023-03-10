using System;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.CurrencySystem
{
    [Serializable]
    public struct CurrencySettingModel : IStorageItem
    {
        public string ModelID;
        public string HexColor;
        
        string IStorageItem.Id => ModelID;

        public Color ConvertedColor()
        {
            return ColorUtility.TryParseHtmlString(HexColor, out var color) ? color : Color.white;
        }
    }
}