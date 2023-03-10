using System.Collections.Generic;
using UnityEngine;

public static class Names
{
    public static readonly List<string> NameList = new List<string>
    {
        "Боб",
        "Кларк",
        "Метью",
        "Стьюи",
        "Дерек",
        "Стив",
        "Марк",
        "Коди",
        "Руди",
        "Адам",
        "Алан",
        "Лео",
        "Чарльз",
        "Дастин",
        "Грег",
        "Питер",
        "Брендон",
        "Патрик",
        "Гарри",
        "Харви",
        "Дональд",
        "Скот",
        "Стенли",
        "Тимм",
        "Тодд",
        "Уилсон",
        "Филипп",
        "Фред",
        "Шон",
        "Вуди",
        "Пол",
    };
    public static string GetRandom()
    {
        return NameList[Random.Range(0, NameList.Count)];
    }
}

