namespace App.Scripts.Domain.Match
{
    public sealed class UnitModel
    {
        public UnitModel(int id, BlockColor color, int charges)
        {
            Id = id;
            Color = color;
            RemainingCharges = charges;
            State = UnitState.Queued;
        }

        public int Id { get; }
        public BlockColor Color { get; }
        public int RemainingCharges { get; private set; }
        public UnitState State { get; private set; }
        public int SlotIndex { get; private set; } = -1;
        public float TravelTimer { get; private set; }
        public float FireCooldown { get; private set; }

        public void Deploy(int slotIndex, float travelDuration)
        {
            SlotIndex = slotIndex;
            TravelTimer = travelDuration;
            State = UnitState.Travelling;
        }

        public void AdvanceTravel(float deltaTime)
        {
            TravelTimer -= deltaTime;

            if (TravelTimer > 0f)
                return;

            TravelTimer = 0f;
            State = UnitState.Active;
        }

        public void AdvanceCooldown(float deltaTime)
        {
            if (FireCooldown > 0f)
                FireCooldown -= deltaTime;
        }

        public bool IsReadyToFire => State == UnitState.Active && FireCooldown <= 0f;

        public void SpendCharge(float fireInterval)
        {
            RemainingCharges--;
            FireCooldown = fireInterval;

            if (RemainingCharges <= 0)
                State = UnitState.Finished;
        }
    }
}
