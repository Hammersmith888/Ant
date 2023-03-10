using System;
using UnityEngine;

namespace BugsFarm.UI
{
    /// <summary>
    /// Позволяет менять разные состояния объекта заменяя его компонент или группу компонентов.
    /// </summary>
    /// <typeparam name="TSwitchData"> TSwitchData должа быть помечена [Serializable] для отображения в инспекторе </typeparam>
    public abstract class Switcher<TSwitchData> : MonoBehaviour where TSwitchData : ISwitchData
    {
        public bool ReverseDirection => _reverseDirection;
        [Tooltip("Меняет направление переключателя, по умолчанию выключено - вперед")]
        [SerializeField] private bool _reverseDirection = false;
        [Tooltip("Текущее состояние")]
        [SerializeField] private int _state = 0;
        [Tooltip("Массив состояний")]
        [SerializeField] private TSwitchData[] _objectList = null;

        public abstract void Init();
        public event Action<int> OnSwitchState;
        public void Switch(int? state = null)
        {
            foreach (var item in _objectList)
            {
                // объект не имеет состояний или не назначен в инспекторе, пропускаем
                if (item.IsNullOrDefault() || item.Count == 0)
                {
                    continue;
                }
                // установить принудительно если возможно
                _state = state ?? _state;

                // перенос индекса в исходную по направлению
                if (_state >= item.Count || _state < 0)
                {
                    _state = _reverseDirection ? item.Count - 1 : 0;
                }

                // переключить состояние
                ToSwitch(item, _state);
            }

            // оповестить что состояние изменилось
            OnSwitchState?.Invoke(_state);
            _state = _reverseDirection ? _state-- : _state++;
        }
        protected abstract void ToSwitch(TSwitchData data, int state);
    }
}

