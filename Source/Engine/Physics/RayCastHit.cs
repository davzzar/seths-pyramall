using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine;

public readonly struct RayCastHit
{
    public readonly Fixture Fixture;

    public readonly Vector2 Point;

    public readonly Vector2 Normal;

    public readonly float Fraction;

    public RayCastHit(Fixture fixture, in Vector2 point, in Vector2 normal, float fraction)
    {
        this.Fixture = fixture;
        this.Point = point;
        this.Normal = normal;
        this.Fraction = fraction;
    }
}