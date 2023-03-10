using UnityEngine;

namespace BugsFarm.CurrencyCollectorSystem
{
    public interface ICurrencyAnimationItem
    {
        RectTransform ParentAnimateTarget { get; }
        RectTransform ChildAnimateTarget { get; }
    }
}