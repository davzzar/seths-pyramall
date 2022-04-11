namespace SandPerSand.SandSim
{
    /// <summary>
    /// Stores the collected information of a ShapeCast against sand data.
    /// </summary>
    public readonly struct SandCastResult
    {
        /// <summary>
        /// The intersection area in world units squared.
        /// </summary>
        public readonly float OverlapArea;

        /// <summary>
        /// The number of cells that intersected.
        /// </summary>
        public readonly int OverlapCellCount;

        public SandCastResult(float overlapArea, int overlapCellCount)
        {
            this.OverlapArea = overlapArea;
            this.OverlapCellCount = overlapCellCount;
        }
    }
}