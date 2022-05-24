using Engine;

namespace SandPerSand
{
    public sealed class CameraControlPoint : Behaviour
    {
        public bool AffectsHorizontal { get; set; } = true;

        public bool AffectsVertical { get; set; } = true;

        public Border Margin { get; set; } = Border.Zero;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            CameraController2.AddControlPoint(this);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            CameraController2.RemoveControlPoint(this);
        }
    }
}