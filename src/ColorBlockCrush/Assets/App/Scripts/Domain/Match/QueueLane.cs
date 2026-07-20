using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class QueueLane
    {
        private readonly List<UnitModel> _units;

        public QueueLane(int index, IReadOnlyList<UnitModel> headToTail)
        {
            Index = index;
            _units = new List<UnitModel>(headToTail);
        }

        public int Index { get; }
        public int Count => _units.Count;
        public bool IsEmpty => _units.Count == 0;

        public UnitModel Head => _units[0];

        public UnitModel this[int position] => _units[position];

        public UnitModel PopHead()
        {
            UnitModel head = _units[0];
            _units.RemoveAt(0);
            return head;
        }
    }
}
