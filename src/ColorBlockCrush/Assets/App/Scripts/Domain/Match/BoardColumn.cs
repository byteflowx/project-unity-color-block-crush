using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class BoardColumn
    {
        private readonly List<BoardCell> _cells;

        public BoardColumn(int index, IReadOnlyList<BoardCell> bottomToTop)
        {
            Index = index;
            _cells = new List<BoardCell>(bottomToTop);
        }

        public int Index { get; }
        public int Count => _cells.Count;
        public bool IsEmpty => _cells.Count == 0;

        public float SettleTimer { get; private set; }
        public bool IsSettling => SettleTimer > 0f;
        public bool IsTargetable => _cells.Count > 0 && SettleTimer <= 0f;

        public BoardCell this[int row] => _cells[row];
        public BoardCell Bottom => _cells[0];

        public void RemoveBottom(float settleDuration)
        {
            _cells.RemoveAt(0);

            if (_cells.Count > 0)
                SettleTimer = settleDuration;
        }

        public void Advance(float deltaTime)
        {
            if (SettleTimer <= 0f)
                return;

            SettleTimer -= deltaTime;

            if (SettleTimer < 0f)
                SettleTimer = 0f;
        }
    }
}
