using System;
using BugsFarm.CurrencySystem;

namespace BugsFarm.UpgradeSystem
{
    [Serializable]
    public struct UpgradeLevelModel
    {
        public int Level;
        public CurrencyModel Price;
        public UpgradeStatModel[] Stats;
    }
}