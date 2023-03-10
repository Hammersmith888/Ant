using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Game;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public enum WinPanelStage
{
    None,

    ChestClosed,
    ChestOpenAnimation,
    Wait,
    ShowReward,
    GetRewardAnimation,
    ContinueOrExit,
}


public class WinPanel : APanel
{
    [Inject] private readonly BattleContext _battle;
    [Inject] private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
    
    [SerializeField] private Animation panelAnimation;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private SkeletonGraphic chestSpine;
    [SerializeField] private RectTransform[] props;

    #region Old

    [Header("---")] public GameObject _buttonOpenGet;

    public GameObject _buttonReturn;
    public Button _buttonNextRoom;
    [SerializeField] GameObject _chestContent;
    [SerializeField] Text _text_Coins;
    [SerializeField] Text _text_Crystals;
    [SerializeField] GameObject _icon_Coins;
    [SerializeField] GameObject _icon_Crystals;
    [SerializeField] GameObject _text;
    [SerializeField] Text _buttonOpenGetText;

    #endregion

    private AnimPlayer _player;
    private WinPanelStage _stage;
    private RewardWrap _coins;
    private RewardWrap _crystals;
    private readonly Timer _timerIdleOpened = new Timer(TimerType.Unscaled);

    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = false;
        manualClose = true;
    }

    private void Awake()
    {
        _player = new AnimPlayer(chestSpine);

        _coins = new RewardWrap(_text_Coins, _icon_Coins);
        _crystals = new RewardWrap(_text_Crystals, _icon_Crystals);
    }

    protected override void OnOpened()
    {
        //UI_Control.Instance.SetVeil(VeilType.Fight, true);

        panelAnimation.Play("popup");

        for (var i = 0; i < props.Length; i++)
            Tween(props[i], props[i].anchoredPosition, i);

        SetStage(WinPanelStage.ChestClosed);
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

    private void SetStage(WinPanelStage stage)
    {
        _stage = stage;

        switch (stage)
        {
            case WinPanelStage.ChestClosed:
                chestSpine.gameObject.SetActive(true);
                chestSpine.AnimationState
                    .ClearTracks(); // (!) Important, https://github.com/pixijs/pixi-spine/issues/166
                chestSpine.Skeleton.SetToSetupPose();
                //_player.Play("Idle", true);

                _chestContent.SetActive(false);
                _text.SetActive(false);

                _buttonOpenGetText.text = Texts.Open;
                _buttonOpenGet.gameObject.SetActive(true);
                _buttonReturn.gameObject.SetActive(false);
                _buttonNextRoom.gameObject.SetActive(false);
                break;

            case WinPanelStage.ChestOpenAnimation:
                _player.Play("Open", false);
                Sounds.Play(Sound.ChestOpen);

                _buttonOpenGet.gameObject.SetActive(false);
                break;

            case WinPanelStage.Wait:
                _player.Play("Idle open", false);
                _timerIdleOpened.Set(.75f);
                break;

            case WinPanelStage.ShowReward:
                chestSpine.gameObject.SetActive(false);

                _chestContent.SetActive(true);
                var rooms = Data_Fight.Instance.enemies.roomsConfig;
                var index = Mathf.Min(
                    _battle.currentRoomEntity.roomIndex.Value,
                    rooms.Count - 1);
                var room = Data_Fight.Instance.enemies.roomsConfig[index];
                _coins.SetStart(room.reward_Coins);
                _crystals.SetStart(room.reward_Crystals);

                _buttonOpenGetText.text = Texts.Get;
                _buttonOpenGet.gameObject.SetActive(true);
                break;

            case WinPanelStage.GetRewardAnimation:
               // CollectedCoins.Instance.Collect(_text_Coins.transform, Currency.Coins, _coins.Value, _coins.Decrease);
               // CollectedCoins.Instance.Collect(_text_Crystals.transform, Currency.Crystals, _crystals.Value,
                    //_crystals.Decrease);

                _buttonOpenGet.gameObject.SetActive(false);
                break;

            case WinPanelStage.ContinueOrExit:
                _text.SetActive(true);
                _buttonReturn.gameObject.SetActive(true);
                _buttonNextRoom.gameObject.SetActive(true);
                break;
        }

        Canvas.ForceUpdateCanvases(); // for Tutorial
        GameEvents.OnWinPanelNextStage?.Invoke(stage);
    }

    private void Update()
    {
        switch (_stage)
        {
            case WinPanelStage.ChestOpenAnimation:
                if (_player.IsAnimComplete)
                    NextStage();
                break;

            case WinPanelStage.Wait:
                if (_timerIdleOpened.IsReady)
                    NextStage();
                break;

            case WinPanelStage.GetRewardAnimation:
                if (
                    _coins.Value <= 0 &&
                    _crystals.Value <= 0
                )
                    NextStage();
                break;
        }
    }

    private void NextStage() => SetStage(_stage + 1);

    public void OnOpenGet() => NextStage();

    public void OnReturnToFarm()
    {
        Close();
        // todo return to farm
        _battle.ReplaceFightState(EFightState.None);
    }

    public void OnNextRoom()
    {
        Close();
        _battle.ReplaceFightState(EFightState.EnterCave);
    }

    protected override void OnClosed()
    {
        canvasGroup.blocksRaycasts = false;
    }

    public void EventOpened()
    {
        canvasGroup.blocksRaycasts = true;
        GameEvents.OnWinPanelNextStage?.Invoke(WinPanelStage.ChestClosed);
    }

    public void EventClosed() => Deactivate();

    void Deactivate()
    {
        Close();
    }
}