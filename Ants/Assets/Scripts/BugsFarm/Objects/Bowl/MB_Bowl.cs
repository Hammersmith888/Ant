using UnityEngine;
using UnityEngine.UI;


public class MB_Bowl : A_MB_Consumable
{
#pragma warning disable 0649

    public Button WorldCanvasButton;

    [SerializeField] SpriteRenderer _water;

#pragma warning restore 0649

    protected override PanelID Allowed => PanelID.HudInfoPanel;
    protected override AConsumable consumable => _bowl;


    protected BowlPresenter _bowl;

    public override void Init(APlaceable placeable) 
    {
        _bowl = (BowlPresenter)placeable;
    }


    public void OnTap() => _bowl.cb_Tapped();

    // (!) Not used
    protected override void OnTapped()
    {
        _bowl.cb_Tapped();
    }

    // (!) Not used
    // Special case - PanelID.MoveUpgrade is opened, but water is not full because ants are drinking right now!
    protected override void OnTappedSpecial() => _bowl.cb_TappedSpecial();


    protected override void OnHolded()
    {
        // OpenReplacePanel();
    }

    public void SetWaterSprite(float water_01)
    {
        Sprite sprite = _water.sprite;
        float height = sprite.texture.height / sprite.pixelsPerUnit * water_01;
        _water.size = _water.size.SetY(height);
    }
}

