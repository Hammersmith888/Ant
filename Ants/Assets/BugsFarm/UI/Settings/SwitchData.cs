using System;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public interface ISwitchData
    {
        int Count { get; }
    }
    [Serializable]
    public class SwitchData<TObject, TValue> : ISwitchData
    {
        [Tooltip("Целевой объект для изменения")]
        [SerializeField] private TObject _targetObject = default;
        [Tooltip("Компоненты целевого объекта")]
        [SerializeField] private TValue[] _switchList = default;
        public TObject TargetObject => _targetObject;
        public TValue[] SwitchList => _switchList;
        public int Count => _switchList.Length;
    }

    [Serializable]
    public class SpriteSwitchData : SwitchData<SpriteRenderer, Sprite> { }

    [Serializable]
    public class ImageSwitchData : SwitchData<Image, Sprite> { }
}