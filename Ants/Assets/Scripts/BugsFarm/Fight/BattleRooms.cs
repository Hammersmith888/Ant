using BugsFarm.Model.Enum;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class BattleRooms : MB_Singleton<BattleRooms>
{
    [Inject] private readonly GameContext _game;
    [Inject] private readonly BattleContext _battle;

    [SerializeField] private Transform posRoom1;
    [SerializeField] private Transform posRoom2;
    [SerializeField] private Material darkness;
    [SerializeField] private BattleRoom roomPrefab;
    [SerializeField] private BattleRoom roomNewPrefab;

    public Transform PosRoom1 => posRoom1;
    public Transform PosRoom2 => posRoom2;
    public BattleRoom RoomPrefab => roomPrefab;
    public Material Darkness => darkness;
    public BattleRoom RoomNewPrefab => roomNewPrefab;
    
    public void AnimateLogo()
    {
        var currentRoom = _battle.currentRoomEntity;
        var battleRoom = currentRoom.battleRoom.Value;
        var logo = battleRoom.Logo;

        logo.gameObject.SetActive(true);
        logo.alpha = 1;
        logo.transform.localScale = Vector3.zero;

        DOTween.Sequence()
                .SetDelay(.75f)
                .Append(logo.transform.DOScale(Vector3.one * .01f * 1.25f, 1).SetEase(Ease.OutQuint))
                .AppendInterval(.5f)
                .Append(logo.DOFade(0, .75f))
                .OnComplete(() =>
                {
                    logo.gameObject.SetActive(false);
                    _battle.ReplaceFightState(EFightState.Fight);
                })
            ;
    }
}