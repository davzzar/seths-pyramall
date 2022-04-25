using System;

namespace Engine
{
    public readonly struct Border : IEquatable<Border>
    {
        public static readonly Border Zero = new Border(0, 0, 0, 0);

        public readonly float Right;

        public readonly float Top;

        public readonly float Left;

        public readonly float Bottom;

        public Border(float right, float top, float left, float bottom)
        {
            this.Right = right;
            this.Top = top;
            this.Left = left;
            this.Bottom = bottom;
        }

        public static bool operator ==(Border left, Border right)
        {
            return Math.Abs(left.Right - right.Right) < 1e-6f &&
                   Math.Abs(left.Top - right.Top) < 1e-6f &&
                   Math.Abs(left.Left - right.Left) < 1e-6f &&
                   Math.Abs(left.Bottom - right.Bottom) < 1e-6f;
        }

        public static bool operator !=(Border left, Border right)
        {
            return !(left == right);
        }

        public bool Equals(Border other)
        {
            return Math.Abs(this.Right  - other.Right) < 1e-3f &&
                   Math.Abs(this.Top    - other.Top) < 1e-3f &&
                   Math.Abs(this.Left   - other.Left) < 1e-3f &&
                   Math.Abs(this.Bottom - other.Bottom) < 1e-3f;
        }

        public override bool Equals(object obj)
        {
            return obj is Border other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Right, Top, Left, Bottom);
        }
    }
}
