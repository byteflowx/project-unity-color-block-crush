namespace App.Scripts.Domain.Match
{
    public sealed class MatchConfig
    {
        public const float DEFAULT_SETTLE_DURATION = 0.15f;
        public const float DEFAULT_FIRE_INTERVAL = 0.25f;
        public const float DEFAULT_TRAVEL_DURATION = 0.35f;
        public const float DEFAULT_PROJECTILE_MIN_FLIGHT = 0.08f;
        public const float DEFAULT_PROJECTILE_SECONDS_PER_COLUMN = 0.018f;
        public const float DEFAULT_LOSE_GRACE = 0.5f;

        public MatchConfig(
            float settleDuration,
            float fireInterval,
            float travelDuration,
            float projectileMinFlight,
            float projectileSecondsPerColumn,
            float loseGrace)
        {
            SettleDuration = settleDuration;
            FireInterval = fireInterval;
            TravelDuration = travelDuration;
            ProjectileMinFlight = projectileMinFlight;
            ProjectileSecondsPerColumn = projectileSecondsPerColumn;
            LoseGrace = loseGrace;
        }

        public float SettleDuration { get; }
        public float FireInterval { get; }
        public float TravelDuration { get; }
        public float ProjectileMinFlight { get; }
        public float ProjectileSecondsPerColumn { get; }
        public float LoseGrace { get; }

        public static MatchConfig CreateDefault()
            => new MatchConfig(
                DEFAULT_SETTLE_DURATION,
                DEFAULT_FIRE_INTERVAL,
                DEFAULT_TRAVEL_DURATION,
                DEFAULT_PROJECTILE_MIN_FLIGHT,
                DEFAULT_PROJECTILE_SECONDS_PER_COLUMN,
                DEFAULT_LOSE_GRACE);
    }
}
