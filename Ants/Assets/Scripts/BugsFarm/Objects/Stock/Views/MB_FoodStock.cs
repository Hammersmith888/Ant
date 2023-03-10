using UnityEngine;


public class MB_FoodStock : MB_Food
{
    public override void SetSprite(Sprite sprite)
    {
        base.SetSprite(sprite);
        // SetColliderAllowed(sprite != null);
    }
}

