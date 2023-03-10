using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services;
using BugsFarm.UnitSystem;
using BugsFarm.Views.Core;
using BugsFarm.Views.Screen;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BugsFarm.Views.Fight.Ui
{
    // todo: change to uiService
    public class UiSquadSelectView : AUiView
    {
        [SerializeField] private SquadSelectItemView prefab;
        [SerializeField] private Transform content;
        [SerializeField] private Transform dragItemParent;
        [SerializeField] private Image foodbarImage;
        [SerializeField] private Text foodbarText;
        [SerializeField] private SquadSelectSlot[] slots;
        [SerializeField] private Button buttonGoFight;
        [SerializeField] private Button buttonPlus;
        [SerializeField] private Button buttonClose;
        [SerializeField] private GameObject emptyItem;

        public Transform Content => content;
        public Transform DragItemParent => dragItemParent;
        public SquadSelectSlot[] Slots => slots;
        public GameObject EmptyItem => emptyItem;

        private BattleService _battleService;
        private FightView _fightView;
        private UnitDtoStorage _unitDtoStorage;
        private UnitModelStorage _unitModelStorage;
        private IInstantiator _instantiator;
        
        [Inject] 
        private void Inject(
            IInstantiator instantiator,
            UnitDtoStorage unitDtoStorage,
            UnitModelStorage unitModelStorage,
            BattleService battleService, 
            FightView fightView)
        {
            _unitDtoStorage = unitDtoStorage;
            _unitModelStorage = unitModelStorage;
            _battleService = battleService;
            _fightView = fightView;
            _instantiator = instantiator;
        }

        private void Start()
        {
            var allUnits = _unitDtoStorage.Get();
            foreach (var dtoUnit in allUnits)
            {
                var unit = _instantiator.InstantiatePrefabForComponent<SquadSelectItemView>(prefab, content);
                var model = _unitModelStorage.Get(dtoUnit.ModelID);
                //unit.AntType = (AntType) model.TypeID;
            }
        }

        public void OnButtonDone()
        {
            _fightView.MainCamera.transform.position = new Vector3(0, .6f, -10);

            var ants = new Dictionary<AntType, int>();

            foreach (var squadSelectSlot in slots)
            {
                var squadSelectItemView = squadSelectSlot.GetComponentInChildren<SquadSelectItemView>();
                if (squadSelectItemView == null)
                    continue;

                if (ants.ContainsKey(squadSelectItemView.AntType))
                    ants[squadSelectItemView.AntType]++;
                else
                    ants.Add(squadSelectItemView.AntType, 1);
            }

            var fightScreen = GetComponentInParent<FightScreen>();
            fightScreen.FightPanel.gameObject.SetActive(true);
            
            _battleService.StartBattle(ants);
            gameObject.SetActive(false);
        }

        public void SetDoneButtonInteract()
        {
            var count = slots.Count(squadSelectSlot => squadSelectSlot.transform.childCount > 0);
            buttonGoFight.interactable = count > 0;
        }
    }
}