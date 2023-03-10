using BugsFarm.Views.Core;
using UnityEngine.SceneManagement;

namespace BugsFarm.Views.Fight
{
    public class GlobalMapView : AView
    {
        public void OnSeason(int season)
        {
            if (season == 1)
                SceneManager.LoadScene("Battle Season 1", LoadSceneMode.Additive);
            else
                SceneManager.LoadScene("Battle Season 2", LoadSceneMode.Additive);
        }
    }
}