using System.Linq;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.UnitSystem;
using BugsFarm.Utility;
using Zenject;

namespace BugsFarm.Quest
{
    public class WinWithQuestElement : QuestElement
    {
        private readonly UnitModelStorage _unitModelStorage;

        public WinWithQuestElement(string guid,
            QuestElementModelStorage questElementModelStorage,
            QuestElementDtoStorage questElementDtoStorage,
            UnitModelStorage unitModelStorage,
            IInstantiator instantiator) 
            : base(guid, 
            questElementModelStorage, 
            questElementDtoStorage, 
            instantiator)
        {
            _unitModelStorage = unitModelStorage;
        }

        public override void Initialize()
        {
            base.Initialize();
            ChooseUnitModelID();
        }

        private void ChooseUnitModelID()
        {
            if (_questDto.ReferenceID != QuestType.Any)
                return;
            var model = _unitModelStorage.Get().ElementAt(UnityEngine.Random.Range(0, 9));
            _questDto.ReferenceID = model.ModelID;
        }

        public override string GetTitleText()
        {
            var model = _questElementModelStorage.Get(_questDto.ModelID);
            var localization = LocalizationManager.Localize(model.LocalizationKey);
            return _regex.Replace(localization,  LocalizationHelper.GetBugTypeName(_questDto.ReferenceID));
        }
    }
}