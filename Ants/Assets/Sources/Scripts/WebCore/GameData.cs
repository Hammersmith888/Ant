using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts
{
    public partial class GameData
    {
        [JsonProperty("flag", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Flag { get; set; }

        [JsonProperty("setup", NullValueHandling = NullValueHandling.Ignore)]
        public int Setup { get; set; }

        [JsonProperty("supportPage", NullValueHandling = NullValueHandling.Ignore)]
        public string SupportPage { get; set; }

        [CanBeNull]
        [JsonProperty("appsFlyerKey")]
        public string AppsFlyerKey { get; set; }

    }

    public partial class GameData
    {
        public static GameData FromJson(string json) => JsonConvert.DeserializeObject<GameData>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GameData self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}