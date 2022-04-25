using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Interface for 2D area shapes that support geometric containment and intersection capabilities.
    /// </summary>
    public interface IArea
    {
        /// <summary>
        /// Gets the axis aligned bounding rectangle of this shape.
        /// </summary>
        public Aabb Bounds { get; }

        /// <summary>
        /// Evaluates whether the given point is inside this shape.
        /// </summary>
        public bool ContainsPoint(in Vector2 point);

        /// <summary>
        /// Evaluates whether the given rectangle is fully contained inside this shape.
        /// </summary>
        public bool ContainsRect(in Vector2 min, in Vector2 max);

        /// <summary>
        /// Evaluates whether the given rectangle intersects this shape.
        /// </summary>
        public bool IntersectsRect(in Vector2 min, in Vector2 max);
    }
}
