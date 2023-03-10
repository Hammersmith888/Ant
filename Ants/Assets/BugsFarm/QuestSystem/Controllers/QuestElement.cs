using System;
using System.Text.RegularExpressions;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.Quest
{
    public class QuestElement
    {
        public string Guid => _guid;

        public event Action<string> OnQuestExpired;

        private string _guid;
        
        private readonly IInstantiator _instantiator;
        
        protected readonly QuestElementModelStorage _questElementModelStorage;
        protected readonly QuestElementDtoStorage _questElementDtoStorage;
        protected readonly Regex _regex;
        
        protected QuestElementDto _questDto;
        private SimulatedTimerTask _questElementTimer;

        private const string _pattern = "[{0}]+";
        private const string _questInProgressStateKey = "InProgress";

        public QuestElement(string guid,
                            QuestElementModelStorage questElementModelStorage,
                            QuestElementDtoStorage questElementDtoStorage,
                            IInstantiator instantiator)
        {
            _guid = guid;
            _regex = new Regex(_pattern);
            _questElementModelStorage = questElementModelStorage;
            _questElementDtoStorage = questElementDtoStorage;    
            _instantiator = instantiator;
        }

        public virtual void Initialize()
        {
            _questDto = _questElementDtoStorage.Get(_guid);
            if (_questDto.TimeLeftForDiscarding > 0.0f)
            {
                _questElementTimer = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
                _questElementTimer.Execute(_questDto.TimeLeftForDiscarding);
                _questElementTimer.SetUpdateAction(OnTimerUpdate);
                _questElementTimer.OnComplete += OnTimerExpired;
            }
            
        }

        private void OnTimerUpdate(float timeLeft)
        {
            _questDto.TimeLeftForDiscarding = timeLeft;
            if (IsCompleted())
            {
                _questElementTimer.Interrupt();
                _questElementTimer.OnComplete -= OnTimerExpired;
                _questDto.TimeLeftForDiscarding = 0.0f;
            }
        }

        private void OnTimerExpired(ITask obj)
        {
            _questDto.IsStashed = true;
            OnQuestExpired?.Invoke(Guid);
        }

        public virtual void AddProgress(int value)
        {
            _questDto.CurrentValue = Mathf.Min(_questDto.GoalValue, _questDto.CurrentValue + value);
        }
        
        public bool IsCompleted()
        {
            return _questDto.CurrentValue >= _questDto.GoalValue;
        }

        public virtual string GetTitleText()
        {
            var model = _questElementModelStorage.Get(_questDto.ModelID);
            string localization = LocalizationManager.Localize(model.LocalizationKey);
            
            if (_regex.IsMatch(localization))
            {
                return _regex.Replace(localization, _questDto.GoalValue.ToString());
            }

            return localization;
        }
        
        public void Dispose()
        {
            _questElementTimer.Interrupt();
            _questElementTimer.OnComplete -= OnTimerExpired;
        }
    }

    public enum QuestElementState
    {
        InProgress,
        Completed
    }
}
