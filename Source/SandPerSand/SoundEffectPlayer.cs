using System;
using Engine;
using Microsoft.Xna.Framework.Audio;

namespace SandPerSand
{
    public sealed class SoundEffectPlayer : Behaviour
    {
        private string loadFromContentPath;

        private float playStart = float.NegativeInfinity;

        public Func<bool> Trigger { get; set; }

        public SoundEffect Sound { get; set; }

        public float Volume { get; set; } = 1f;

        public float Pitch { get; set; } = 0f;

        public float Pan { get; set; } = 0f;

        public void LoadFromContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.loadFromContentPath = path;
            this.LoadFromContentPath();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            if (this.Trigger == null || this.Sound == null)
            {
                return;
            }

            if (this.playStart + this.Sound.Duration.TotalSeconds < Time.GameTime && this.Trigger.Invoke())
            {
                this.Sound.Play(this.Volume, this.Pitch, this.Pan);
                this.playStart = Time.GameTime;
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            this.LoadFromContentPath();
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPath != null && this.Owner.Scene.IsLoaded && this.IsActiveInHierarchy)
            {
                this.Sound = GameEngine.Instance.Content.Load<SoundEffect>(this.loadFromContentPath);
                this.loadFromContentPath = null;
            }
        }
    }
}
