using BugsFarm.CurrencySystem;
using BugsFarm.Services.CommandService;

namespace BugsFarm.UserSystem
{
    public readonly struct UserCurrencyChangedProtocol : IProtocol
    {
        public readonly CurrencyModel Currency;
        public UserCurrencyChangedProtocol(CurrencyModel currency)
        {
            Currency = currency;
        }
    }
}