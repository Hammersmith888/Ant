using System;
using System.Collections.Generic;
using BugsFarm.CurrencySystem;

namespace BugsFarm.UserSystem
{
    public interface IUser
    {
        string Id { get; }
        UserDto Dto { get;}
        void Initialize();
        int GetLevel();
        void AddCurrency(string currencyId, int value);
        int GetCurrency(string currencyId);
        bool HasCurrency(CurrencyModel price);
        IEnumerable<CurrencyModel> GetCurrency();
    }
}