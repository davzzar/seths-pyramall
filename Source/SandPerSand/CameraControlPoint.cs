using Engine;

namespace SandPerSand
{
    public sealed class CameraControlPoint : Component
    {
        public bool AffectsHorizontal { get; set; } = true;

        public bool AffectsVertical { get; set; } = true;

        public Border Margin { get; set; } = Border.Zero;

        /// <inheritdoc />
        protected override void OnAwake()
        {
            CameraController2.AddControlPoint(this);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            CameraController2.RemoveControlPoint(this);
        }
    }
}