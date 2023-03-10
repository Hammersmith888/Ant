using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AudioSystem
{
    public class SoundPlayer : MonoBehaviour 
    { 
        public AudioSource PlayerWhole => _audioPlayerWhole;
        public IEnumerable<AudioSource> Players => _soundPlayers;
        [SerializeField] private AudioSource _audioPlayerWhole;
        [SerializeField] private AudioSource[] _soundPlayers;
    }
}