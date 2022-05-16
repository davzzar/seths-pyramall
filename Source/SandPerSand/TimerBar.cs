using System;
using Engine;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SandPerSand
{
    public class TimerBar : Renderer
    {
        public bool IsRunning = true;
        public const float Capacity = 1.0f;
        public const float EmptyLevel = 0.0f;
        public float DepletionSpeed { get; set; } = 0.05f;
        public float FillLevel { get; set; } = 1.0f;
        public bool ShouldSelfDestruct = false;

        // Events
        public event Action OnEmpty;
        public event Action OnFilled;

        // Drawing
        public float Width { get; set; } = 1.0f;
        public float Height { get; set; } = 0.1f;
        public Vector2 OriginOffset = Vector2.One;
        public Vector2 BorderWidth { get; set; } = new Vector2(0.1f, 0.2f);
        private float FillWidth => FillLevel * Width;
        public Color FillColor { get; set; } = Color.Magenta;
        public Color BorderColor { get; set; } = Color.White;

        private Texture2D pixel;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            pixel = new Texture2D(Graphics.GraphicsDevice, 1, 1);
            pixel.Name = "Pixel";
            pixel.SetData(new[] { Color.White });
        }

        protected override void Update()
        {
            if (!IsRunning) return;
            FillLevel = MathUtils.MoveTowards(FillLevel, EmptyLevel, DepletionSpeed * Time.DeltaTime);

            if (ShouldSelfDestruct && FillLevel <= EmptyLevel)
            {
                this.Destroy();
            }
            
            FillLevel = MathHelper.Clamp(FillLevel, EmptyLevel, Capacity);

            if (Math.Abs(FillLevel - EmptyLevel) < 1e-05)
            {
                OnEmpty?.Invoke(); 
            } 
            else if (Math.Abs(FillLevel - Capacity) < 1e-05)
            {
                OnFilled?.Invoke();
            }
        }

        public override void Draw()
        {
            var origin = Owner.Transform.Position + OriginOffset;
            var fillOrigin = new Vector2(origin.X + (FillWidth - Width) * 0.5f, origin.Y);
            Gizmos.DrawRect(origin, new Vector2(Width, Height), BorderColor);
            Gizmos.DrawRect(fillOrigin, new Vector2(FillWidth, Height), FillColor);
            
            var borderMatrix = Matrix3x3.CreateTRS(origin, 0.0f, new Vector2(Width, Height) + BorderWidth) * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));
            var fillMatrix = Matrix3x3.CreateTRS(fillOrigin, 0.0f, new Vector2(FillWidth, Height)) * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));
            Graphics.Draw(pixel, BorderColor, ref borderMatrix, 0.1f);
            Graphics.Draw(pixel, FillColor, ref fillMatrix, 0.0f);
        }

        /// <summary>
        /// Refill the bar to a given level or full by default.
        /// </summary>
        /// <param name="level"></param>
        public void RefillBar(float level = Capacity)
        {
            level = MathHelper.Clamp(level, EmptyLevel, Capacity);
            FillLevel = level;
        }

        /// <summary>
        /// Set the bar to adjust depletion speed such that it reached EmptyLevel after a duration.
        /// This method does not refill the bar.
        /// </summary>
        /// <param name="duration">duration of timer in seconds</param>
        public void SetDuration(float duration)
        {
            DepletionSpeed = FillLevel / duration; 
        }

        /// <summary>
        /// Set the bar to adjust depletion speed such that it reached EmptyLevel after a duration.
        /// Refill the bar to a given level or Capacity by default.
        /// </summary>
        /// <param name="duration"></param>
        public void SetDurationAndRefill(float duration, float level=Capacity)
        {
            this.RefillBar(level);
            this.SetDuration(duration);
        }

        public void SetRechargingAt(in float rechargeSpeed)
        {
            DepletionSpeed = -rechargeSpeed;
            IsRunning = true;
        }

        public void SetDepletingAt(in float depletionSpeed)
        {
            DepletionSpeed = depletionSpeed;
            IsRunning = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefillBar();
        }
    }
}