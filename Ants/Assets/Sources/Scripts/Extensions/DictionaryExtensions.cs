using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryExtensions
{
    public static string ToFullString<Key, Value>(this Dictionary<Key, Value> dictionary)
    {
        return string.Join(Environment.NewLine, dictionary);
    }

    public static WWWForm ToWWWForm(this Dictionary<string, object> dictionary)
    {
        WWWForm form = new WWWForm();
        dictionary.ForEach((item) =>
        {
            if (item.Value is int)
            {
                form.AddField(item.Key, (int)item.Value);
            } else if(item.Value is string)
            {
                form.AddField(item.Key, (string)item.Value);
            }
        });

        return form;
    }
}
