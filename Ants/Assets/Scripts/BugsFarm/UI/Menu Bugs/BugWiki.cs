using UnityEngine;
using UnityEngine.UI;


public class BugWiki : MonoBehaviour
{
    public static BugWiki Instance { get; private set; }


#pragma warning disable 0649

    [SerializeField] Text _header;
    [SerializeField] Text _description;

#pragma warning restore 0649


    public void Init()
    {
        Instance = Tools.SingletonPattern(this, Instance);
    }


    public void Open(AntType type)
    {
        Wiki wiki = Data_Ants.Instance.GetData(type).wiki;

        _header.text = wiki.Header;
        _description.text = wiki.wiki;

        OpenClose(true);
    }


    public void OpenClose(bool open)
    {
        gameObject.SetActive(open);
    }


    public void OnOK()
    {
        OpenClose(false);

        MyBugs.Instance.OpenClose(true);
    }
}