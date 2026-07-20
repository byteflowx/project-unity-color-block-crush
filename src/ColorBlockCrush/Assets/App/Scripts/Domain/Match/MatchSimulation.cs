using System;
using System.Collections.Generic;

namespace App.Scripts.Domain.Match
{
    public sealed class MatchSimulation
    {
        private readonly BoardModel _board;
        private readonly List<QueueLane> _lanes;
        private readonly SlotBoard _slots = new SlotBoard();
        private readonly MatchConfig _config;
        private readonly Random _random;

        private readonly List<Projectile> _projectiles = new List<Projectile>(16);
        private readonly List<BoardCell> _frontLine = new List<BoardCell>(16);
        private readonly List<MatchEvent> _events = new List<MatchEvent>(32);

        private float _loseTimer;

        public MatchSimulation(BoardModel board, IReadOnlyList<QueueLane> lanes, MatchConfig config, int randomSeed)
        {
            _board = board;
            _lanes = new List<QueueLane>(lanes);
            _config = config;
            _random = new Random(randomSeed);
            State = MatchState.Playing;
        }

        public MatchState State { get; private set; }
        public BoardModel Board => _board;
        public SlotBoard Slots => _slots;
        public MatchConfig Config => _config;
        public int LaneCount => _lanes.Count;

        public QueueLane GetLane(int index) => _lanes[index];

        public float Progress
            => _board.TotalPieces == 0 ? 1f : 1f - (float)_board.RemainingPieces / _board.TotalPieces;

        public void ConsumeEvents(List<MatchEvent> buffer)
        {
            buffer.Clear();
            buffer.AddRange(_events);
            _events.Clear();
        }

        public bool TryDeploy(int laneIndex)
        {
            if (State != MatchState.Playing)
                return false;

            QueueLane lane = _lanes[laneIndex];

            if (lane.IsEmpty || !_slots.TryGetRandomFreeSlot(_random, out int slotIndex))
            {
                _events.Add(MatchEvent.DeployRejected(laneIndex));
                return false;
            }

            UnitModel unit = lane.PopHead();
            unit.Deploy(slotIndex, _config.TravelDuration);
            _slots.Occupy(slotIndex, unit);

            _events.Add(MatchEvent.UnitDeployed(unit.Id, laneIndex, slotIndex, unit.Color, _config.TravelDuration, unit.RemainingCharges));
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (State != MatchState.Playing)
                return;

            _board.Advance(deltaTime);
            AdvanceUnits(deltaTime);
            AdvanceProjectiles(deltaTime);
            FireReadyUnits();
            EvaluateOutcome(deltaTime);
        }

        private void AdvanceUnits(float deltaTime)
        {
            for (int i = 0; i < SlotBoard.SLOT_COUNT; i++)
            {
                UnitModel unit = _slots[i];

                if (unit == null)
                    continue;

                if (unit.State == UnitState.Travelling)
                    unit.AdvanceTravel(deltaTime);
                else if (unit.State == UnitState.Active)
                    unit.AdvanceCooldown(deltaTime);
            }
        }

        private void AdvanceProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _projectiles[i];
                projectile.Remaining -= deltaTime;

                if (projectile.Remaining > 0f)
                {
                    _projectiles[i] = projectile;
                    continue;
                }

                _projectiles.RemoveAt(i);
                ResolveImpact(projectile);
            }
        }

        private void ResolveImpact(Projectile projectile)
        {
            BoardCell target = projectile.Target;
            target.IsTargeted = false;

            int column = target.Column;
            BlockColor destroyed = target.TopColor;
            bool cellEmptied = _board.ConsumeTop(target, _config.SettleDuration);

            _events.Add(MatchEvent.PieceDestroyed(target.Id, column, destroyed));

            if (cellEmptied)
                _events.Add(MatchEvent.ColumnCollapsed(column, _config.SettleDuration));
            else
                _events.Add(MatchEvent.LayerRevealed(target.Id, column, target.TopColor));
        }

        private void FireReadyUnits()
        {
            _board.CollectFrontLine(_frontLine);

            for (int i = 0; i < SlotBoard.SLOT_COUNT; i++)
            {
                UnitModel unit = _slots[i];

                if (unit == null || !unit.IsReadyToFire)
                    continue;

                BoardCell target = FindNearestTarget(unit);

                if (target == null)
                    continue;

                target.IsTargeted = true;

                float flight = ComputeFlightDuration(unit.SlotIndex, target.Column);

                _projectiles.Add(new Projectile
                {
                    UnitId = unit.Id,
                    SlotIndex = unit.SlotIndex,
                    Target = target,
                    Color = unit.Color,
                    Remaining = flight
                });

                unit.SpendCharge(_config.FireInterval);

                _events.Add(MatchEvent.ShotFired(unit.Id, unit.SlotIndex, target.Id, target.Column, unit.Color, flight, unit.RemainingCharges));

                if (unit.State != UnitState.Finished)
                    continue;

                _slots.Release(i);
                _events.Add(MatchEvent.UnitFinished(unit.Id, i));
            }
        }

        private BoardCell FindNearestTarget(UnitModel unit)
        {
            float slotPosition = SlotToColumnSpace(unit.SlotIndex);
            BoardCell best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < _frontLine.Count; i++)
            {
                BoardCell cell = _frontLine[i];

                if (cell.IsTargeted || cell.TopColor != unit.Color)
                    continue;

                float distance = Math.Abs(cell.Column - slotPosition);

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = cell;
            }

            return best;
        }

        private float SlotToColumnSpace(int slotIndex)
            => (slotIndex + 0.5f) / SlotBoard.SLOT_COUNT * _board.ColumnCount - 0.5f;

        private float ComputeFlightDuration(int slotIndex, int column)
        {
            float distance = Math.Abs(column - SlotToColumnSpace(slotIndex));
            return _config.ProjectileMinFlight + distance * _config.ProjectileSecondsPerColumn;
        }

        private void EvaluateOutcome(float deltaTime)
        {
            if (_board.IsCleared)
            {
                State = MatchState.Won;
                _events.Add(MatchEvent.Outcome(MatchEventType.Won));
                return;
            }

            if (!IsDeadlocked())
            {
                _loseTimer = 0f;
                return;
            }

            _loseTimer += deltaTime;

            if (_loseTimer < _config.LoseGrace)
                return;

            State = MatchState.Lost;
            _events.Add(MatchEvent.Outcome(MatchEventType.Lost));
        }

        private bool IsDeadlocked()
        {
            if (_projectiles.Count > 0)
                return false;

            for (int i = 0; i < _board.ColumnCount; i++)
            {
                if (_board.GetColumn(i).IsSettling)
                    return false;
            }

            for (int i = 0; i < SlotBoard.SLOT_COUNT; i++)
            {
                UnitModel unit = _slots[i];

                if (unit == null)
                    continue;

                if (unit.State == UnitState.Travelling)
                    return false;

                if (_board.HasTargetableColor(unit.Color))
                    return false;
            }

            if (!_slots.HasFreeSlot)
                return true;

            for (int i = 0; i < _lanes.Count; i++)
            {
                if (!_lanes[i].IsEmpty)
                    return false;
            }

            return true;
        }
    }
}
