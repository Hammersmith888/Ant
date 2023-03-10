using System;
using UnityEngine;

namespace BugsFarm.Locations
{
    public class LocationSceneObject : MonoBehaviour
    {
        public string Id { get; private set; } = "Default";
        [SerializeField] private SpriteRenderer[] _backGroundRenderers;

        public void Init(string id)
        {
            Id = id;
        }
        public void SetBackground(Sprite[] sprites)
        {
            if (sprites == null)
            {
                throw new ArgumentException("Sprites is Empty!");
            }
            
            if (sprites.Length > _backGroundRenderers.Length)
            {
                throw new ArgumentException($"Sprites passed count : {sprites.Length - _backGroundRenderers.Length}");
            }
            
            if (sprites.Length < _backGroundRenderers.Length)
            {
                throw new ArgumentException($"Sprites does not resolve count : {_backGroundRenderers.Length - sprites.Length}");
            }

            for (var i = 0; i < sprites.Length; i++)
            {
                var backGround = _backGroundRenderers[i];
                backGround.sprite = sprites[i];
            }
        }
    }
}
