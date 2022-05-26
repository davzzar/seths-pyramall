using System;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using Microsoft.Xna.Framework.Graphics;

namespace SandPerSand
{
    public static partial class Template
    {

        public static GameObject MakeJumpThroughSandEffect(Vector2 position,Vector2? GravityDirection = null)
        {
            if (GravityDirection == null)
            {
                GravityDirection = new Vector2(-1f, -1f);
            }
            var psGo = new GameObject("JumpThroughSand Particle System");
            psGo.Transform.Position = position;
            var psComp = psGo.AddComponent<ParticleSystem>();

            var particleTexture = new Texture2D(GameEngine.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            var textureRegion = new TextureRegion2D(particleTexture);           
            psComp.Effect.Emitters.Add(new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(2.5),
                Profile.BoxUniform(1, 1))
            {
                Parameters = new ParticleReleaseParameters
                {
                    Speed = new Range<float>(0f, 0.3f),
                    Quantity = 3,
                    Rotation = new Range<float>(-1f, 1f),
                    Scale = new Range<float>(0.1f, 0.1f)
                },
                Modifiers =
                {
                    new AgeModifier
                    {
                        Interpolators =
                        {
                            new ColorInterpolator
                            {
                                StartValue = new HslColor(0.54f, 1f, 0.76f),
                                EndValue = new HslColor(0.54f, 1f, 0.76f)
                            }
                        }
                    },
                    new RotationModifier {RotationRate = -2.1f},
                    new RectangleContainerModifier {Width = 800, Height = 480},
                    new LinearGravityModifier {Direction = (Vector2)GravityDirection, Strength = 30f},
                }
            });
            psGo.AddComponent<GoTimer>().Init(0.5f, () =>
            {
                psGo.Destroy();
            });
            return psGo;
        }
    }
}
