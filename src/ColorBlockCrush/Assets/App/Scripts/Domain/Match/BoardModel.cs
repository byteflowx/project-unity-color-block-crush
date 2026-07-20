using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class BoardModel
    {
        private readonly List<BoardColumn> _columns;

        public BoardModel(IReadOnlyList<BoardColumn> columns, int totalPieces)
        {
            _columns = new List<BoardColumn>(columns);
            TotalPieces = totalPieces;
            RemainingPieces = totalPieces;
        }

        public int ColumnCount => _columns.Count;
        public int TotalPieces { get; }
        public int RemainingPieces { get; private set; }
        public bool IsCleared => RemainingPieces == 0;

        public BoardColumn GetColumn(int index) => _columns[index];

        public void Advance(float deltaTime)
        {
            for (int i = 0; i < _columns.Count; i++)
                _columns[i].Advance(deltaTime);
        }

        public void CollectFrontLine(List<BoardCell> buffer)
        {
            buffer.Clear();

            for (int i = 0; i < _columns.Count; i++)
            {
                BoardColumn column = _columns[i];

                if (!column.IsTargetable)
                    continue;

                buffer.Add(column.Bottom);
            }
        }

        public bool HasTargetableColor(BlockColor color)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                BoardColumn column = _columns[i];

                if (!column.IsTargetable)
                    continue;

                if (column.Bottom.TopColor == color)
                    return true;
            }

            return false;
        }

        public bool ConsumeTop(BoardCell cell, float settleDuration)
        {
            cell.PopTop();
            RemainingPieces--;

            if (!cell.IsEmpty)
                return false;

            _columns[cell.Column].RemoveBottom(settleDuration);
            return true;
        }
    }
}
