using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Common;

namespace SandPerSand.SandSim
{
    public interface ISandGrid
    {
        public int ResolutionX { get; }

        public int ResolutionY { get; }

        public Vector2 Size { get; }

        public Vector2 Position { get; }

        public Vector2 CellSize { get; }
        
        public ref SandCell this[in int x, in int y] { get; }

        public ref SandCell this[in Int2 index] { get; }

        /// <summary>
        /// Marks every cell that is inside the polygon as obstacle, clearing any prior information inside that cell.
        /// </summary>
        public void AddPolygonObstacle(Vertices polygon);

        /// <summary>
        /// Marks every cell that is inside the circle as obstacle, clearing any prior information inside that cell.
        /// </summary>
        public void AddCircleObstacle(Vector2 center, float radius);

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
