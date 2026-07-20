using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class LevelDefinition
    {
        public LevelDefinition(int id, int columns, List<List<string>> cells, List<List<LevelUnitDefinition>> lanes)
        {
            Id = id;
            Columns = columns;
            Cells = cells;
            Lanes = lanes;
        }

        public int Id { get; }
        public int Columns { get; }
        public IReadOnlyList<IReadOnlyList<string>> Cells { get; }
        public IReadOnlyList<IReadOnlyList<LevelUnitDefinition>> Lanes { get; }
    }
}
