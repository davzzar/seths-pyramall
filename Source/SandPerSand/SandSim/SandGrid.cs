﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Common;

namespace SandPerSand.SandSim
{
    public sealed class SandGrid : ISandGrid
    {
        #region Private Fields
        
        private readonly SandCell[] data;

        private readonly Int2 maxCellIndex;

        private readonly int resX;

        private readonly int resY;

        private readonly Vector2 pos;

        private readonly Vector2 size;

        private readonly Vector2 cellSize;

        private readonly Vector2 halfCellSize;

        #endregion

        #region Public Properties and Indexers

        /// <inheritdoc />
        public int ResolutionX => this.resX;

        /// <inheritdoc />
        public int ResolutionY => this.resY;

        /// <inheritdoc />
        public Vector2 Size => this.size;

        /// <inheritdoc />
        public Vector2 Position => this.pos;

        /// <inheritdoc />
        public Vector2 CellSize => this.cellSize;
        
        public ref SandCell this[in int x, in int y] => ref this.GetInternal(in x, in y);

        public ref SandCell this[in Int2 index] => ref this.GetInternal(in index);

        #endregion

        #region Constructors

        public SandGrid(int resolutionX, int resolutionY, Vector2 position, Vector2 size)
        {
            if (resolutionX < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(resolutionX));
            }

            if (resolutionY < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(resolutionY));
            }

            if (float.IsInfinity(position.X) || float.IsNaN(position.X) || float.IsInfinity(position.Y) || float.IsNaN(position.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            if (size.X <= 0 || float.IsInfinity(size.X) || float.IsNaN(size.X))
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (size.Y <= 0 || float.IsInfinity(size.Y) || float.IsNaN(size.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.resX = resolutionX;
            this.resY = resolutionY;
            this.maxCellIndex = new Int2(this.resX - 1, this.resY - 1);
            this.pos = position;
            this.size = size;
            this.cellSize = new Vector2(size.X / resolutionX, size.Y / resolutionY);
            this.halfCellSize = this.cellSize / 2f;
            this.data = new SandCell[resolutionX * resolutionY];
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void AddPolygonObstacle(Vertices polygon)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException(nameof(polygon));
            }

            var aabb = polygon.GetAABB();

            var minIndex = this.PointToIndexClamped(in aabb.LowerBound);
            var maxIndex = this.PointToIndexClamped(in aabb.UpperBound);

            var testOffset = new[]
            {
                Vector2.Zero,
                Vector2.UnitX * this.CellSize * 0.25f,
                -Vector2.UnitX * this.CellSize * 0.25f,
                Vector2.UnitY * this.CellSize * 0.25f,
                -Vector2.UnitY * this.CellSize * 0.25f,
            };

            for (var y = minIndex.Y; y <= maxIndex.Y; y++)
            {
                for (var x = minIndex.X; x <= maxIndex.X; x++)
                {
                    var center = this.IndexToCenterPoint(x, y);

                    foreach (var offset in testOffset)
                    {
                        var testPoint = center + offset;
                        if (polygon.PointInPolygon(ref testPoint) > 0)
                        {
                            ref var cell = ref this.GetInternal(in x, in y);
                            cell.MarkObstacle();
                            break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public void AddCircleObstacle(Vector2 center, float radius)
        {
            if (float.IsInfinity(center.X) || float.IsNaN(center.X) || float.IsInfinity(center.Y) || float.IsNaN(center.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(center));
            }

            if (float.IsInfinity(radius) || float.IsNaN(radius))
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }
            
            var minIndex = this.PointToIndexClamped(new Vector2(center.X - radius, center.Y - radius));
            var maxIndex = this.PointToIndexClamped(new Vector2(center.X + radius, center.Y + radius));
            var radiusSquared = radius * radius;

            for (var y = minIndex.Y; y < maxIndex.Y; y++)
            {
                for (var x = minIndex.X; x < maxIndex.X; x++)
                {
                    var cellCenter = this.IndexToCenterPoint(x, y);
                    var sqDist = Vector2.DistanceSquared(cellCenter, center);

                    if (sqDist <= radiusSquared)
                    {
                        ref var cell = ref this.GetInternal(in x, in y);
                        cell.MarkObstacle();
                    }
                }
            }
        }

        public void CopyTo(SandGrid target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (target == this)
            {
                throw new ArgumentException("Can't copy a sand grid to itself.", nameof(target));
            }

            if (this.resX != target.resX || this.resY != target.resY)
            {
                throw new InvalidOperationException("The resolution of the target sand grid doesn't match.");
            }

            this.data.CopyTo(target.data, 0);
        }
        
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 IndexToCenterPoint(in int x, in int y)
        {
            return this.IndexToMinPoint(in x, in y) + this.halfCellSize;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 IndexToCenterPoint(in Int2 index) => this.IndexToCenterPoint(in index.X, in index.Y);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 IndexToMaxPoint(in int x, in int y)
        {
            return this.IndexToMinPoint(x + 1, y + 1);
        }
        
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 IndexToMaxPoint(in Int2 index) => this.IndexToMaxPoint(in index.X, in index.Y);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 IndexToMinPoint(in int x, in int y)
        {
            return this.pos + new Vector2(x * this.cellSize.X, y * this.cellSize.Y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 IndexToMinPoint(in Int2 index) => this.IndexToMinPoint(in index.X, in index.Y);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Int2 PointToIndex(in Vector2 point)
        {
            var relative = (point - this.pos) / this.size;
            var x = (int)(relative.X * this.resX);
            var y = (int)(relative.Y * this.resY);

            return new Int2(x, y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Int2 PointToIndexClamped(in Vector2 point)
        {
            var index = this.PointToIndex(in point);
            return Int2.Clamp(in index, in Int2.Zero, in this.maxCellIndex);
        }

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref SandCell GetInternal(int x, int y)
        {
            Debug.Assert(x >= 0);
            Debug.Assert(x < this.resX);
            Debug.Assert(y >= 0);
            Debug.Assert(y < this.resY);

            return ref this.data[x + y * this.resX];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref SandCell GetInternal(in int x, in int y)
        {
            Debug.Assert(x >= 0);
            Debug.Assert(x < this.resX);
            Debug.Assert(y >= 0);
            Debug.Assert(y < this.resY);

            return ref this.data[x + y * this.resX];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref SandCell GetInternal(in Int2 index)
        {
            Debug.Assert(index.X >= 0);
            Debug.Assert(index.X < this.resX);
            Debug.Assert(index.Y >= 0);
            Debug.Assert(index.Y < this.resY);

            return ref this.data[index.X + index.Y * this.resX];
        }
        
        #endregion
    }
}
