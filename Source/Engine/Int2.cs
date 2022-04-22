using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Represents an integer vector of dimension 2.
    /// </summary>
    public readonly struct Int2 : IEquatable<Int2>
    {
        /// <summary>
        /// An <see cref="Int2"/> with zero for both components.
        /// </summary>
        public static readonly Int2 Zero = new Int2(0, 0);

        /// <summary>
        /// The x component.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// The y component.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Creates a new instance of the <see cref="Int2"/> type.
        /// </summary>
        public Int2(int x, int y)
        {
            this.X = x;
            this.Y = y;
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

        /// <summary>
        /// Clamps the components of <see cref="value"/> between <see cref="min"/> and <see cref="max"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 Clamp(Int2 value, Int2 min, Int2 max)
        {
            return new Int2(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
        }

        /// <summary>
        /// Gets the component wise minimum of <see cref="a"/> and <see cref="b"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 Min(Int2 a, Int2 b)
        {
            return new Int2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        /// <summary>
        /// Gets the component wise maximum of <see cref="a"/> and <see cref="b"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 Max(Int2 a, Int2 b)
        {
            return new Int2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }
        
        /// <summary>
        /// Converts the given <see cref="Int2"/> to a <see cref="Vector2"/> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Int2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        /// <summary>
        /// Converts the given <see cref="Vector2"/> to a <see cref="Int2"/> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Int2(Vector2 value)
        {
            return new Int2((int)value.X, (int)value.Y);
        }

        /// <summary>
        /// Converts the given <see cref="Int2"/> to a <see cref="Tuple{Int32, Int32}"/> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator (int X, int Y)(Int2 value)
        {
            return (value.X, value.Y);
        }
        
        /// <summary>
        /// Converts the given <see cref="Tuple{Int32, Int32}"/> to a <see cref="Int2"/> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Int2((int X, int Y) value)
        {
            return new Int2(value.X, value.Y);
        }

        /// <summary>
        /// Converts the given <see cref="Int32"/> to a <see cref="Int2"/> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Int2(int value)
        {
            return new Int2(value, value);
        }

        /// <summary>
        /// Tests the two given <see cref="Int2"/> instances for equality.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Int2 left, Int2 right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        /// <summary>
        /// Tests the two given <see cref="Int2"/> instances for inequality.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Int2 left, Int2 right)
        {
            return left.X != right.X || left.Y != right.Y;
        }

        /// <summary>
        /// Component wise addition of two <see cref="Int2"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator +(Int2 left, Int2 right)
        {
            return new Int2(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        /// Component wise subtraction of two <see cref="Int2"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator -(Int2 left, Int2 right)
        {
            return new Int2(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Component wise multiplication of two <see cref="Int2"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator *(Int2 left, Int2 right)
        {
            return new Int2(left.X * right.X, left.Y * right.Y);
        }

        /// <summary>
        /// Component wise integer division of two <see cref="Int2"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int2 operator /(Int2 left, Int2 right)
        {
            return new Int2(left.X / right.X, left.Y / right.Y);
        }
    }
}