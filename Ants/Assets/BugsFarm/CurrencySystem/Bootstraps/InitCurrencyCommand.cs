using BugsFarm.Services.BootstrapService;
using BugsFarm.Utility;

namespace BugsFarm.CurrencySystem
{
    public class InitCurrencyCommand : Command
    {
        private readonly CurrencySettingStorage _currencyModelsStorage;

        public InitCurrencyCommand(CurrencySettingStorage currencyModelsStorage)
        {
            _currencyModelsStorage = currencyModelsStorage;
        }

        public override void Do()
        {
            var config = ConfigHelper.Load<CurrencySettingModel>("CurrencySettingModels");
            foreach (var model in config)
            {
                _currencyModelsStorage.Add(model);
            }

            OnDone();
        }
    }
}