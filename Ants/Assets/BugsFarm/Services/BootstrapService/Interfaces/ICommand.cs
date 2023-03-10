using System;

namespace BugsFarm.Services.BootstrapService
{
    public interface ICommand
    {
        event EventHandler Done;
        void Do();
    }
}