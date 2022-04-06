using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    public readonly struct Ray
    {
        public readonly Vector2 Origin;

        public readonly Vector2 Direction;

        public Ray(Vector2 origin, Vector2 direction)
        {
            this.Origin = origin;
            this.Direction = direction / direction.Length();
        }
    }
}
