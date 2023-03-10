using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using UnityEngine;


public class Refs : MB_Singleton< Refs >
{
	public static Sprite		sprite_Coin			=> Instance._sprite_Coin;
	public static Sprite		sprite_Crystal		=> Instance._sprite_Crystal;

	public MusicPlayer	MusicFarm;
	public MusicPlayer	MusicBattle;


#pragma warning disable 0649

    [SerializeField] Sprite		_sprite_Coin;
    [SerializeField] Sprite		_sprite_Crystal;

    [SerializeField] Color		_color_Coins;
    [SerializeField] Color		_color_Crystals;

#pragma warning restore 0649


	public Sprite GetCurrencySprite( Currency currency )
	{
		switch (currency)
		{
			case Currency.Coins:		return _sprite_Coin;
			case Currency.Crystals:		return _sprite_Crystal;
			default:					return null;
		}
	}


	public Color GetCurrencyColor( Currency currency )
	{
		switch (currency)
		{
			case Currency.Coins:		return _color_Coins;
			case Currency.Crystals:		return _color_Crystals;
			default:					return Color.magenta;
		}
	}
}

