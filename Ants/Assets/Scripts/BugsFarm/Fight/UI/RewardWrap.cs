using UnityEngine;
using UnityEngine.UI;


public class RewardWrap
{
    public int Value;


    Text _text;
    GameObject _icon;


    public RewardWrap(Text text, GameObject icon)
    {
        _text = text;
        _icon = icon;
    }


    public void SetStart(int value)
    {
        _text.transform.parent.gameObject.SetActive(value > 0); // Horizontal Layout Group

        Set(value);
    }


    public void Decrease(int decrement) => Set(Value - decrement);


    void Set(int value)
    {
        bool any = value > 0;

        Value = value;
        _text.text = value.ToString();

        _text.gameObject.SetActive(any);
        _icon.gameObject.SetActive(any);
    }
}