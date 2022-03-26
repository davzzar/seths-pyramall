using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    /// <summary>
    /// Lets the owning game object sway left and right like a windshield wiper.<br/>
    /// Takes control of Owner.<see cref="Transform.LocalRotation"/>
    /// </summary>
    public class SwayComponent : Behaviour
    {
        private float defaultRotation;
        private float currentSway;
        private float maxSway;
        private float swaySpeed;
        private int direction;

        public float DefaultRotation
        {
            get => defaultRotation;
            set => defaultRotation = MathHelper.WrapAngle(value);
        }

        public float CurrentSway
        {
            get => currentSway;
            set => currentSway = MathHelper.Clamp(value, -this.MaxSway, this.MaxSway);
        }

        public float MaxSway
        {
            get => maxSway;
            set => maxSway = MathHelper.Clamp(value, 0f, MathHelper.Pi);
        }

        public float SwaySpeed
        {
            get => swaySpeed;
            set => swaySpeed = MathHelper.Clamp(value, 0.001f, float.MaxValue);
        }

        public SwayComponent()
        {
            this.defaultRotation = 0f;
            this.currentSway = 0f;
            this.maxSway = MathHelper.Pi * 0.5f;
            this.SwaySpeed = MathHelper.Pi * 45f / 180f;
            this.direction = 1;
        }

        protected override void Update()
        {
            var sway = this.SwaySpeed * Time.DeltaTime;

            this.currentSway += this.direction * sway;
            
            if (this.direction * this.currentSway >= this.maxSway)
            {
                this.currentSway = this.direction * this.maxSway;
                this.direction *= -1;
            }

            this.Transform.LocalRotation = this.defaultRotation + this.currentSway;
        }
    }
}