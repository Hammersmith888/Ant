using UnityEngine;

namespace BugsFarm.UI
{
    [CreateAssetMenu(fileName = "StatUIConfig", menuName = "Config/BugStatUIConfig", order = 0)]
    public class StatUIConfig : ScriptableObject
    {
        [SerializeField] private string[] statsToShow;

        public string[] StatsToShow => statsToShow;
    }
}