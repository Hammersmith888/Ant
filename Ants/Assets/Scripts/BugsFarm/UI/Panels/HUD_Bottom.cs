using System;
using System.Linq;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Game;
using BugsFarm.Objects.Stock.Utils;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD_Bottom : MonoBehaviour
{
    [Serializable]
    public class TButtons : SerializableDictionaryBase<PanelID, Button>
    {
    }

    public TButtons Buttons;
    public event EventHandler FarmButtonPressEvent;
    public event EventHandler ShopButtonPressEvent;
    public event EventHandler BugShopButtonPressEvent;

    public static HUD_Bottom Instance { get; private set; }


    void OnEnable()
    {
        /*
            Looks like Unity has a bug - button's state resets after parent disabled and then enabled again (if Button.enabled == false)
            This fixes the problem
        */
        foreach (var pair in Buttons)
        {
            Button button = pair.Value;
            bool isEnabled = button.enabled;
            button.enabled = true;
            button.enabled = isEnabled;
        }
    }


    void Awake()
    {
        Instance = Tools.SingletonPattern(this, Instance);
    }


    void Start()
    {
        //GetComponent<Animation>().Play();

        var stock = Stock.Find<FightStockPresenter>().FirstOrDefault();
        var stockExistsAndVisible = stock?.IsFightStockVisible ?? false;
        SetButtonFightInteractable(stockExistsAndVisible);

        GameEvents.OnFightStockBecameVisible += x => SetButtonFightInteractable(true);
        GameEvents.OnObjectDestroyed += x =>
        {
            if (x.Type == ObjType.Food && x.SubType == (int) FoodType.FightStock) SetButtonFightInteractable(false);
        };

        foreach (var button in Buttons)
        {
            switch (button.Key)
            {
                case PanelID.MenuShop:
                    {
                        button.Value.onClick.AddListener(() =>
                        {
                            ShopButtonPressEvent?.Invoke(this, EventArgs.Empty);
                            PlaySound();
                        });   
                    }
                    break;
                
                case PanelID.MenuFarm:
                    {
                        button.Value.onClick.AddListener(() =>
                        {
                            FarmButtonPressEvent?.Invoke(this, EventArgs.Empty);
                            PlaySound();
                        });
                    }
                    break;

                case PanelID.MenuBugs:
                    {
                        button.Value.onClick.AddListener(() =>
                        {
                            BugShopButtonPressEvent?.Invoke(this, EventArgs.Empty);
                            PlaySound();
                        });   
                    }
                    break;
            }
        }
    }


    void SetButtonFightInteractable(bool interactable)
    {
        // todo debug
        //Buttons[ PanelID.SquadSelect ].interactable			= interactable;	
        Buttons[PanelID.SquadSelect].interactable = true;
    }


    public void OnShop()
    {
        // UI_Control.Instance.Open(PanelID.MenuShop);
        // PlaySound();
    }


    public void OnFarm()
    {
        //UI_Control.Instance.Open(PanelID.MenuFarm);
        //PlaySound();
    }


    public void OnBugs()
    {
        // UI_Control.Instance.Open(PanelID.MenuBugs);
        // PlaySound();
    }


    public void OnAttack()
    {
        SceneManager.LoadScene("GlobalMap");
        PlaySound();
    }


    void PlaySound() => Sounds.Play(Sound.button_BottomHUD);


    public void SetBlock(bool block)
    {
        foreach (var pair in Buttons)
            pair.Value.enabled = !block;
    }
}