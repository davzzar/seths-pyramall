using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.TextureAtlases;

namespace Engine
{
    public sealed class ParticleSystem : Renderer
    {
        private static readonly FieldInfo bufferField = typeof(ParticleEmitter).GetField("Buffer",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private Vector2 prevCamPos;

        [CanBeNull]
        private ParticleEffect effect;

        public ParticleEffect Effect => this.effect ?? throw new InvalidOperationException("Can't access the particle system after it has been destroyed.");
        
        public ParticleSystem()
        {
            this.effect = new ParticleEffect();
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.effect?.Dispose();
        }
        
        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();

            if (this.effect != null)
            {
                this.effect.Position = this.Transform.Position;
                this.effect.Rotation = this.Transform.Rotation;
                this.effect.Scale = this.Transform.Scale;

                this.effect.Update(Time.DeltaTime);
            }
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (this.effect != null)
            {
                ref var localToWorld = ref this.Transform.LocalToWorld;
                ref var worldToView = ref Graphics.CurrentWorldToView;

                var matrix = worldToView * localToWorld;
                matrix.DecomposeTRS(out var position, out var rotation, out var scale);
                
                foreach (var emitter in this.effect.Emitters)
                {
                    UnsafeDraw(emitter);
                }


                //Graphics.SpriteBatch.Draw(this.effect);
            }
        }

        private static unsafe void UnsafeDraw(ParticleEmitter emitter)
        {
            if (emitter.TextureRegion == null)
            {
                return;
            }

            var textureRegion = emitter.TextureRegion;
            var origin = new Vector2((float) textureRegion.Width / 2f, (float) textureRegion.Height / 2f);
            var buffer = (ParticleBuffer)bufferField.GetValue(emitter);
            var iterator = buffer.Iterator;
            
            while (iterator.HasNext)
            {
                var particlePtr = iterator.Next();
                var rgb = particlePtr->Color.ToRgb();
                
                if (Graphics.SpriteBatch.GraphicsDevice.BlendState == BlendState.AlphaBlend)
                {
                    rgb *= particlePtr->Opacity;
                }
                else
                {
                    rgb.A = (byte) ((double) particlePtr->Opacity * (double) byte.MaxValue);
                }

                var position = new Vector2(particlePtr->Position.X, particlePtr->Position.Y);
                var scale = particlePtr->Scale;
                var color = new Color(rgb, particlePtr->Opacity);
                var rotation = particlePtr->Rotation;
                var layerDepth = particlePtr->LayerDepth;

                var matrix = Matrix3x3.CreateTRS(position, rotation, scale);
                
                Graphics.Draw(textureRegion.Texture, color, ref matrix, layerDepth, SpriteEffects.None);
            }
        }
    }
}
