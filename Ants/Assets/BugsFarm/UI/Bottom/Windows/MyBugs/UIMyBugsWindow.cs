 using System;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIMyBugsWindow : UISimpleWindow
    {
        public CatalogView CatalogView => _catalogView;
        public TerrariumView TerrariumView => _terrariumView;
        public WikiView WikiView => _wikiView;

        [SerializeField] private CatalogView _catalogView;
        [SerializeField] private TerrariumView _terrariumView;
        [SerializeField] private WikiView _wikiView;
        [SerializeField] private RectTransform[] _greenTabs;
        [SerializeField] private RectTransform[] _blueTabs;
        [SerializeField] private Button _terrariumButton;
        [SerializeField] private Button _catalogButton;
        [SerializeField] private Button[] _closeButtons;

        public event EventHandler<string> TabEvent;

        public override void Show()
        {
            base.Show();
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }

            _terrariumButton.onClick.AddListener(OnTerrariumEventHandler);
            _catalogButton.onClick.AddListener(OnCatalogEventHandler);
        }

        public override void Hide()
        {
            base.Hide();
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }

            _terrariumButton.onClick.RemoveListener(OnTerrariumEventHandler);
            _catalogButton.onClick.RemoveListener(OnCatalogEventHandler);
            TabEvent = null;
        }

        public void SetPrimaryTab(string tabId)
        {
            RectTransform[] tabs;
            switch (tabId)
            {
                case "Terrarium":
                    tabs = _greenTabs;
                    break;
                case "Catalog":
                    tabs = _blueTabs;
                    break;
                case "Wiki":
                    tabs = _blueTabs;
                    break;
                default: return;
            }

            if (tabs == null)
            {
                return;
            }

            foreach (var tab in tabs)
            {
                tab.SetAsLastSibling();
            }
        }

        private void OnTerrariumEventHandler()
        {
            TabEvent?.Invoke(this, "Terrarium");
        }

        private void OnCatalogEventHandler()
        {
            TabEvent?.Invoke(this, "Catalog");
        }
    }
}