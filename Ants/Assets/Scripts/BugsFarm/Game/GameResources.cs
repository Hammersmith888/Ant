using System;


public static class GameResources
{
    public static Action OnCoinsChanged;
    static int _coins;
    public static int Coins
    {
        get => _coins;
        set
        {
            _coins = value;

            OnCoinsChanged?.Invoke();
        }
    }


    public static Action OnCrystalsChanged;
    static int _crystals;
    public static int Crystals
    {
        get => _crystals;
        set
        {
            _crystals = value;

            OnCrystalsChanged?.Invoke();
        }
    }


    public static void Pack(GameData data)
    {
        data.coins = Coins;
        data.crystals = Crystals;
    }


    public static void Unpack(GameData data)
    {
        Coins = data.coins;
        Crystals = data.crystals;
    }


    public static int GetResource(Currency currency)
    {
        switch (currency)
        {
            case Currency.Coins: return Coins;
            case Currency.Crystals: return Crystals;
            default: return 0;
        }
    }


    public static void SetResource(Currency currency, int value)
    {
        switch (currency)
        {
            case Currency.Coins: Coins = value; break;
            case Currency.Crystals: Crystals = value; break;
        }
    }


    public static void AddResource(Currency currency, int delta)
    {
        SetResource(currency, GetResource(currency) + delta);
    }
}

