using System;

namespace BugsFarm.UpgradeSystem
{
    [Serializable]
    public struct UpgradeStatModel
    {
        public string StatID;
        public float Value;
        public UpgradeStatModel(string statID, float value)
        {
            StatID = statID;
            Value = value;
        }
    }
}