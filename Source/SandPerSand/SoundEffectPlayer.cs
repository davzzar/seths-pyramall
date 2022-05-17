using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace SandPerSand
{
    public sealed class SoundEffectPlayer : Behaviour
    {
        private string[] loadFromContentPaths;

        private float lockTime = float.NegativeInfinity;

        private Random random = new Random();
        private float volume = 1f;
        private float pitch = 0f;
        private float pan = 0f;

        public Func<bool> Trigger { get; set; }

        public List<SoundEffect> Sounds { get; } = new List<SoundEffect>();

        public float Volume
        {
            get => this.volume;
            set => this.volume = MathHelper.Clamp(value, 0f, 1f);
        }

        public float Pitch
        {
            get => this.pitch;
            set => this.pitch = MathHelper.Clamp(value, 0f, 1f);
        }

        public float Pan
        {
            get => this.pan;
            set => this.pan = MathHelper.Clamp(value, -1f, 1f);
        }

        public bool AllowOverlappingPlays { get; set; }

        public void LoadFromContent(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (paths.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                throw new ArgumentException("All paths must be valid.");
            }

            this.loadFromContentPaths = paths;
            this.LoadFromContentPath();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            if ((this.Trigger == null) || this.Sounds.Count == 0)
            {
                return;
            }

            // check trigger 
            if (!this.Trigger.Invoke())
            {
                return;
            }

            this.Play();
        }

        /// <summary>
        /// Force the SoundEffectPlayer to play its sound effect. This method does not respect the Tigger property.
        /// </summary>
        public void Play()
        {
            if (this.Sounds.Count == 0)
            {
                return;
            }

            if (!this.AllowOverlappingPlays && Time.GameTime <= this.lockTime)
            {
                return;
            }

            var sound = this.Sounds[this.random.Next(0, this.Sounds.Count)];
            sound.Play(this.Volume, this.Pitch, this.Pan);
            this.lockTime = Time.GameTime + (float)sound.Duration.TotalSeconds;
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            this.LoadFromContentPath();
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPaths == null || !this.Owner.Scene.IsLoaded || !this.IsActiveInHierarchy)
            {
                return;
            }

            this.Sounds.Clear();

            foreach (var path in this.loadFromContentPaths)
            {
                var sound = GameEngine.Instance.Content.Load<SoundEffect>(path);
                this.Sounds.Add(sound);
            }

            this.loadFromContentPaths = null;
        }
    }
}
