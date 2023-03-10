using BugsFarm.Views.Core;
using Entitas.Unity;
using UnityEngine;

namespace BugsFarm.Views.Fight
{
    public class ArrowView : AView
    {
        [SerializeField] private bool isEnabled;
        [SerializeField] private float y;
        [SerializeField] private float fadeTimer;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        public float Y
        {
            get => y;
            set => y = value;
        }

        public float FadeTimer
        {
            get => fadeTimer;
            set => fadeTimer = value;
        }

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isEnabled)
                return;
            
            if (other.CompareTag("Enemy"))
            {
                isEnabled = false;
                transform.parent = other.transform;

                var mbUnit = other.GetComponent<MB_Unit>();
                if (mbUnit == null)
                    mbUnit = other.GetComponentInParent<MB_Unit>();

                var enemyEntity = (AntEntity) mbUnit.gameObject.GetEntityLink().entity;
                if (!enemyEntity.hasDamage)
                    //todo fix
                    enemyEntity.AddDamage(2f);
                else
                {
                    var damage = enemyEntity.damage.Value;
                    damage += 1f;
                    enemyEntity.ReplaceDamage(damage);
                }
            }
        }
    }
}