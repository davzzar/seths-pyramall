using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace SandPerSand.SandSim
{
    public interface IReadOnlySandGrid
    {
        public int ResolutionX { get; }

        public int ResolutionY { get; }

        public Vector2 Size { get; }

        public Vector2 Position { get; }

        public Vector2 CellSize { get; }
        
        public SandCell this[in int x, in int y] { get; }

        public SandCell this[in Int2 index] { get; } 

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
