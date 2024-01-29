using System;
using System.Collections.Generic;
using LSCore.LevelSystem;
using Sirenix.Serialization;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseEffector
    {
        [OdinSerialize] protected FindTargetComp findTargetComp;
        [SerializeField] private LevelsManager levelsManager;
        [NonSerialized] public Id id;
        protected float radius;
        protected bool isApplied;
        protected SpriteRenderer radiusRenderer;
        private static Sprite circleSprite = Resources.Load<Sprite>("unit-circle");
        
        [field: SerializeField] public int Fund { get; private set; }
        protected virtual bool NeedFindOpponent => true;
        private Dictionary<Type, Prop> props;
        
        protected float GetValue<T>() where T : FloatGameProp
        {
            return FloatGameProp.GetValue<T>(props);
        }

        public void Init()
        {
            props = levelsManager.GetProps(id);
            radius = GetValue<RadiusGP>();
            radiusRenderer = new GameObject($"{GetType().Name} Radius").AddComponent<SpriteRenderer>();
            radiusRenderer.sprite = circleSprite;
            var gameObject = radiusRenderer.gameObject;
            gameObject.SetActive(false);
            //findTargetComponent.Init(gameObject.transform);
            OnInit();
        }

        public void DrawRadius(Vector3 position, Color color, int sortingOrder = 0)
        {
            radiusRenderer.gameObject.SetActive(true);
            radiusRenderer.color = color;
            radiusRenderer.sortingOrder = sortingOrder;
            var transform = radiusRenderer.transform;
            transform.position = position;
        }

        public void EndDrawRadius()
        {
            if (!isApplied)
            {
                radiusRenderer.gameObject.SetActive(false);
            }
        }

        public void Apply()
        {
            isApplied = true;
            OnApply();
        }
        
        protected virtual void OnInit(){}
        protected abstract void OnApply();
    }
}