using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public static class UnitsUtils
    {
        private const int _maleCount = 31;
        private const int _femaleCount = 47;
        public static int GenerateNameKey(bool isFemale)
        {
            var maxCount = isFemale ? _femaleCount : _maleCount;
            return Random.Range(0, maxCount);
        }
    }
}