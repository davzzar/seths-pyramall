using System.Collections.Generic;
using Engine;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace SandPerSand.SandSim
{
    public interface IReadOnlySandGrid
    {
        /// <summary>
        /// Gets the number of cells along the X axis.
        /// </summary>
        public int ResolutionX { get; }

        /// <summary>
        /// Gets the number of cells along the Y axis.
        /// </summary>
        public int ResolutionY { get; }

        /// <summary>
        /// Gets the world size of the bounding rectangle
        /// </summary>
        public Vector2 Size { get; }

        /// <summary>
        /// Gets the lower left corner of the bounding rectangle
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// Gets the world size of a single cell
        /// </summary>
        public Vector2 CellSize { get; }
        
        /// <summary>
        /// Gets the cell at the given index.
        /// </summary>
        public SandCell this[in int x, in int y] { get; }

        /// <summary>
        /// Gets the cell at the given index.
        /// </summary>
        public SandCell this[in Int2 index] { get; }

        /// <summary>
        /// Evaluates whether the shape intersects with any sand.
        /// </summary>
        /// <param name="shape">The shape to cast, only sand intersecting with the shape will be considered.</param>
        /// <returns>Returns <c>true</c> if any sand intersects the shape, <c>false</c> otherwise.</returns>
        public bool ShapeCast<T>(in T shape) where T : IArea;

        /// <summary>
        /// Evaluates whether the shape intersects with any sand and collects information about the intersection.
        /// </summary>
        /// <param name="shape">The shape to cast, only sand intersecting with the shape will be considered.</param>
        /// <param name="result">Contains all collected information about the sand inside the shape.</param>
        /// <returns>Returns <c>true</c> if any sand intersects the shape, <c>false</c> otherwise.</returns>
        public bool ShapeCast<T>(in T shape, out SandCastResult result) where T : IArea;

        /// <summary>
        /// Evaluates whether the shape intersects with any sand and collects information about the intersection and compiles a list of all cell indices that intersect.
        /// </summary>
        /// <param name="shape">The shape to cast, only sand intersecting with the shape will be considered.</param>
        /// <param name="result">Contains all collected information about the sand inside the shape.</param>
        /// <param name="cellIndices">Array containing the indices of all cells that intersect the shape.</param>
        /// <returns>Returns <c>true</c> if any sand intersects the shape, <c>false</c> otherwise.</returns>
        public bool ShapeCast<T>(in T shape, out SandCastResult result, [NotNull]out Int2[] cellIndices) where T : IArea;

        /// <summary>
        /// Evaluates whether the shape intersects with any sand and collects information about the intersection and compiles a list of all cell indices that intersect.
        /// </summary>
        /// <param name="shape">The shape to cast, only sand intersecting with the shape will be considered.</param>
        /// <param name="result">Contains all collected information about the sand inside the shape.</param>
        /// <param name="cellIndices">List to which the indices of all cells that intersect the shape will be written.</param>
        /// <returns>Returns <c>true</c> if any sand intersects the shape, <c>false</c> otherwise.</returns>
        public bool ShapeCast<T>(in T shape, out SandCastResult result, [NotNull] IList<Int2> cellIndices) where T : IArea;

        /// <summary>
        /// Gets the absolute coordinates of the center of the cell with the given index.
        /// </summary>
        public Vector2 IndexToCenterPoint(in int x, in int y);

        /// <summary>
        /// Gets the absolute coordinates of the center of the cell with the given index.
        /// </summary>
        public Vector2 IndexToCenterPoint(in Int2 index);

        /// <summary>
        /// Gets the absolute coordinates of the upper right corner of the cell with the given index.
        /// </summary>
        public Vector2 IndexToMaxPoint(in int x, in int y);

        /// <summary>
        /// Gets the absolute coordinates of the upper right corner of the cell with the given index.
        /// </summary>
        public Vector2 IndexToMaxPoint(in Int2 index);

        /// <summary>
        /// Gets the absolute coordinates of the lower left corner of the cell with the given index.
        /// </summary>
        public Vector2 IndexToMinPoint(in int x, in int y);

        /// <summary>
        /// Gets the minimum coordinate inside the cell with the given index.
        /// </summary>
        public Vector2 IndexToMinPoint(in Int2 index);

        /// <summary>
        /// Gets the index of the cell containing the given point.
        /// </summary>
        /// <param name="point">The point in absolute coordinates.</param>
        /// <returns>Returns the index of the containing cell, this index can be out of bounds.</returns>
        public Int2 PointToIndex(in Vector2 point);

        /// <summary>
        /// Gets the index of the cell closest to the given point.
        /// </summary>
        /// <param name="point">The point in absolute coordinates.</param>
        /// <returns>Returns the index of the containing cell, or if the point is out of bounds, returns the index of the cell closest to the containing cell.</returns>
        public Int2 PointToIndexClamped(in Vector2 point);
    }
}
