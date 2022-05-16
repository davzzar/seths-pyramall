using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;

namespace SandPerSand
{
    class OnScreenControlController : Behaviour
    {
        private SpriteRenderer spriteRenderer;
        private Buttons button = Buttons.A;
        private Texture2D[] controlTextures;
        private float lastUpdateTime;
        private int animationIndex;

        public bool ShouldAnimate { get; set; } = true;

        private float animationSpeed = 5f;
        /// <summary>
        /// Determines animation speed in frames per second.
        /// </summary>
        public float AnimationSpeed
        {
            get => animationSpeed;
            set {
                this.animationSpeed = MathF.Max(value, 0.1f);
            }
        }

        public Buttons Button
        {
            get => button;
            set
            {
                if (button == value) return;

                button = value;
                RestartAnimation();

                // can't always load the texture. Need to delay until the game started
                if (Owner.Scene.IsLoaded && this.IsActiveInHierarchy)
                {
                    LoadControlTextures();
                }
                else
                {
                    controlTextures = null;
                }
            }
        }

        public void RestartAnimation()
        {
            lastUpdateTime = Time.GameTime;
            animationIndex = 0;
            
            if (controlTextures != null && spriteRenderer != null)
            {
                spriteRenderer.Texture = controlTextures[animationIndex];
            }
        }

        private void LoadControlTextures()
        {
            var content = GameEngine.Instance.Content;
            switch (Button)
            {
                case Buttons.A:
                    controlTextures = new Texture2D[2];
                    controlTextures[0] = content.Load<Texture2D>("GUI/button_A_up");
                    controlTextures[1] = content.Load<Texture2D>("GUI/button_A_down");
                    break;
                case Buttons.X:
                    controlTextures = new Texture2D[2];
                    controlTextures[0] = content.Load<Texture2D>("GUI/button_X_up");
                    controlTextures[1] = content.Load<Texture2D>("GUI/button_X_down");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnDisable()
        {
            spriteRenderer.Texture = null;
        }

        protected override void OnEnable()
        {
            RestartAnimation();
            spriteRenderer = Owner.GetOrAddComponent<SpriteRenderer>();
            if (controlTextures == null)
            {
                LoadControlTextures();
            }
        }

        protected override void Update()
        {
            if (controlTextures == null) return;

            // control sprite switching
            var secondsPerFrame = 1f / AnimationSpeed;

            if (ShouldAnimate && lastUpdateTime + secondsPerFrame <= Time.GameTime)
            {
                lastUpdateTime = Time.GameTime;
                animationIndex = (animationIndex + 1) % controlTextures.Length;
                spriteRenderer.Texture = controlTextures[animationIndex];
            }
        }
    }
}