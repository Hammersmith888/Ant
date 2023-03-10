using System;
using System.Collections.Generic;


/*
	https://stackoverflow.com/questions/10816803/finding-next-available-key-in-a-dictionary-or-related-collection
*/


[Serializable]
public class AutoKeys
{
    readonly HashSet<int> _usedKeys = new HashSet<int>();
    int _maxKey = 0;


    public int GetFreeKey()
    {
        int key;

        using (var it = _usedKeys.GetEnumerator())
        {
            if (it.MoveNext())
            {
                key = it.Current;

                _usedKeys.Remove(key);
            }
            else
            {
                key = ++_maxKey;
            }
        }

        return key;
    }


    public void ReturnKey(int key)
    {
        _usedKeys.Add(key);
    }
}

