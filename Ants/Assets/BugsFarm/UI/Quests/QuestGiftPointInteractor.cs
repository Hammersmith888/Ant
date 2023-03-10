using System;
using BugsFarm.ChestSystem;
using BugsFarm.Quest;
using Zenject;

namespace BugsFarm.UI
{
    public class QuestGiftPointInteractor
    {
        
        public event Action<VirtualChestDto> OnCollectRewardButtonClicked;

        private readonly IInstantiator _instantiator;
        private readonly UIQuestGiftPoint _questGiftPoint;
        private VirtualChestDto _virtualChestDto;
        private string _chestModelID;


        public QuestGiftPointInteractor(UIQuestGiftPoint questGiftPoint,
                                        IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _questGiftPoint = questGiftPoint;
        }

        public void Initialize()
        {
            _questGiftPoint.Initialize();
            _questGiftPoint.OnCollectButtonClicked += NotifyCollectButtonClicked;
        }

        public void SetVirtualChestDto(VirtualChestDto virtualChestDto, float currentProgress)
        {
            _virtualChestDto = virtualChestDto;
            UpdateGiftPoint(currentProgress);
        }
        
        private void NotifyCollectButtonClicked()
        {
            OnCollectRewardButtonClicked?.Invoke(_virtualChestDto);
        }

        public void UpdateGiftPoint(float currentProgress)
        {
            if (_virtualChestDto.IsOpened)
            {
                _questGiftPoint.SetActiveIcon(false);
                return;
            }
            
            _questGiftPoint.SetActiveIcon(true);
            
            if (currentProgress >= _virtualChestDto.Treshold)
            {
                _questGiftPoint.SwitchToChestState();
            }
            else
            {
                _questGiftPoint.SwitchToGiftState();
            }
        }

        public void Dispose()
        {
            _questGiftPoint.OnCollectButtonClicked -= NotifyCollectButtonClicked;
            _questGiftPoint.Dispose();
        }
    }
}