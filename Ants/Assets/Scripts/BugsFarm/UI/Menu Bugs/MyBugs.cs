using System.Collections.Generic;
using System.Linq;
using BugsFarm.Game;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;
using UnityEngine.UI;


public class MyBugs : MonoBehaviour
{
    public static MyBugs Instance { get; private set; }

#pragma warning disable 0649

    [SerializeField] MyBug[] _icons;
    [SerializeField] GameObject _button_PrevPage;
    [SerializeField] GameObject _button_NextPage;

    [SerializeField] RectTransform _root_Pages;
    [SerializeField] Transform _selectedPage;
    [SerializeField] GameObject _prefab_Page;

    [SerializeField] GameObject _MyBugs;
    [SerializeField] BugInfo _bugInfo;

#pragma warning restore 0649


    int _page;

    List<GameObject> _pages = new List<GameObject>();

    List<AntPresenter> _alive = new List<AntPresenter>();


    public void Init()
    {
        _bugInfo.Init();

        Instance = Tools.SingletonPattern(this, Instance);

        GameEvents.OnAntSpawned += Refresh;
        GameEvents.OnAntDied += (ant, reason) => Refresh();
        GameEvents.OnAntDestroyed += Refresh;
        GameEvents.OnGameReset += Refresh;
        GameEvents.OnSimulationEnd += Refresh;
    }


    void OnEnable()
    {
        _bugInfo.Refresh();
    }


    void Refresh(AntPresenter ant) => Refresh();

    public void Refresh()
    {
        if (SimulationOld.Type != SimulationType.None)
            return;

        Setup();
    }


    public void Setup()
    {
        _alive.Clear();
        _alive.AddRange(Keeper.Ants.Where(x => x.IsAlive));

        int nIcons = _icons.Length;
        int nPages = Tools.Ceil(_alive.Count, nIcons);
        int pageMax = nPages - 1;

        _page = Mathf.Min(_page, pageMax);

        for (int i = 0; i < nIcons; i++)
        {
            AntPresenter ant = _alive.ElementAtOrDefault(_page * nIcons + i);
            MyBug ui = _icons[i];

            ui.Set(ant);
        }


        _button_PrevPage.SetActive(_page > 0);
        _button_NextPage.SetActive(_page < pageMax);


        while (_pages.Count != nPages)
        {
            if (_pages.Count < nPages)
            {
                _pages.Add(Instantiate(_prefab_Page, _root_Pages));
            }
            else
            {
                int last_i = _pages.Count - 1;

                Destroy(_pages[last_i].gameObject);

                _pages.RemoveAt(last_i);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_root_Pages);


        _selectedPage.position = _pages[_page].transform.position;
    }


    public void OnPage(int delta)
    {
        _page += delta;

        Setup();
    }


    public void OpenClose(bool open)
    {
        _MyBugs.SetActive(open);
    }
}