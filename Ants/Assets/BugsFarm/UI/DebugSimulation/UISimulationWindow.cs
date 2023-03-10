using System;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISimulationWindow : UISimpleWindow
    {
        [SerializeField] private Button _hours;
        [SerializeField] private Button _mins;
        [SerializeField] private Button _secs;
        [SerializeField] private Button _simulate;
        [SerializeField] private Button _reset;
        [SerializeField] private Button _close;
        [SerializeField] private TextMeshProUGUI _timeText;
        public event EventHandler HoursClickEvent;

        public event EventHandler MinsClickEvent;

        public event EventHandler SecondClickEvent;

        public event EventHandler SimulateClickEvent;

        public event EventHandler ResetClickEvent;

        public event EventHandler CloseClickEvent;
        
        public override void Show()
        {
            base.Show();
            
            _hours.onClick.AddListener(OnHoursClickEventHandler);
            _mins.onClick.AddListener(OnMinsClickEventHandler);
            _secs.onClick.AddListener(OnSecondClickEventHandler);
            _simulate.onClick.AddListener(OnSimulateClickEventHandler);
            _reset.onClick.AddListener(OnResetClickEventHandler);
            _close.onClick.AddListener(OnCloseClickEventHandler);
        }

        public override void Hide()
        {
            base.Hide();
            
            _hours.onClick.RemoveListener(OnHoursClickEventHandler);
            _mins.onClick.RemoveListener(OnMinsClickEventHandler);
            _secs.onClick.RemoveListener(OnSecondClickEventHandler);
            _simulate.onClick.RemoveListener(OnSimulateClickEventHandler);
            _reset.onClick.RemoveListener(OnResetClickEventHandler);
            _close.onClick.RemoveListener(OnCloseClickEventHandler);
        }
        
        public void SetTimeText(string text)    
        {
            if(!_timeText) return;
            _timeText.text = text;
        }

        private void OnHoursClickEventHandler()
        {
            HoursClickEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnMinsClickEventHandler()
        {
            MinsClickEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnSecondClickEventHandler()
        {
            SecondClickEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnSimulateClickEventHandler()
        {
            SimulateClickEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnResetClickEventHandler()
        {
            ResetClickEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnCloseClickEventHandler()
        {
            CloseClickEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
