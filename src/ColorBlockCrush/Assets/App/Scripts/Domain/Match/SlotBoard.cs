using System;
using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class SlotBoard
    {
        public const int SLOT_COUNT = 5;

        private readonly UnitModel[] _slots = new UnitModel[SLOT_COUNT];
        private readonly List<int> _freeBuffer = new List<int>(SLOT_COUNT);

        public UnitModel this[int slotIndex] => _slots[slotIndex];

        public bool HasFreeSlot
        {
            get
            {
                for (int i = 0; i < SLOT_COUNT; i++)
                {
                    if (_slots[i] == null)
                        return true;
                }

                return false;
            }
        }

        public bool TryGetRandomFreeSlot(Random random, out int slotIndex)
        {
            _freeBuffer.Clear();

            for (int i = 0; i < SLOT_COUNT; i++)
            {
                if (_slots[i] == null)
                    _freeBuffer.Add(i);
            }

            if (_freeBuffer.Count == 0)
            {
                slotIndex = -1;
                return false;
            }

            slotIndex = _freeBuffer[random.Next(_freeBuffer.Count)];
            return true;
        }

        public void Occupy(int slotIndex, UnitModel unit)
        {
            _slots[slotIndex] = unit;
        }

        public void Release(int slotIndex)
        {
            _slots[slotIndex] = null;
        }
    }
}
