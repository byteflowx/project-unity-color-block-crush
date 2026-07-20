namespace App.Scripts.Domain.Match
{
    public readonly struct MatchEvent
    {
        private MatchEvent(
            MatchEventType type,
            int unitId,
            int laneIndex,
            int slotIndex,
            int cellId,
            int column,
            BlockColor color,
            float duration,
            int charges)
        {
            Type = type;
            UnitId = unitId;
            LaneIndex = laneIndex;
            SlotIndex = slotIndex;
            CellId = cellId;
            Column = column;
            Color = color;
            Duration = duration;
            Charges = charges;
        }

        public MatchEventType Type { get; }
        public int UnitId { get; }
        public int LaneIndex { get; }
        public int SlotIndex { get; }
        public int CellId { get; }
        public int Column { get; }
        public BlockColor Color { get; }
        public float Duration { get; }
        public int Charges { get; }

        public static MatchEvent UnitDeployed(int unitId, int laneIndex, int slotIndex, BlockColor color, float duration, int charges)
            => new MatchEvent(MatchEventType.UnitDeployed, unitId, laneIndex, slotIndex, -1, -1, color, duration, charges);

        public static MatchEvent ShotFired(int unitId, int slotIndex, int cellId, int column, BlockColor color, float duration, int charges)
            => new MatchEvent(MatchEventType.ShotFired, unitId, -1, slotIndex, cellId, column, color, duration, charges);

        public static MatchEvent PieceDestroyed(int cellId, int column, BlockColor color)
            => new MatchEvent(MatchEventType.PieceDestroyed, -1, -1, -1, cellId, column, color, 0f, 0);

        public static MatchEvent LayerRevealed(int cellId, int column, BlockColor color)
            => new MatchEvent(MatchEventType.LayerRevealed, -1, -1, -1, cellId, column, color, 0f, 0);

        public static MatchEvent ColumnCollapsed(int column, float duration)
            => new MatchEvent(MatchEventType.ColumnCollapsed, -1, -1, -1, -1, column, BlockColor.Red, duration, 0);

        public static MatchEvent UnitFinished(int unitId, int slotIndex)
            => new MatchEvent(MatchEventType.UnitFinished, unitId, -1, slotIndex, -1, -1, BlockColor.Red, 0f, 0);

        public static MatchEvent DeployRejected(int laneIndex)
            => new MatchEvent(MatchEventType.DeployRejected, -1, laneIndex, -1, -1, -1, BlockColor.Red, 0f, 0);

        public static MatchEvent Outcome(MatchEventType type)
            => new MatchEvent(type, -1, -1, -1, -1, -1, BlockColor.Red, 0f, 0);
    }
}
