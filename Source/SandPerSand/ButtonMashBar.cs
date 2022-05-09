using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SandPerSand
{
    enum ButtonMashBarState
    {
        Idile = 0,
        Depleting = 1,
        Increasing = 2,
        Filled = 3,
        Empty = 4
    }

    class ButtonMashBar : Renderer
    {
        // A Button Mash Bar has a capacity of 1 and deals with percentages.

        public InputHandler InputHandler { get; set; }
        private const Buttons ActionButton = Buttons.LeftShoulder;
        private bool ActionButtonPressed => InputHandler.getButtonState(ActionButton) == ButtonState.Pressed;

        public const float Capacity = 1.0f;
        public const float EmptyLevel = 0.0f;
        public float DepletionSpeed { get; set; } = 0.05f;
        public float FillSpeed { get; set; } = 0.1f;
        public float FillLevel { get; set; } = 0.5f;

        // Drawing
        public float Width { get; set; } = 1.0f;
        public float Height { get; set; } = 0.1f;
        public Vector2 OriginOffset = Vector2.One;
        public Vector2 BorderWidth { get; set; } = new Vector2(0.1f, 0.2f);
        private float FillWidth => FillLevel * Width;
        public Color FillColor { get; set; } = Color.Magenta;
        public Color BorderColor { get; set; } = Color.White;
        public Color FilledStateColor { get; set; } = Color.Green;
        public Color EmptyStateColor { get; set; } = Color.Red;
        public Color DepletingStateColor { get; set; } = Color.Magenta;

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
            FillLevel -= DepletionSpeed * Time.DeltaTime;

            if (ActionButtonPressed) FillLevel += FillSpeed;

            FillLevel = MathUtils.MoveTowards(FillLevel, EmptyLevel, DepletionSpeed * Time.DeltaTime);

            FillLevel = MathHelper.Clamp(FillLevel, EmptyLevel, Capacity);

            InputHandler.UpdateState();

            Debug.WriteLine($"Masher Fill Level: {FillLevel}");
        }

        public override void Draw()
        {
            var origin = Owner.Transform.Position + OriginOffset;
            var fillOrigin = new Vector2(origin.X + (FillWidth - Width) * 0.5f, origin.Y);
            Gizmos.DrawRect(origin, new Vector2(Width, Height), BorderColor);
            Gizmos.DrawRect(fillOrigin, new Vector2(FillWidth, Height), FillColor);
            
            var borderMatrix = Matrix3x3.CreateTRS(origin, 0.0f, new Vector2(Width, Height) + BorderWidth) * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));
            var fillMatrix = Matrix3x3.CreateTRS(fillOrigin, 0.0f, new Vector2(FillWidth, Height)) * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));
            Graphics.Draw(pixel, BorderColor, ref borderMatrix, 0.2f);
            Graphics.Draw(pixel, FillColor, ref fillMatrix, 0.0f);
        }
    }
}