using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class BoardCell
    {
        private readonly List<BlockColor> _stack;

        public BoardCell(int id, int column, IReadOnlyList<BlockColor> bottomToTop)
        {
            Id = id;
            Column = column;
            _stack = new List<BlockColor>(bottomToTop);
        }

        public int Id { get; }
        public int Column { get; }
        public bool IsTargeted { get; set; }

        public int Depth => _stack.Count;
        public bool IsEmpty => _stack.Count == 0;
        public BlockColor TopColor => _stack[_stack.Count - 1];

        public BlockColor ColorAt(int index) => _stack[index];

        public BlockColor PopTop()
        {
            int topIndex = _stack.Count - 1;
            BlockColor color = _stack[topIndex];
            _stack.RemoveAt(topIndex);
            return color;
        }
    }
}
