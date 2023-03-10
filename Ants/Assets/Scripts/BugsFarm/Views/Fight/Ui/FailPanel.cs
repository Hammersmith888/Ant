using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Views.Screen;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BugsFarm.Views.Fight.Ui
{
    public class FailPanel : APanel
    {
        [Inject] private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        [Inject] private readonly FightScreen _fightScreen;

        [SerializeField] private RectTransform[] props;
        [SerializeField] private Animation openAnimation;
        [SerializeField] private CanvasGroup canvasGroup;

        private readonly List<Vector2> _propsPositions = new List<Vector2>();

        protected override void Init(out bool isModal, out bool manualClose)
        {
            isModal = true;
            manualClose = true;
        }

        private void Awake()
        {
            foreach (var prop in props)
                _propsPositions.Add(prop.anchoredPosition);
        }

        protected override void OnOpened()
        {
            _fightScreen.Veil.enabled = true;
            openAnimation.Play("popup");

            for (var i = 0; i < props.Length; i++)
                Tween(props[i], _propsPositions[i], i);
        }

        private void Tween(RectTransform rt, Vector3 target, int i)
        {
            rt.anchoredPosition = target * _battleSettings.failPanelStartMul;
            rt.DOKill();
            rt
                .DOAnchorPos(target, _battleSettings.failPanelDuration)
                .SetDelay(_battleSettings.failPanelDelay + _battleSettings.failPanelDelayStep * i)
                .SetEase(_battleSettings.failPanelEase);
        }

        protected override void OnClosed()
        {
            canvasGroup.blocksRaycasts = false;
            Deactivate();
        }

        public void EventOpened() => canvasGroup.blocksRaycasts = true;
        public void EventClosed() => Deactivate();

        private void Deactivate()
        {
            gameObject.SetActive(false);
            _fightScreen.Veil.enabled = false;
        }
    }
}