namespace App.Scripts.Domain.Match
{
    public struct Projectile
    {
        public int UnitId;
        public int SlotIndex;
        public BoardCell Target;
        public BlockColor Color;
        public float Remaining;
    }
}
