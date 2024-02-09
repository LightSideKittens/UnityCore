using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace BeatHeroes
{
    [RequireComponent(typeof(SpriteRenderer))]
    public partial class SpriteAtlasSequencer : MonoBehaviour
    {
        [Serializable]
        private struct AnimationData
        {
            private SpriteRenderer spriteRenderer;
            [ReadOnly] public SpriteAtlas spriteAtlas;
            private Sprite[] sprites;
            private int currentSprite;
            private float timePerFrame;
            private float time;

            public void Init(SpriteRenderer spriteRenderer, int framePerSecond)
            {
                this.spriteRenderer = spriteRenderer;
                sprites = new Sprite[spriteAtlas.spriteCount];
                for (int i = 0; i < spriteAtlas.spriteCount; i++)
                {
                    var name = (i+1).ToString("0000");
                    sprites[i] = spriteAtlas.GetSprite(name);
                }

                timePerFrame = 1f / framePerSecond;
            }
            
            public void Update()
            {
                time += Time.deltaTime;
                
                if (time > timePerFrame)
                {
                    spriteRenderer.sprite = sprites[currentSprite++ % sprites.Length];
                    time = 0;
                }
            }
        }
        
        [SerializeField] private string animName;
        [SerializeField] private int framePerSecond = 24;
        [SerializeField] private AnimationData forward;
        [SerializeField] private AnimationData forwardLeft;
        [SerializeField] private AnimationData forwardRight;
        [SerializeField] private AnimationData back;
        [SerializeField] private AnimationData backLeft;
        [SerializeField] private AnimationData backRight;
        [SerializeField] private AnimationData left;
        [SerializeField] private AnimationData right;
        
        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            forward.Init(spriteRenderer, framePerSecond);
            forwardLeft.Init(spriteRenderer, framePerSecond);
            forwardRight.Init(spriteRenderer, framePerSecond);
            back.Init(spriteRenderer, framePerSecond);
            backLeft.Init(spriteRenderer, framePerSecond);
            backRight.Init(spriteRenderer, framePerSecond);
            left.Init(spriteRenderer, framePerSecond);
            right.Init(spriteRenderer, framePerSecond);
        }
    }
}
