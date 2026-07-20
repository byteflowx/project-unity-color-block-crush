namespace App.Scripts.Domain.Match
{
    public enum MatchEventType
    {
        UnitDeployed = 0,
        DeployRejected = 1,
        ShotFired = 2,
        PieceDestroyed = 3,
        LayerRevealed = 4,
        ColumnCollapsed = 5,
        UnitFinished = 6,
        Won = 7,
        Lost = 8
    }
}
