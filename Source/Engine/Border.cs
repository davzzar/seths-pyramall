using System;

namespace Engine
{
    /// <summary>
    /// Represents a border thickness or offset.
    /// </summary>
    public readonly struct Border : IEquatable<Border>
    {
        /// <summary>
        /// A zero border thickness.
        /// </summary>
        public static readonly Border Zero = new Border(0, 0, 0, 0);

        /// <summary>
        /// The right border thickness.
        /// </summary>
        public readonly float Right;

        /// <summary>
        /// The top border thickness.
        /// </summary>
        public readonly float Top;

        /// <summary>
        /// The left border thickness.
        /// </summary>
        public readonly float Left;

        /// <summary>
        /// The bottom border thickness.
        /// </summary>
        public readonly float Bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Border"/> type.
        /// </summary>
        public Border(float right, float top, float left, float bottom)
        {
            this.Right = right;
            this.Top = top;
            this.Left = left;
            this.Bottom = bottom;
        }

        /// <summary>
        /// Compares two borders for equality, returns true if all 4 directions are approximately the same (max difference: 1e-6). 
        /// </summary>
        public static bool operator ==(Border left, Border right)
        {
            return Math.Abs(left.Right - right.Right) < 1e-6f &&
                   Math.Abs(left.Top - right.Top) < 1e-6f &&
                   Math.Abs(left.Left - right.Left) < 1e-6f &&
                   Math.Abs(left.Bottom - right.Bottom) < 1e-6f;
        }

        /// <summary>
        /// Compares two borders for inequality, returns true if any of the 4 directions is not approximately the same (min difference: 1e-6)
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Border left, Border right)
        {
            return !(left == right);
        }


        /// <inheritdoc />
        public bool Equals(Border other)
        {
            return Math.Abs(this.Right  - other.Right) < 1e-3f &&
                   Math.Abs(this.Top    - other.Top) < 1e-3f &&
                   Math.Abs(this.Left   - other.Left) < 1e-3f &&
                   Math.Abs(this.Bottom - other.Bottom) < 1e-3f;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Border other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Right, this.Top, this.Left, this.Bottom);
        }
    }
}
