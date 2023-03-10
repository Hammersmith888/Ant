using UnityEngine;

public class MB_Garden : MB_Food
{
#pragma warning disable 0649

    [SerializeField] Sprite[] _sprites;

#pragma warning restore 0649

    public void SetSprite(int stage)
    {
        SetSprite(_sprites[stage]);
    }
}

