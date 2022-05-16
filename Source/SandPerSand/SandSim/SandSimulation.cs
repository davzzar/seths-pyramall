﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Engine;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand.SandSim
{
    public class SandSimulation : Behaviour
    {
        private SandGrid sandFrontBuffer;
        private SandGrid sandBackBuffer;
        private List<Int2> updateFrontBuffer;
        private List<Int2> updateBackBuffer;
        private List<Int2> newSandBuffer;
        private HashSet<Int2> updateLookup;
        private HashSet<Int2> newSandLookup;
        private SandRenderer sandRenderer;

        private bool dirty;
        private int resolutionX;
        private int resolutionY;
        private Vector2 min;
        private Vector2 size;
        private LayerMask colliderLayerMask;
        private float simulationStepTime;

        private List<Aabb> sandSources;
        private List<Aabb> sandDrains;
        private float currentSimulationTime;

        private readonly SandGridReader sandGridReader;

        private float raisingSandSpeed = 0.6f;
        private float raisingSandDelay = 4f;
        private float raisingSandCurrentTime = 0f;
        private int raisingSandCurrentRow = -1;

        public int ResolutionX
        {
            get => this.resolutionX;
            set
            {
                if (this.resolutionX == value)
                {
                    return;
                }

                this.resolutionX = value;
                this.dirty = true;
            }
        }

        public int ResolutionY
        {
            get => this.resolutionY;
            set
            {
                if (this.resolutionY == value)
                {
                    return;
                }

                this.resolutionY = value;
                this.dirty = true;
            }
        }

        public Vector2 Min
        {
            get => this.min;
            set
            {
                this.min = value;
                this.dirty = true;
            }
        }

        public Vector2 Size
        {
            get => this.size;
            set
            {
                this.size = value;
                this.dirty = true;
            }
        }

        public LayerMask ColliderLayerMask
        {
            get => this.colliderLayerMask;
            set
            {
                if (this.colliderLayerMask == value)
                {
                    return;
                }

                this.colliderLayerMask = value;

                this.dirty = true;
            }
        }

        public float SimulationStepTime
        {
            get => this.simulationStepTime;
            set => this.simulationStepTime = MathF.Max(value, 1e-5f);
        }

        /// <summary>
        /// Gets a value indicating if the RaisingSand is raising
        /// </summary>
        public bool IsSandRising =>
            this.sandBackBuffer.Position.Y + this.sandBackBuffer.Size.Y - this.RaisingSandUpperMargin > this.RaisingSandHeight &&
            !this.PauseRaisingSand && this.RaisingSandHeight > 0.0f;


        /// <summary>
        /// Gets or sets the speed at which the raising sand raises in world units per second.
        /// </summary>
        public float RaisingSandSpeed
        {
            get => this.raisingSandSpeed;
            set => this.raisingSandSpeed = MathF.Max(value, 0.0f);
        }

        /// <summary>
        /// Gets or sets the length of the initial delay after which the raising sand starts raising.
        /// </summary>
        public float RaisingSandDelay
        {
            get => this.raisingSandDelay;
            set => this.raisingSandDelay = MathF.Max(value, 0f);
        }

        /// <summary>
        /// Gets or sets the distance from the top of the sand simulation at which the raising sand stops in world units.
        /// </summary>
        public float RaisingSandUpperMargin { get; set; } = 9.4f;

        /// <summary>
        /// Gets or sets a value indicating whether the raising sand simulation should be paused.
        /// </summary>
        public bool PauseRaisingSand { get; set; }

        /// <summary>
        /// Gets the height of the raising sand as y-coordinate in world space.
        /// </summary>
        public float RaisingSandHeight { get; private set; }

        [CanBeNull]
        public SandRenderer Renderer => this.sandRenderer;

        public int MaxSimulationSteps { get; set; }

        public int MaxLayer { get; set; } = 2;

        /// <summary>
        /// Gets the read only access to the sand data.
        /// </summary>
        public IReadOnlySandGrid SandData => this.sandGridReader;

        public SandSimulation()
        {
            this.resolutionX = 128;
            this.resolutionY = 128;
            this.min = Vector2.Zero;
            this.size = Vector2.One;
            this.colliderLayerMask = LayerMask.All;
            this.simulationStepTime = 1 / 20f;
            this.MaxSimulationSteps = 100;

            this.sandSources = new List<Aabb>();
            this.sandDrains = new List<Aabb>();
            this.dirty = true;

            this.updateFrontBuffer = new List<Int2>();
            this.updateBackBuffer = new List<Int2>();
            this.updateLookup = new HashSet<Int2>();
            this.newSandBuffer = new List<Int2>();
            this.newSandLookup = new HashSet<Int2>();

            this.sandGridReader = new SandGridReader();
        }

        public void AddSandSource(in Aabb rect)
        {
            this.sandSources.Add(rect);
            this.AddSandSourceToSimData(in rect);
        }

        /// <summary>
        /// Removes all sand source rectangles that intersect the given rect.
        /// </summary>
        public void RemoveSandSource(in Aabb rect)
        {
            for (var i = this.sandSources.Count - 1; i >= 0; i--)
            {
                var source = this.sandSources[i];

                if (rect.IntersectsRect(source.Min, source.Max))
                {
                    this.sandSources.RemoveAt(i);
                    this.RemoveSandSourceFromSimData(in source);
                }
            }

            this.RemoveSandSourceFromSimData(in rect);
        }
        
        public void ClearSandSources()
        {
            if (this.sandSources.Count == 0)
            {
                return;
            }

            if (this.sandFrontBuffer != null)
            {
                foreach (var rect in this.sandSources)
                {
                    var min = this.sandFrontBuffer.PointToIndexClamped(rect.Min);
                    var max = this.sandFrontBuffer.PointToIndexClamped(rect.Max);

                    for (var y = min.Y; y <= max.Y; y++)
                    {
                        for (var x = min.X; x <= max.X; x++)
                        {
                            if (this.sandFrontBuffer[x, y].IsSandSource)
                            {
                                this.sandFrontBuffer[x, y].MarkSand();
                                this.sandBackBuffer[x, y].MarkSand();
                                this.MarkForUpdate(x, y);
                                this.MarkNeighborsForUpdate(x, y);
                            }
                        }
                    }
                }
            }

            this.sandSources.Clear();
        }
        
        /// <inheritdoc />
        protected override void OnEnable()
        {
            this.currentSimulationTime = Time.GameTime;
            this.sandRenderer = this.Owner.GetOrAddComponent<SandRenderer>();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            if (this.dirty)
            {
                this.dirty = false;
                this.InvalidateSandGrid();
            }

            var deltaT = Time.GameTime - this.currentSimulationTime;
            var numSteps = Math.Min((int)(deltaT / this.SimulationStepTime), this.MaxSimulationSteps);
            this.currentSimulationTime += this.SimulationStepTime * numSteps;

            //if (Keyboard.GetState().IsKeyDown(Keys.P))
            //{
            //    this.ClearSandSources();
            //}

            if (GameStateManager.Instance.CurrentState != GameState.InRound && GameStateManager.Instance.CurrentState != GameState.CountDown)
            {
                return;
            }

            for (var i = 0; i < numSteps; i++)
            {
                this.currentSimulationTime += this.SimulationStepTime;
                deltaT -= this.SimulationStepTime;
                this.DoSimulationStep();
            }

            this.HandleRaisingSand();

            var min = this.sandBackBuffer.Position;
            var max = new Vector2(this.sandBackBuffer.Position.X + this.sandBackBuffer.Size.X, this.RaisingSandHeight);
            Gizmos.DrawRect((min + max) / 2f, max - min, Color.BurlyWood, 2f);

            this.sandRenderer.MaxLayer = this.MaxLayer;
        }

        private void InvalidateSandGrid()
        {
            this.sandFrontBuffer = new SandGrid(this.ResolutionX, this.ResolutionY, this.Min, this.Size);
            this.sandBackBuffer = new SandGrid(this.ResolutionX, this.resolutionY, this.Min, this.Size);
            this.sandGridReader.SandGrid = this.sandFrontBuffer;
            
            this.updateFrontBuffer.Clear();
            this.updateBackBuffer.Clear();
            this.updateLookup.Clear();

            this.sandRenderer.SandGrid = this.sandFrontBuffer;

            var colliders = GameObject.FindComponents<Collider>();

            foreach (var collider in colliders)
            {
                if (!this.colliderLayerMask.HasLayer(collider.Owner.Layer))
                {
                    continue;
                }

                if (collider.IsTrigger)
                {
                    continue;
                }

                if (collider is PolygonCollider pc)
                {
                    this.sandFrontBuffer.AddPolygonObstacle(pc.Polygon);
                }
                else if (collider is CircleCollider cc)
                {
                    this.sandFrontBuffer.AddCircleObstacle(cc.Transform.Position, cc.Radius);
                }
            }

            for (var y = 0; y < this.sandFrontBuffer.ResolutionY; y++)
            {
                for (var x = 0; x < this.sandFrontBuffer.ResolutionX; x++)
                {
                    var cell = this.sandFrontBuffer[x, y];

                    if (cell.IsSandSource)
                    {
                        this.MarkNeighborsForUpdate(x, y);
                    }
                }
            }

            foreach (var rect in this.sandSources)
            {
                this.AddSandSourceToSimData(rect);
            }

            foreach (var rect in this.sandDrains)
            {
                this.MarkSandDrain(rect);
            }

            this.sandFrontBuffer.CopyTo(this.sandBackBuffer);
        }

        private void AddSandSourceToSimData(in Aabb rect)
        {
            if (this.sandFrontBuffer == null)
            {
                return;
            }

            var min = this.sandFrontBuffer.PointToIndexClamped(rect.Min);
            var max = this.sandFrontBuffer.PointToIndexClamped(rect.Max);

            for (var y = min.Y; y <= max.Y; y++)
            {
                for (var x = min.X; x <= max.X; x++)
                {
                    this.sandFrontBuffer[x, y].MarkSandSource();
                    this.sandBackBuffer[x, y].MarkSandSource();
                    this.MarkNeighborsForUpdate(x, y);
                }
            }
        }

        private void RemoveSandSourceFromSimData(in Aabb rect)
        {
            if (this.sandFrontBuffer == null)
            {
                return;
            }

            var min = this.sandFrontBuffer.PointToIndexClamped(rect.Min);
            var max = this.sandFrontBuffer.PointToIndexClamped(rect.Max);

            for (var y = min.Y; y <= max.Y; y++)
            {
                for (var x = min.X; x <= max.X; x++)
                {
                    if (this.sandBackBuffer[x, y].IsSandSource)
                    {
                        this.sandFrontBuffer[x, y].MarkEmpty();
                        this.sandBackBuffer[x, y].MarkEmpty();
                        this.MarkForUpdate(x, y);
                        this.MarkNeighborsForUpdate(x, y);
                    }
                }
            }
        }

        private void MarkSandDrain(in Aabb rect)
        {
            if (this.sandFrontBuffer == null)
            {
                return;
            }

            var min = this.sandFrontBuffer.PointToIndexClamped(rect.Min);
            var max = this.sandFrontBuffer.PointToIndexClamped(rect.Max);

            for (var y = min.Y; y <= max.Y; y++)
            {
                for (var x = min.X; x <= max.X; x++)
                {
                    this.sandFrontBuffer[x, y].MarkDrain();
                    this.sandBackBuffer[x, y].MarkDrain();
                    this.MarkNeighborsForUpdate(x, y);
                }
            }
        }

        private void DoSimulationStep()
        {
            // Swap the update buffers and prepare them for the next update
            (this.updateFrontBuffer, this.updateBackBuffer) = (this.updateBackBuffer, this.updateFrontBuffer);
            this.updateBackBuffer.Clear();
            this.updateLookup.Clear();
            this.newSandBuffer.Clear();
            this.newSandLookup.Clear();

            // First simulate sand movement
            foreach (var index in this.updateFrontBuffer)
            {
                var cell = this.sandFrontBuffer[in index];

                if (cell.data == SandCell.NoCell.data)
                {
                    continue;
                }

                //if (cell.IsEmpty)
                if (cell.IsEmpty)
                {
                    var newLayer = this.GetSandLayer(cell, index.X, index.Y);

                    if (newLayer >= 0)
                    {
                        cell.MarkSand();
                        cell.Layer = (byte)newLayer;
                        this.sandBackBuffer[in index] = cell;
                        this.MarkNeighborsForUpdateUnsafe(in index);
                        this.MarkForStabilityCheck(index.X, index.Y);
                    }
                }
                else if (cell.HasSand && !cell.IsSandStable && !cell.IsSandSource)
                {
                    var newLayer = this.GetSandLayer(cell, index.X, index.Y);

                    if (newLayer != cell.Layer)
                    {
                        if (newLayer < 0)
                        {
                            cell.MarkEmpty();
                        }
                        else
                        {
                            cell.Layer = (byte)newLayer;
                        }

                        this.sandBackBuffer[in index] = cell;
                        this.MarkNeighborsForUpdateUnsafe(in index);
                        this.MarkForStabilityCheck(index.X, index.Y);
                    }
                }
            }
            
            for (var i = 0; i < this.newSandBuffer.Count; i++)
            {
                var index = this.newSandBuffer[i];
                this.newSandLookup.Remove(index);
                this.TestForStability(in index);
            }

            this.sandBackBuffer.CopyTo(this.sandFrontBuffer);

            // Swap back and front buffer
            (this.sandFrontBuffer, this.sandBackBuffer) = (this.sandBackBuffer, this.sandFrontBuffer);
            this.sandRenderer.SandGrid = this.sandFrontBuffer;
            this.sandGridReader.SandGrid = this.sandFrontBuffer;
        }

        private int GetSandLayer(SandCell cell, int x, int y)
        {
            var cellTL = this.sandFrontBuffer[x - 1, y + 1];
            var cellTC = this.sandFrontBuffer[x + 0, y + 1];
            var cellTR = this.sandFrontBuffer[x + 1, y + 1];

            if (cellTC.HasSand)
            {
                // Sand can flow straight down into empty slots
                return cellTC.Layer;
            }

            if (cellTL.HasSand || cellTR.HasSand)
            {
                var cellL = this.sandFrontBuffer[x - 1, y];
                var cellR = this.sandFrontBuffer[x + 1, y];
                var cellBC = this.sandFrontBuffer[x + 0, y - 1];

                // Sand can flow diagonally into empty slots under which there is an obstacle or stable sand
                if (cellBC.IsSolidUnderground)
                {
                    return 0;
                }

                if (cellBC.HasSand && cellBC.Layer < this.MaxLayer - 1)
                {
                    return cellBC.Layer + 1;
                }

                if (cellTL.HasSand && cellL.IsSolidUnderground || cellTR.HasSand && cellR.IsSolidUnderground)
                {
                    return 0;
                }

                var leftLayer = cellL.HasSand ? cellL.Layer : byte.MaxValue;
                var rightLayer = cellR.HasSand ? cellR.Layer : byte.MaxValue;
                var cellLayer = Math.Min(leftLayer, rightLayer) + 1;

                if (cellLayer < this.MaxLayer)
                {
                    return cellLayer;
                }
            }

            return -1;
        }

        private void TestForStability(in Int2 index)
        {
            ref var cell = ref this.sandBackBuffer[in index];

            if (!cell.HasSand || cell.IsSandStable)
            {
                return;
            }
            
            var cellBL = this.sandBackBuffer[index.X - 1, index.Y - 1];
            var cellBC = this.sandBackBuffer[index.X + 0, index.Y - 1];
            var cellBR = this.sandBackBuffer[index.X + 1, index.Y - 1];

            if (!cellBC.HasObstacle &&
                (!cellBC.HasSand || !cellBC.IsSandStable || !cellBL.IsSolidUnderground || !cellBR.IsSolidUnderground))
            {
                return;
            }

            cell.IsSandStable = true;
            this.MarkNeighborsForUpdateUnsafe(new Int2(index.X, index.Y));
            this.MarkForStabilityCheck(index.X - 1, index.Y + 1);
            this.MarkForStabilityCheck(index.X + 0, index.Y + 1);
            this.MarkForStabilityCheck(index.X + 1, index.Y + 1);
        }

        private void MarkNeighborsForUpdate(int x, int y)
        {
            var maxX = this.sandFrontBuffer.ResolutionX - 1;
            var maxY = this.sandFrontBuffer.ResolutionY;

            if (y > 0)
            {
                if (x > 0)
                {
                    this.MarkForUpdate(x - 1, y - 1);
                }
                
                this.MarkForUpdate(x, y - 1);

                if (x < maxX)
                {
                    this.MarkForUpdate(x + 1, y - 1);
                }
            }

            if (x > 0)
            {
                this.MarkForUpdate(x - 1, y);
            }

            if (x < maxX)
            {
                this.MarkForUpdate(x + 1, y);
            }

            if (y < maxY)
            {
                if (x > 0)
                {
                    this.MarkForUpdate(x - 1, y + 1);
                }

                this.MarkForUpdate(x, y + 1);

                if (x < maxX)
                {
                    this.MarkForUpdate(x + 1, y + 1);
                }
            }
        }

        private void MarkNeighborsForUpdateUnsafe(in Int2 index)
        {
            var x = index.X;
            var y = index.Y;

            this.MarkForUpdate(x - 1, y - 1);
            this.MarkForUpdate(x, y - 1);
            this.MarkForUpdate(x + 1, y - 1);
            this.MarkForUpdate(x - 1, y);
            this.MarkForUpdate(x + 1, y);
            this.MarkForUpdate(x - 1, y + 1);
            this.MarkForUpdate(x, y + 1);
            this.MarkForUpdate(x + 1, y + 1);
        }

        private void MarkForUpdate(int x, int y)
        {
            var index = new Int2(x, y);

            if (this.updateLookup.Contains(index))
            {
                return;
            }

            this.updateLookup.Add(index);
            this.updateBackBuffer.Add(index);
        }

        private void MarkForStabilityCheck(int x, int y)
        {
            var index = new Int2(x, y);

            if (this.newSandLookup.Contains(index))
            {
                return;
            }

            this.newSandBuffer.Add(index);
            this.newSandLookup.Add(index);
        }

        private void HandleRaisingSand()
        {
            if (this.PauseRaisingSand)
            {
                return;
            }

            var deltaT = Time.DeltaTime;

            if (this.raisingSandCurrentTime < this.raisingSandDelay)
            {
                deltaT -= this.raisingSandDelay - this.raisingSandCurrentTime;
                this.raisingSandCurrentTime += Time.DeltaTime;
            }

            if (deltaT <= 0)
            {
                return;
            }

            var deltaH = this.RaisingSandSpeed * deltaT;

            this.RaisingSandHeight = MathF.Min(this.RaisingSandHeight + deltaH, this.sandBackBuffer.Position.Y + this.sandBackBuffer.Size.Y - this.RaisingSandUpperMargin);
            
            var newRow = this.sandBackBuffer.PointToIndexClamped(new Vector2(0f, this.RaisingSandHeight - this.sandBackBuffer.CellSize.Y / 2f)).Y;

            while (newRow > this.raisingSandCurrentRow)
            {
                this.raisingSandCurrentRow++;

                for (var x = 0; x < this.sandBackBuffer.ResolutionX; x++)
                {
                    ref var cell = ref this.sandBackBuffer[x, this.raisingSandCurrentRow];

                    cell.MarkSand();
                    cell.IsSandStable = true;
                    this.MarkForUpdate(x, this.raisingSandCurrentRow);
                }
            }

            this.sandBackBuffer.CopyTo(this.sandFrontBuffer);
        }

        /// <summary>
        /// Proxy class for the sand data to provide a read only interface while allowing only the sand simulation to modify internal data.
        /// </summary>
        private sealed class SandGridReader : IReadOnlySandGrid
        {
            /// <summary>
            /// The sand grid data to use, this can only be modified by <see cref="SandSimulation"/> duo to the class protection level.
            /// </summary>
            public SandGrid SandGrid { get; set; }

            /// <inheritdoc />
            public int ResolutionX => this.SandGrid.ResolutionX;

            /// <inheritdoc />
            public int ResolutionY => this.SandGrid.ResolutionY;

            /// <inheritdoc />
            public Vector2 Size => this.SandGrid.Size;

            /// <inheritdoc />
            public Vector2 Position => this.SandGrid.Position;

            /// <inheritdoc />
            public Vector2 CellSize => this.SandGrid.CellSize;

            /// <inheritdoc />
            public SandCell this[in int x, in int y] => this.SandGrid[in x, in y];

            public SandCell this[in Int2 index] => this.SandGrid[in index];

            /// <inheritdoc />
            public bool ShapeCast<T>(in T shape) where T : IArea
            {
                return this.SandGrid.ShapeCast(shape);
            }

            /// <inheritdoc />
            public bool ShapeCast<T>(in T shape, out SandCastResult result) where T : IArea
            {
                return this.SandGrid.ShapeCast(shape, out result);
            }

            /// <inheritdoc />
            public bool ShapeCast<T>(in T shape, out SandCastResult result, out Int2[] cellIndices) where T : IArea
            {
                return this.SandGrid.ShapeCast(shape, out result, out cellIndices);
            }

            /// <inheritdoc />
            public bool ShapeCast<T>(in T shape, out SandCastResult result, IList<Int2> cellIndices) where T : IArea
            {
                return this.SandGrid.ShapeCast(shape, out result, cellIndices);
            }

            /// <inheritdoc />
            public Vector2 IndexToCenterPoint(in int x, in int y)
            {
                return this.SandGrid.IndexToCenterPoint(in x, in y);
            }

            /// <inheritdoc />
            public Vector2 IndexToCenterPoint(in Int2 index)
            {
                return this.SandGrid.IndexToCenterPoint(in index);
            }

            /// <inheritdoc />
            public Vector2 IndexToMaxPoint(in int x, in int y)
            {
                return this.SandGrid.IndexToMaxPoint(in x, in y);
            }

            /// <inheritdoc />
            public Vector2 IndexToMaxPoint(in Int2 index)
            {
                return this.SandGrid.IndexToMaxPoint(in index);
            }

            /// <inheritdoc />
            public Vector2 IndexToMinPoint(in int x, in int y)
            {
                return this.SandGrid.IndexToMinPoint(in x, in y);
            }

            /// <inheritdoc />
            public Vector2 IndexToMinPoint(in Int2 index)
            {
                return this.SandGrid.IndexToMinPoint(in index);
            }

            /// <inheritdoc />
            public Int2 PointToIndex(in Vector2 point)
            {
                return this.SandGrid.PointToIndex(in point);
            }

            /// <inheritdoc />
            public Int2 PointToIndexClamped(in Vector2 point)
            {
                return this.SandGrid.PointToIndexClamped(in point);
            }
        }
    }
}

