using System;
using Com.LuisPedroFonseca.ProCamera2D;
using Malee.List;
using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    public class CinematicPresset : MonoBehaviour
    {
        [Serializable]
        private class ReorderableCinematicTartets : ReorderableArray<CinematicTarget>{}
        public CinematicTarget[] Targets => _targets.ToArray();
        public float DelayBeforStart => _delayBeforStart;
        public bool StayOnLastReachTarget => _stayOnLastReachTarget;
        public bool WaitUserActionToEnd => _waitUserActionToEnd;
        public bool UseNumetricBoundaries => _useNumetricBoundaries;
        public bool UseLetterBox => _useLetterBox;
        public float LetterBoxDuration => _letterBoxDuration;
        public float LetterBoxAmmout => _letterBoxAmmout;
        public Color LetterBoxColor => _letterBoxColor;
        
        [SerializeField] private float _delayBeforStart = 0f;
        [SerializeField] private bool _stayOnLastReachTarget = false;
        [SerializeField] private bool _waitUserActionToEnd = false;
        
        
        [Tooltip("Можно ли колизится камере в границы")]
        [SerializeField] private bool _useNumetricBoundaries = false;
        

        [Header("LetterBox Top/Down")]
        [SerializeField] private bool  _useLetterBox = false;
        [SerializeField] private float _letterBoxDuration = 1f;
        [SerializeField] private float _letterBoxAmmout = 0.1f;
        [SerializeField] private Color _letterBoxColor = Color.black;

        [Header("Cinematic scenario")]
        [Reorderable] [SerializeField] private ReorderableCinematicTartets _targets;
    }
}