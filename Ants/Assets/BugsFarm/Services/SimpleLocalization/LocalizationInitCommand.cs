using System;
using BugsFarm.Services.BootstrapService;

namespace BugsFarm.Services.SimpleLocalization
{
    public class LocalizationInitCommand : Command
    {
        public override void Do()
        {
            LocalizationManager.Read();
            LocalizationManager.Language = "Russian";
            OnDone();
        }
    }
}