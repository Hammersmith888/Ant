using System;
using System.Collections.Generic;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;

[Serializable]
public abstract class APublisher : IDisposable
{
    protected HashSet<ATask> _subscribers = new HashSet<ATask>();
    public void SetSubscriber(ATask subscriber, bool add)
    {
        if (add)
            _subscribers.Add(subscriber);
        else
            _subscribers.Remove(subscriber);
    }
    public virtual void Dispose()
    {
        CastObjectEvent(ObjEvent.Destroyed);
    }

    protected virtual void CastObjectEvent(ObjEvent objEvent)
    {
        // Collection can be modified, so have to use _tmp
        var _tmpSubscribers = new HashSet<ATask>(_subscribers);
        foreach (ATask subscriber in _tmpSubscribers)
            subscriber.ObjectEvent?.Invoke(this, objEvent);
    }
}

