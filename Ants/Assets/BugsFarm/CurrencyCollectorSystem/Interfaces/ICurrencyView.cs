using UnityEngine;

namespace BugsFarm.CurrencyCollectorSystem
{
    public interface ICurrencyView
    {
        string CurrencyID { get; }
        Transform Target { get; }
        void SetCurrencyText(string text);
    }
}