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
            var psGo = new GameObject("JumpThroughSand PS with player");
            psGo.Transform.Position = position;
            var psComp = psGo.AddComponent<ParticleSystem>();

            var particleTexture = new Texture2D(GameEngine.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            var textureRegion = new TextureRegion2D(particleTexture);           
            psComp.Effect.Emitters.Add(new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(0.5f),
                Profile.BoxUniform(0.1f, 1.5f))
            {
                Parameters = new ParticleReleaseParameters
                {
                    Speed = new Range<float>(0.05f, 0.1f),
                    Quantity = 2,
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
                                StartValue = new HslColor(38f, 0.9f, 0.7f),
                                EndValue = new HslColor(46f, 1f, 0.6f)
                            },
                            new OpacityInterpolator { StartValue = 1f, EndValue = 0f }
                        }
                    },
                    new DragModifier
                    {
                        Density = 1f, DragCoefficient = 3f
                    },
                    new RotationModifier {RotationRate = -2.1f},
                    new LinearGravityModifier {Direction = (Vector2)GravityDirection, Strength = 30f},
                }
            });
            psGo.AddComponent<GoTimer>().Init(0.2f, () =>
            {
                psGo.Destroy();
            });
            return psGo;
        }

        public static GameObject MakeJumpThroughSandEffectInPlace(Vector2 position, Vector2? GravityDirection = null)
        {
            if (GravityDirection == null)
            {
                GravityDirection = new Vector2(-1f, -1f);
            }
            var psGo = new GameObject("JumpThroughSand PS in place");
            psGo.Transform.Position = position;
            var psComp = psGo.AddComponent<ParticleSystem>();
            var particleTexture = new Texture2D(GameEngine.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            var textureRegion = new TextureRegion2D(particleTexture);
            psComp.Effect.Emitters.Add(new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(0.4f),
                Profile.Spray((Vector2)GravityDirection, 2f))
            {
                Parameters = new ParticleReleaseParameters
                {
                    Speed = new Range<float>(10f, 15f),
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
                                StartValue = new HslColor(38f, 0.9f, 0.7f),
                                EndValue = new HslColor(46f, 1f, 0.6f)
                            },
                            new OpacityInterpolator { StartValue = -0.2f, EndValue = 1f }
                        }
                    },
                    new DragModifier
                    {
                        Density = 2f, DragCoefficient = 5f
                    },
                    new RotationModifier {RotationRate = -2.1f},
                    new RectangleContainerModifier {Width = 800, Height = 480},
                    new LinearGravityModifier {Direction = new Vector2(0,-1), Strength = 30f},
                }
            });
            psGo.AddComponent<GoTimer>().Init(0.6f, () =>
            {
                psGo.Destroy();
            });
            return psGo;
        }
    }
}
