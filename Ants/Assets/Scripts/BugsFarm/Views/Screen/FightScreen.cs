using BugsFarm.Views.Fight.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.Views.Screen
{
    public class FightScreen : MonoBehaviour
    {
        [SerializeField] private APanel winPanel;
        [SerializeField] private APanel failPanel;
        [SerializeField] private FightPanel fightPanel;
        [SerializeField] private Image veil;
        [SerializeField] private Button retreatButton;
        [SerializeField] private Button backButton;
        [SerializeField] private BoosterButtonView armorButton;
        [SerializeField] private BoosterButtonView attackButton;
        [SerializeField] private Transform damagesContainer;
        [SerializeField] private CanvasScaler canvasScaler;

        public APanel WinPanel => winPanel;
        public APanel FailPanel => failPanel;
        public FightPanel FightPanel => fightPanel;
        public Image Veil => veil;
        public Button RetreatButton => retreatButton;
        public Transform DamagesContainer => damagesContainer;
        public Button BackButton => backButton;
        public BoosterButtonView ArmorButton => armorButton;
        public BoosterButtonView AttackButton => attackButton;
        public CanvasScaler CanvasScaler => canvasScaler;
    }
}