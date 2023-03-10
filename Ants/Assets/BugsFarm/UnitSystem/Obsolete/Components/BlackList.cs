using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace BugsFarm.UnitSystem.Obsolete.Components
{
    public class BlackList : IDisposable
    {
        private readonly List<object> _list;
        private const float _autoReleaseTimer = 20f;
        private readonly CompositeDisposable _disposables;
        public BlackList()
        {
            _list = new List<object>();
            _disposables = new CompositeDisposable();
        }
        public void Dispose()
        {
            _disposables?.Dispose();
        }
        public void Add(params object[] objects)
        {
            foreach (var obj in objects)
            {
                if(!_list.Contains(obj))
                {
                    _list.Add(obj);
                }
            }
            _disposables.Add(Observable.Timer(TimeSpan.FromSeconds(_autoReleaseTimer)).Subscribe(x =>
            {
                foreach (var obj in objects)
                {
                    Remove(obj);
                }
            }));
        }
        public void Remove(params object[] objects)
        {
            foreach (var obj in objects)
            {
                _list.Remove(obj);
            }
        }
        public bool HasObjects(params object[] objects)
        {
            return _list.Any(objects.Contains);
        }
        public IEnumerable<object> Get()
        {
            return _list;
        }
        public void Release()
        {
            _list.Clear();
        }
    }
}