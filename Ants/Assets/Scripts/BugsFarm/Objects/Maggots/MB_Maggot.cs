using System.Collections.Generic;
using BugsFarm.AstarGraph;
using BugsFarm.SpeakerSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using Spine.Unity;
using UnityEngine;

public class MB_Maggot : MonoBehaviour
{
   // public SpeakerView  SpeakerView => _speakerView;
    [SerializeField] private SpriteRenderer _stage1;
    [SerializeField] private SpriteRenderer _stage2;
    [SerializeField] private SkeletonAnimation _stage3;
    //[SerializeField] private SpeakerView _speakerView;

    private Maggot _maggot;
    private AnimPlayer _player;

    private static readonly List<( string, float )> _weights = new List<( string, float )>
    {
        ("2", 70),       // Breath
        ("Morgaet", 15), // Blink
        ("1", 15),       // Jitter
    };

    //public void Init(Maggot maggot, PositionInfo gPos)
    //{
    //    _maggot = maggot;
    //    _player = new AnimPlayer(_stage3);
    //    // Set pos
    //    transform.position = transform.position.SetXY(gPos.Position);
    //}
    public void SetStage(int stage)
    {
        _stage1.gameObject.SetActive(stage == 0);
        _stage2.gameObject.SetActive(stage == 1);
        _stage3.gameObject.SetActive(stage == 2);
    }
    private void Update()
    {
        _maggot.Update();
        
        if ( _stage3.gameObject.activeSelf && _player.IsAnimComplete )
        {
            _player.Play(Tools.RandomWeighted(_weights), true);
        }
    }
}