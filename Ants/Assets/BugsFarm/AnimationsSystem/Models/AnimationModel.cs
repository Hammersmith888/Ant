using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StorageService;
using BugsFarm.Utility;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace BugsFarm.AnimationsSystem
{
    [Serializable]
    public class DictAnimReference: SerializableDictionaryBase<AnimKey, AnimationReferenceModel> {}
    
    [CreateAssetMenu(menuName = "Config/Animations/AnimationModel", fileName = "AnimationModel")]
    public class AnimationModel : ScriptableObject, IStorageItem
    {
        string IStorageItem.Id => _typeName; // If need you can make it public
        
        [SerializeField] private string _typeName;
        [SerializeField] private DictAnimReference _animations;

        public bool HasAnim( AnimKey animation )
        {
            return _animations.ContainsKey(animation);
        }
        
        public AnimationReferenceModel GetAnimModel( AnimKey animation )
        {
            return HasAnim(animation) ? _animations[animation] : default;
        }

    #region SerializeSettingHelper
    #if UNITY_EDITOR
        [ContextMenu("ExportSetting")]
        private void ExportSetting()
        {
            var config = _animations.Select(animation => new AnimModel
            {
                Key = animation.Key,
                TimeScale = animation.Value.TimeScale,
                IsLoop = animation.Value.IsLoop
            }).ToArray();
            ConfigHelper.Save(config, _typeName, true);
        }
        [ContextMenu("ImportSettings")]
        private void ImportSettings()
        {
            var config = ConfigHelper.Load<AnimModel>(_typeName);
            foreach (var animModel in config)
            {
                if(!_animations.ContainsKey(animModel.Key)) continue;
                var old = _animations[animModel.Key];
                var animation = new AnimationReferenceModel(animModel.TimeScale, animModel.IsLoop, old.ReferenceAsset);
                _animations[animModel.Key] = animation;
            }
        }
        
        [Serializable]
        private class AnimModel
        {
            public AnimKey Key;
            public float TimeScale;
            public bool IsLoop;
        }
    #endif
    #endregion
    }


}