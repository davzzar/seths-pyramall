using System;
using System.Runtime.CompilerServices;

namespace SandPerSand.SandSim
{
    /// <summary>
    /// A simple convenience struct to pass around indices.
    /// </summary>
    public readonly struct Int2 : IEquatable<Int2>
    {
        public static readonly Int2 Zero = new Int2(0, 0);

        public readonly int X;

        public readonly int Y;

        public Int2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Int2(in int x, in int y)
        {
            this.X = x;
            this.Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Int2 Clamp(in Int2 value, in Int2 min, in Int2 max)
        {
            return new Int2(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
        }

        /// <inheritdoc />
        public bool Equals(Int2 other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Int2 other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.X, this.Y);
        }
    }
}