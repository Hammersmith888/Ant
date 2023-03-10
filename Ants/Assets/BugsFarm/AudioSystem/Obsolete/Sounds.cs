using System;
using System.Collections.Generic;
using BugsFarm.SimulationSystem.Obsolete;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace BugsFarm.AudioSystem.Obsolete
{
    public enum Sound
    {
        None = 0,

        Worker_Attack1 = 1,
        Worker_Attack2 = 2,
        Pikeman_Attack1 = 3,
        Pikeman_Attack2 = 4,
        Spider_Attack1 = 17,
        Spider_Attack2 = 18,

        Archer_Shoot1 = 5,
        Archer_Shoot2 = 6,
        Archer_BowString1 = 7,
        Archer_BowString2 = 8,
        Archer_ArrowHit1 = 9,
        Archer_ArrowHit2 = 10,

        PotatoBug_Attack1 = 11,
        PotatoBug_Attack2 = 12,
        Cockroach_Attack1 = 13,
        Cockroach_Attack2 = 14,
        Worm_Attack1 = 15,
        Worm_Attack2 = 16,

        CoinBubble_1 = 19,
        CoinBubble_2 = 20,
        GroundToPile_1 = 21,
        GroundToPile_2 = 22,
        button_BottomHUD = 23,
        Dig_1 = 24,
        Dig_2 = 25,
        Dig_3 = 26,
        Eat_1 = 27,
        Eat_2 = 28,
        Water = 29,
        PutObject = 30,
        PutFoodOrPlant = 31,
        DeleteObject = 32,
        Gardening_1 = 33,
        Gardening_2 = 34,
        PikemanTraining_1 = 35,
        PikemanTraining_2 = 36,
        PikemanTraining_3 = 37,
        Mattock_1 = 38,
        Mattock_2 = 39,
        Mattock_3 = 40,
        GoldmineRockLift = 41,

        JingleVictory = 42,
        JingleDefeat = 43,

        ChestOpen = 44,
        Coin = 45,

        Hammer = 46,
        Saw = 47,
    }


    [Serializable]
    public class SoundData
    {
        public AudioClip clip;
        public float volumeScale = 1;
    }


    public class Sounds : MB_Singleton<Sounds>
    {
        [Serializable]
        public class TSounds : SerializableDictionaryBase<Sound, SoundData> { }

        [SerializeField] TSounds _sounds;
        [SerializeField] AudioSource _audioSource;
        [SerializeField] float _reverb_Farm;
        [SerializeField] float _reverb_Fight;
        [SerializeField] MusicPlayer _musicA;
        [SerializeField] MusicPlayer _musicB;

        private static readonly HashSet<Sound> _played = new HashSet<Sound>();

        public void SetReverb()
        {
            //todo fix
            //if (!_game.hasFightState)
            //    return;
            //if (_game.fightState.Value == EFightState.None || _game.fightState.Value == EFightState.SquadSelect)
            _audioSource.reverbZoneMix = _reverb_Farm;
            //else
            //    _audioSource.reverbZoneMix = _reverb_Fight;
        }

        public static void Play(Sound sound) => Instance._Play(sound);
        public static void PlayPutSound(APlaceable placeable) => PlayPutSound(placeable.Type, placeable.SubType);
        public static void PlayPutSound(ObjType type, int subType) => Play(GetPutSound(type, subType));
        public void SetMuteSounds(bool mute)
        {
            _audioSource.mute = mute;
        }
        public void SetMuteMusic(bool mute)
        {
            _musicA.SetMute(mute);
            _musicB.SetMute(mute);
        }

        private void Start() => SetReverb();
        private void LateUpdate() => _played.Clear();
        private void _Play(Sound sound)
        {
            if ( SimulationOld.Type != SimulationType.None || _played.Contains(sound) || !_sounds.ContainsKey(sound))
            {
                return;
            }


            SoundData data = _sounds[sound];

            _audioSource.PlayOneShot(data.clip, data.volumeScale);

            _played.Add(sound);
        
        }
        private static Sound GetPutSound(ObjType type, int subType)
        {
            switch (type)
            {
                case ObjType.Food: return Sound.PutFoodOrPlant;

                case ObjType.Decoration:
                    switch ((DecorType)subType)
                    {
                        case DecorType.Flower:
                        case DecorType.Grass_1:
                        case DecorType.Grass_2:
                            // case DecorType.Mushrooms:
                            return Sound.PutFoodOrPlant;

                        default: return Sound.PutObject;
                    }

                default: return Sound.PutObject;
            }
        }
    }
}