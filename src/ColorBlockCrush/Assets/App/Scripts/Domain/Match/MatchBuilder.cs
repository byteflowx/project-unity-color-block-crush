using System;
using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public static class MatchBuilder
    {
        public static MatchSimulation Build(LevelDefinition level, MatchConfig config, int randomSeed)
        {
            int rows = level.Cells.Count;
            int nextCellId = 0;
            int totalPieces = 0;

            List<BoardColumn> boardColumns = new List<BoardColumn>(level.Columns);

            for (int column = 0; column < level.Columns; column++)
            {
                List<BoardCell> cells = new List<BoardCell>(rows);

                for (int row = rows - 1; row >= 0; row--)
                {
                    string stack = level.Cells[row][column];

                    if (string.IsNullOrEmpty(stack))
                        continue;

                    List<BlockColor> bottomToTop = new List<BlockColor>(stack.Length);

                    for (int i = stack.Length - 1; i >= 0; i--)
                        bottomToTop.Add(ParseColorCode(stack[i]));

                    totalPieces += stack.Length;
                    cells.Add(new BoardCell(nextCellId++, column, bottomToTop));
                }

                boardColumns.Add(new BoardColumn(column, cells));
            }

            BoardModel board = new BoardModel(boardColumns, totalPieces);

            List<QueueLane> lanes = new List<QueueLane>(level.Lanes.Count);
            int nextUnitId = 0;

            for (int laneIndex = 0; laneIndex < level.Lanes.Count; laneIndex++)
            {
                IReadOnlyList<LevelUnitDefinition> source = level.Lanes[laneIndex];
                List<UnitModel> units = new List<UnitModel>(source.Count);

                for (int i = 0; i < source.Count; i++)
                    units.Add(new UnitModel(nextUnitId++, ParseColorName(source[i].Color), source[i].Charges));

                lanes.Add(new QueueLane(laneIndex, units));
            }

            return new MatchSimulation(board, lanes, config, randomSeed);
        }

        private static BlockColor ParseColorCode(char code)
        {
            switch (code)
            {
                case 'R': return BlockColor.Red;
                case 'B': return BlockColor.Blue;
                case 'Y': return BlockColor.Yellow;
                case 'G': return BlockColor.Green;
                case 'P': return BlockColor.Purple;
                default: throw new ArgumentException($"Unknown block color code '{code}'");
            }
        }

        private static BlockColor ParseColorName(string name)
        {
            switch (name)
            {
                case "Red": return BlockColor.Red;
                case "Blue": return BlockColor.Blue;
                case "Yellow": return BlockColor.Yellow;
                case "Green": return BlockColor.Green;
                case "Purple": return BlockColor.Purple;
                default: throw new ArgumentException($"Unknown block color name '{name}'");
            }
        }
    }
}
