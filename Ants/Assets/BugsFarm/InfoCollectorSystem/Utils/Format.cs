using System;
using System.Globalization;
using BugsFarm.Services.SimpleLocalization;

namespace BugsFarm.InfoCollectorSystem
{
    public static class Format
    {
        public static string Header(string header, int level)
        {
            return header + " : " + level + Texts.lvl;
        }

        public static string Time(TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0)
            {
                return Age(timeSpan.TotalSeconds);
            }
            return timeSpan.ToString(timeSpan.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        }
        public static string Resource(float current, float maximum, bool useKiloFormat = false)
        {
            var valueCurr = current.ToString(CultureInfo.InvariantCulture);
            var valueMax = maximum.ToString(CultureInfo.InvariantCulture);
            if (useKiloFormat)
            {
                valueCurr = FormatNumber((int) current);
                valueMax = FormatNumber((int) maximum);
            }
            return $"{valueCurr} / {valueMax}";
        }
        public static string ResourceColored(float current, string currColor, 
                                             float maximum, string maxColor,
                                             bool useKiloFormat = false)
        {
            var valueCurr = current.ToString(CultureInfo.InvariantCulture);
            var valueMax = maximum.ToString(CultureInfo.InvariantCulture);
            if (useKiloFormat)
            {
                valueCurr = FormatNumber((int) current);
                valueMax = FormatNumber((int) maximum);
            }

            if (!string.IsNullOrEmpty(currColor))
            {
                valueCurr = $"<color={currColor.ToLower()}>{valueCurr}</color>";
            }

            if (!string.IsNullOrEmpty(maxColor))
            {
                valueMax = $"<color={maxColor.ToLower()}>{"/" + valueMax}</color>";
            }
            else
            {
                valueMax = "/" + valueMax;
            }
            
            return valueCurr + valueMax;
        }

        public static string FormatNumber(int num)
        {
            if (num >= 1000000000) {
                return (num / 10000000D).ToString("0.##kkk",CultureInfo.InvariantCulture);
            }
            if (num >= 100000000) {
                return (num / 1000000D).ToString("0.#kk",CultureInfo.InvariantCulture);
            }
            if (num >= 1000000) {
                return (num / 1000000D).ToString("0.##kk",CultureInfo.InvariantCulture);
            }
            if (num >= 100000) {
                return (num / 1000D).ToString("0.#k",CultureInfo.InvariantCulture);
            }
            if (num >= 10000) {
                return (num / 1000D).ToString("0.##k",CultureInfo.InvariantCulture);
            }
            if (num >= 1000)
            {
                return (num / 1000D).ToString("0.##k",CultureInfo.InvariantCulture);
            }

            return num.ToString();
        }

    #region ConvertTime
        
        public static float SecondsToMinutes(float seconds)
        {
            return seconds / 60f;
        }
        
        public static float SecondsToHours(float seconds)
        {
            return seconds / 60f  // minutes
                           / 60f; // hours
        }
        
        public static float MinutesToSeconds(float minutes)
        {
            return minutes * 60f;
        }
        
        public static float MinutesToHours(float minutes)
        {
            return minutes / 60f;
        }
        
        public static float HoursToSeconds(float hours)
        {
            return hours * 60f  // minutes
                         * 60f; // seconds
        }
        
        public static float HoursToMinutes(float hours)
        {
            return hours * 60f; // minutes
        }
    #endregion

        public static string Age(double seconds, bool onlyOne = false)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.Days > 0)
            {
                var day = LocalizationManager.Localize(Texts.Day);
                var hour = LocalizationManager.Localize(Texts.Hour);
                return Age(ts.Days, ts.Hours, day, hour, onlyOne);
            }
            
            if (ts.Hours > 0)
            {
                var min = LocalizationManager.Localize(Texts.Minute);
                var hour = LocalizationManager.Localize(Texts.Hour);
                return Age(ts.Hours, ts.Minutes, hour, min, onlyOne);
            }
            else
            {
                var min = LocalizationManager.Localize(Texts.Minute);
                var sec = LocalizationManager.Localize(Texts.Second);
                return Age(ts.Minutes, ts.Seconds, min, sec, onlyOne);
            }
        }
        private static string Age( int t1, int t2, string str1, string str2, bool onlyOne = false )
        {
            return t1 > 0 ? 
                $"{t1} {str1}" + (onlyOne || t2 <= 0 ? "" : $" {t2} {str2}") : 
                $"{t2} {str2}";

        }

        public static string FormatId(string formatId, double value)
        {
            switch (formatId)
            {
                case "TimeMin": return Time(TimeSpan.FromMinutes(value));
                case "TimeSec": return Time(TimeSpan.FromSeconds(value));
                case "AgeMin":  return Age(MinutesToSeconds((float) value), true);
                case "AgeSec":  return Age((float) value, true);
                default:        return value.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}