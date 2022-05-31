using System;
using System.Runtime.CompilerServices;

namespace SandPerSand.SandSim
{
    /// <summary>
    /// A simple convenience struct to pass around indices.
    /// </summary>
    public readonly struct Int2_old : IEquatable<Int2_old>
    {
        public static readonly Int2_old Zero = new Int2_old(0, 0);

        public readonly int X;

        public readonly int Y;

        public Int2_old(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Int2_old(in int x, in int y)
        {
            this.X = x;
            this.Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Int2_old Clamp(in Int2_old value, in Int2_old min, in Int2_old max)
        {
            return new Int2_old(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
        }

        /// <inheritdoc />
        public bool Equals(Int2_old other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Int2_old other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.X, this.Y);
        }
    }
}