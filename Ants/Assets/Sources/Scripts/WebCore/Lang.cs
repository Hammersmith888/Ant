
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Assets.Scripts
{
    class Lang
    {
        static Dictionary<string, string> langs = new Dictionary<string, string> {
            {"aar", "aa"},
            {"abk", "ab"},
            {"afr", "af"},
            {"aka", "ak"},
            {"alb", "sq"},
            {"amh", "am"},
            {"ara", "ar"},
            {"arg", "an"},
            {"arm", "hy"},
            {"asm", "as"},
            {"ava", "av"},
            {"ave", "ae"},
            {"aym", "ay"},
            {"aze", "az"},
            {"bak", "ba"},
            {"bam", "bm"},
            {"baq", "eu"},
            {"bel", "be"},
            {"ben", "bn"},
            {"bih", "bh"},
            {"bis", "bi"},
            {"bos", "bs"},
            {"bre", "br"},
            {"bul", "bg"},
            {"bur", "my"},
            {"cat", "ca"},
            {"cha", "ch"},
            {"che", "ce"},
            {"chi", "zh"},
            {"chu", "cu"},
            {"chv", "cv"},
            {"cor", "kw"},
            {"cos", "co"},
            {"cre", "cr"},
            {"cze", "cs"},
            {"dan", "da"},
            {"div", "dv"},
            {"dut", "nl"},
            {"dzo", "dz"},
            {"eng", "en"},
            {"epo", "eo"},
            {"est", "et"},
            {"ewe", "ee"},
            {"fao", "fo"},
            {"fij", "fj"},
            {"fin", "fi"},
            {"fre", "fr"},
            {"fry", "fy"},
            {"ful", "ff"},
            {"geo", "ka"},
            {"ger", "de"},
            {"gla", "gd"},
            {"gle", "ga"},
            {"glg", "gl"},
            {"glv", "gv"},
            {"gre", "el"},
            {"grn", "gn"},
            {"guj", "gu"},
            {"hat", "ht"},
            {"hau", "ha"},
            {"heb", "he"},
            {"her", "hz"},
            {"hin", "hi"},
            {"hmo", "ho"},
            {"hrv", "hr"},
            {"hun", "hu"},
            {"ibo", "ig"},
            {"ice", "is"},
            {"ido", "io"},
            {"iii", "ii"},
            {"iku", "iu"},
            {"ile", "ie"},
            {"ina", "ia"},
            {"ind", "id"},
            {"ipk", "ik"},
            {"ita", "it"},
            {"jav", "jv"},
            {"jpn", "ja"},
            {"kal", "kl"},
            {"kan", "kn"},
            {"kas", "ks"},
            {"kau", "kr"},
            {"kaz", "kk"},
            {"khm", "km"},
            {"kik", "ki"},
            {"kin", "rw"},
            {"kir", "ky"},
            {"kom", "kv"},
            {"kon", "kg"},
            {"kor", "ko"},
            {"kua", "kj"},
            {"kur", "ku"},
            {"lao", "lo"},
            {"lat", "la"},
            {"lav", "lv"},
            {"lim", "li"},
            {"lin", "ln"},
            {"lit", "lt"},
            {"ltz", "lb"},
            {"lub", "lu"},
            {"lug", "lg"},
            {"mac", "mk"},
            {"mah", "mh"},
            {"mal", "ml"},
            {"mao", "mi"},
            {"mar", "mr"},
            {"may", "ms"},
            {"mlg", "mg"},
            {"mlt", "mt"},
            {"mon", "mn"},
            {"nau", "na"},
            {"nav", "nv"},
            {"nbl", "nr"},
            {"nde", "nd"},
            {"ndo", "ng"},
            {"nep", "ne"},
            {"nno", "nn"},
            {"nob", "nb"},
            {"nor", "no"},
            {"nya", "ny"},
            {"oci", "oc"},
            {"oji", "oj"},
            {"ori", "or"},
            {"orm", "om"},
            {"oss", "os"},
            {"pan", "pa"},
            {"per", "fa"},
            {"pli", "pi"},
            {"pol", "pl"},
            {"por", "pt"},
            {"pus", "ps"},
            {"que", "qu"},
            {"roh", "rm"},
            {"rum", "ro"},
            {"run", "rn"},
            {"rus", "ru"},
            {"sag", "sg"},
            {"san", "sa"},
            {"sin", "si"},
            {"slo", "sk"},
            {"slv", "sl"},
            {"sme", "se"},
            {"smo", "sm"},
            {"sna", "sn"},
            {"snd", "sd"},
            {"som", "so"},
            {"sot", "st"},
            {"spa", "es"},
            {"srd", "sc"},
            {"srp", "sr"},
            {"ssw", "ss"},
            {"sun", "su"},
            {"swa", "sw"},
            {"swe", "sv"},
            {"tah", "ty"},
            {"tam", "ta"},
            {"tat", "tt"},
            {"tel", "te"},
            {"tgk", "tg"},
            {"tgl", "tl"},
            {"tha", "th"},
            {"tib", "bo"},
            {"tir", "ti"},
            {"ton", "to"},
            {"tsn", "tn"},
            {"tso", "ts"},
            {"tuk", "tk"},
            {"tur", "tr"},
            {"twi", "tw"},
            {"uig", "ug"},
            {"ukr", "uk"},
            {"urd", "ur"},
            {"uzb", "uz"},
            {"ven", "ve"},
            {"vie", "vi"},
            {"vol", "vo"},
            {"wel", "cy"},
            {"wln", "wa"},
            {"wol", "wo"},
            {"xho", "xh"},
            {"yid", "yi"},
            {"yor", "yo"},
            {"zha", "za"},
            {"zul", "zu" }
        };

        public static string get3Alpha()
        {
            /*var alpha2 = Get2LetterISOCodeFromSystemLanguage().Trim().ToLower();
            foreach (var item in langs)
                if (alpha2.StartsWith(item.Value)) return item.Key;
            return alpha2;*/
            var cu = CultureInfo.CurrentCulture;
            return cu.ThreeLetterISOLanguageName;
        }

        public static string Get2LetterISOCodeFromSystemLanguage()
        {
            SystemLanguage lang = Application.systemLanguage;
            string res = "EN";
            switch (lang)
            {
                case SystemLanguage.Afrikaans: res = "AF"; break;
                case SystemLanguage.Arabic: res = "AR"; break;
                case SystemLanguage.Basque: res = "EU"; break;
                case SystemLanguage.Belarusian: res = "BY"; break;
                case SystemLanguage.Bulgarian: res = "BG"; break;
                case SystemLanguage.Catalan: res = "CA"; break;
                case SystemLanguage.Chinese: res = "ZH"; break;
                case SystemLanguage.Czech: res = "CS"; break;
                case SystemLanguage.Danish: res = "DA"; break;
                case SystemLanguage.Dutch: res = "NL"; break;
                case SystemLanguage.English: res = "EN"; break;
                case SystemLanguage.Estonian: res = "ET"; break;
                case SystemLanguage.Faroese: res = "FO"; break;
                case SystemLanguage.Finnish: res = "FI"; break;
                case SystemLanguage.French: res = "FR"; break;
                case SystemLanguage.German: res = "DE"; break;
                case SystemLanguage.Greek: res = "EL"; break;
                case SystemLanguage.Hebrew: res = "IW"; break;
                case SystemLanguage.Hungarian: res = "HU"; break;
                case SystemLanguage.Icelandic: res = "IS"; break;
                case SystemLanguage.Indonesian: res = "IN"; break;
                case SystemLanguage.Italian: res = "IT"; break;
                case SystemLanguage.Japanese: res = "JA"; break;
                case SystemLanguage.Korean: res = "KO"; break;
                case SystemLanguage.Latvian: res = "LV"; break;
                case SystemLanguage.Lithuanian: res = "LT"; break;
                case SystemLanguage.Norwegian: res = "NO"; break;
                case SystemLanguage.Polish: res = "PL"; break;
                case SystemLanguage.Portuguese: res = "PT"; break;
                case SystemLanguage.Romanian: res = "RO"; break;
                case SystemLanguage.Russian: res = "RU"; break;
                case SystemLanguage.SerboCroatian: res = "SH"; break;
                case SystemLanguage.Slovak: res = "SK"; break;
                case SystemLanguage.Slovenian: res = "SL"; break;
                case SystemLanguage.Spanish: res = "ES"; break;
                case SystemLanguage.Swedish: res = "SV"; break;
                case SystemLanguage.Thai: res = "TH"; break;
                case SystemLanguage.Turkish: res = "TR"; break;
                case SystemLanguage.Ukrainian: res = "UK"; break;
                case SystemLanguage.Unknown: res = "EN"; break;
                case SystemLanguage.Vietnamese: res = "VI"; break;
                case SystemLanguage.ChineseSimplified: res = "zh"; break;
                case SystemLanguage.ChineseTraditional: res = "zh"; break;
            }
            //		Debug.Log ("Lang: " + res);
            return res;
        }
    }
}