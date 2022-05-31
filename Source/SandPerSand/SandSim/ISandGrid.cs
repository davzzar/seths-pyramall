using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Common;

namespace SandPerSand.SandSim
{
    public interface ISandGrid : IReadOnlySandGrid
    {
        public new ref SandCell this[in int x, in int y] { get; }

        public new ref SandCell this[in Int2 index] { get; }

        /// <summary>
        /// Marks every cell that is inside the polygon as obstacle, clearing any prior information inside that cell.
        /// </summary>
        public void AddPolygonObstacle(Vertices polygon);

        /// <summary>
        /// Marks every cell that is inside the circle as obstacle, clearing any prior information inside that cell.
        /// </summary>
        public void AddCircleObstacle(Vector2 center, float radius);
    }
}
