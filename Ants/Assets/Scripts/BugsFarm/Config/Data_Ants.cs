using System;
using System.Collections.Generic;
using System.Linq;
using Malee.List;
using UnityEngine;

public class Data_Ants : MB_Singleton<Data_Ants>
{
    public MB_Maggot PrefabMaggot => _prefabMaggot;
    [SerializeField] private MB_Maggot _prefabMaggot;
    [Reorderable] [SerializeField] private CofigAntList _configList;
    
    public bool HasData(AntType antType)
    {
        return _configList.Any(x => x.antType == antType);
    } 
    public CfgAnt GetData(AntType antType)
    {
        return _configList?.FirstOrDefault(x => x.antType == antType);
    } 
    public IEnumerable<CfgAnt> GetDatas(params AntType[] exclude)
    {
        return _configList.Where(x=> !exclude.Contains(x.antType));
    }
    [Serializable] private class CofigAntList : ReorderableArray<CfgAnt>{}
}