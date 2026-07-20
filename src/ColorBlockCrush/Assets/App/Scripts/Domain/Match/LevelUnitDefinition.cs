namespace App.Scripts.Domain.Match
{
    public sealed class LevelUnitDefinition
    {
        public LevelUnitDefinition(string color, int charges)
        {
            Color = color;
            Charges = charges;
        }

        public string Color { get; }
        public int Charges { get; }
    }
}
