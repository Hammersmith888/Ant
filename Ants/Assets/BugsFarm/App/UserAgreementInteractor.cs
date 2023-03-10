using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UI;

namespace BugsFarm.App
{
    public class UserAgreementInteractor
    {
        private readonly IUIService _uiService;
        private const string _privatePolicyTextKey = "UserAgreementWindow_PrivatePolicy";
        private const string _acceptTextKey = "UserAgreementWindow_Accept";
        private const char _str = '"';
        private string Link1 => _str + "http://topscale.games/bugs/terms.html" + _str;
        private string Link2 => _str + "http://topscale.games/bugs/privacy.html" + _str;
        private string Arg1 => @"<link ={0}><color=blue><u>";
        private string Arg2 => @"</u></color></link>";
        private Action _onAccept;

        public UserAgreementInteractor(IUIService uiService)
        {
            _uiService = uiService;
        }

        public void Initialize(Action onAccept)
        {
            var privatePolicyText = LocalizationManager.Localize(_privatePolicyTextKey);
            var arg0 = LocalizationManager.Localize(_acceptTextKey);
            var arg1 = string.Format(Arg1, Link1);
            var arg3 = string.Format(Arg1, Link2);
            _onAccept = onAccept;

            var window = _uiService.Show<UIMessageBoxWindow>();
            window.AcceptEvent += OnAccept;
            window.SetAcceptText(arg0);
            window.SetMessageText(string.Format(privatePolicyText, arg0, arg1, Arg2, arg3));
        }

        public void Dispose()
        {
            _uiService.Hide<UIMessageBoxWindow>();
            _onAccept = null;
        }

        private void OnAccept(object sender, EventArgs e)
        {
            _onAccept?.Invoke();
            Dispose();
        }
    }
}