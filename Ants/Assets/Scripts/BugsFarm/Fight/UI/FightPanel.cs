using BugsFarm.Installers;
using BugsFarm.Views.Fight.Ui;
using UnityEngine;
using UnityEngine.UI;

public class FightPanel : APanel
{
    [SerializeField] private Transform[] slots;
    [SerializeField] private SquadSelectItemView[] slotItems;
    [SerializeField] private Image foodbarImage;
    [SerializeField] private Text foodbarText;

    public void SetIcons(AntContext ant, UnitViewSettingsInstaller.UnitViewSettings unitViewSettings)
    {
        var group = ant.GetGroup(AntMatcher
            .AllOf(AntMatcher.Player, AntMatcher.InBattle)
            .NoneOf(AntMatcher.Dead));
        var index = 0;

        foreach (var entity in group.GetEntities())
        {
            var config = unitViewSettings.units[entity.antType.Value];
            var slot = slotItems[index];

            slot.Icon.sprite = config.avatar;
            slot.gameObject.SetActive(true);

            index++;
        }
    }

    #region

    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = false;
        manualClose = false;
    }

    protected override void OnOpened()
    {
    }

    #endregion
}