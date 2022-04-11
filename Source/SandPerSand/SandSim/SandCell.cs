using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SandPerSand.SandSim
{
    [StructLayout(LayoutKind.Sequential, Pack = sizeof(uint), Size = sizeof(uint))]
    public struct SandCell
    {
        private const uint HasSandMask = 0b0000_0000_0000_0000_0000_0000_0000_0001;

        private const uint HasObstacleMask = 0b0000_0000_0000_0000_0000_0000_0000_0010;

        private const uint HasDrainMask = 0b0000_0000_0000_0000_0000_0000_0000_0100;

        private const uint IsSandStableMask = 0b0000_0000_0000_0000_0000_0000_0001_0000;

        private const uint IsSandSourceMask = 0b0000_0000_0000_0000_0000_0000_0010_0000;

        private const uint HorizontalFlowMask = 0b0000_0000_0000_0000_0000_0011_0000_0000;

        private const int HorizontalFlowOffset = 0x08;

        private const uint VerticalFlowMask = 0b0000_0000_0000_0000_0000_1100_0000_0000;

        private const int VerticalFlowOffset = 0x0A;

        private const uint LayerMask = 0b0000_0000_1111_1111_0000_0000_0000_0000;

        private const int LayerOffset = 0x10;

        public uint data;

        public readonly bool IsEmpty => (this.data & (HasSandMask | HasObstacleMask)) == 0;

        /// <summary>
        /// Gets or sets a value indicating whether this cell contains sand.
        /// </summary>
        public readonly bool HasSand
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => (this.data & HasSandMask) == HasSandMask;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this cell contains an obstacle.
        /// </summary>
        public readonly bool HasObstacle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => (this.data & HasObstacleMask) == HasObstacleMask;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this cell contains a drain.
        /// </summary>
        public readonly bool HasDrain
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => (this.data & HasDrainMask) == HasDrainMask;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sand in this cell is considered stable.
        /// </summary>
        public bool IsSandStable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            readonly get => (this.data & IsSandStableMask) == IsSandStableMask;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    this.data |= IsSandStableMask;
                }
                else
                {
                    unchecked
                    {
                        this.data = (uint)~IsSandStableMask;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sand in this cell is a source.
        /// </summary>
        public bool IsSandSource
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            readonly get => (this.data & IsSandSourceMask) == IsSandSourceMask;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    this.data |= IsSandSourceMask;
                }
                else
                {
                    unchecked
                    {
                        this.data = (uint)~IsSandSourceMask;
                    }
                }
            }
        }

        public readonly bool IsSolidUnderground => this.HasObstacle || (this.HasSand && this.IsSandStable);

        /// <summary>
        /// Gets or sets a value representing the horizontal flow of the sand in this cell.
        /// </summary>
        public HorizontalFlow HorizontalFlow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            readonly get => (HorizontalFlow)((this.data & HorizontalFlowMask) >> HorizontalFlowOffset);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                Debug.Assert(value >= HorizontalFlow.None);
                Debug.Assert(value <= HorizontalFlow.Right);

                unchecked
                {
                    this.data = (uint)((this.data & ~HorizontalFlowMask) | (uint)value << HorizontalFlowOffset);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value representing the vertical flow of the sand in this cell.
        /// </summary>
        public VerticalFlow VerticalFlow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            readonly get => (VerticalFlow)((this.data & VerticalFlowMask) >> VerticalFlowOffset);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                Debug.Assert(value >= VerticalFlow.None);
                Debug.Assert(value <= VerticalFlow.Up);

                unchecked
                {
                    this.data = (uint)((this.data & ~VerticalFlowMask) | (uint)value << VerticalFlowOffset);
                }
            }
        }

        /// <summary>
        /// Gets or sets the layer value of the sand in this cell.
        /// </summary>
        public byte Layer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            readonly get => (byte)((uint)(this.data & LayerMask) >> LayerOffset);

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                unchecked
                {
                    this.data = (uint)((this.data & ~LayerMask) | (uint)value << LayerOffset);
                }
            }
        }

        /// <summary>
        /// Marks this cell to contain nothing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkEmpty()
        {
            this.data = 0;
        }

        /// <summary>
        /// Marks this cell to contain an obstacle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkObstacle()
        {
            this.data = HasObstacleMask;
        }

        /// <summary>
        /// Marks this cell to contain sand.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkSand()
        {
            this.data = HasSandMask;
        }

        /// <summary>
        /// Marks this cell to contain sand.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkSandSource()
        {
            this.data = HasSandMask | IsSandSourceMask;
        }

        /// <summary>
        /// Marks this cell to contain a drain.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkDrain()
        {
            this.data = HasDrainMask;
        }
    }
}