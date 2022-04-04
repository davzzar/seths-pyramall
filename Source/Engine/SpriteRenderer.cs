using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class SpriteRenderer : Renderer
    {
        private Texture2D texture;
        private string loadFromContentPath;

        public Texture2D Texture
        {
            get => this.texture;
            set
            {
                this.texture = value;
                this.loadFromContentPath = null;
            }
        }

        public Color Color { get; set; }
        
        public Rectangle? SourceRect { get; set; }

        public SpriteRenderer()
        {
            this.Color = Color.White;
            this.SourceRect = null;
        }
        
        /// <inheritdoc />
        public override void Draw()
        {
            if (this.texture == null)
            {
                return;
            }

            var matrix = this.Transform.LocalToWorld * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));

            if (this.SourceRect.HasValue)
            {
                Graphics.Draw(this.texture, this.Color, this.SourceRect.Value, ref matrix, this.Depth);
            }
            else
            {
                Graphics.Draw(this.texture, this.Color, ref matrix, this.Depth);
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            this.LoadFromContentPath();
        }

        public void LoadFromContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.loadFromContentPath = path;
            this.LoadFromContentPath();
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPath != null && SceneManager.IsReady && this.IsActiveInHierarchy)
            {
                this.texture = GameEngine.Instance.Content.Load<Texture2D>(this.loadFromContentPath);
                this.loadFromContentPath = null;
            }
        }
    }
}
