using System;
using System.Collections.Generic;
using System.Threading;
using App.Scripts.Core.Constants;
using App.Scripts.Domain.Match;
using App.Scripts.Engine.Addressable;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace App.Scripts.Gameplay.Levels
{
    public sealed class LevelService : ILevelService
    {
        private const int INTRO_LEVEL_COUNT = 2;

        private static readonly int[] ROTATION_LEVEL_IDS = { 3, 4, 5 };

        private readonly IAddressableService _addressables;
        private readonly System.Random _random = new System.Random();
        private readonly List<int> _cycleOrder = new List<int>(ROTATION_LEVEL_IDS.Length);

        private int _introIndex;
        private int _cyclePosition;

        public LevelService(IAddressableService addressables)
        {
            _addressables = addressables;
        }

        public int CurrentLevelId
            => _introIndex < INTRO_LEVEL_COUNT ? _introIndex + 1 : _cycleOrder[_cyclePosition];

        public UniTask InitializeAsync(CancellationToken ct = default)
        {
            _introIndex = 0;
            _cyclePosition = 0;

            BuildCycle(0);

            return UniTask.CompletedTask;
        }

        public async UniTask<LevelDefinition> LoadCurrentAsync(CancellationToken ct = default)
        {
            string address = AddressableConstants.LevelAddress(CurrentLevelId);
            TextAsset asset = await _addressables.Load<TextAsset>(address, ct);

            if (asset == null)
                throw new InvalidOperationException($"Level asset not found: {address}");

            return JsonConvert.DeserializeObject<LevelDefinition>(asset.text);
        }

        public UniTask AdvanceAsync(CancellationToken ct = default)
        {
            int completedId = CurrentLevelId;

            if (_introIndex < INTRO_LEVEL_COUNT)
            {
                _introIndex++;

                if (_introIndex >= INTRO_LEVEL_COUNT)
                    BuildCycle(completedId);

                return UniTask.CompletedTask;
            }

            _cyclePosition++;

            if (_cyclePosition >= _cycleOrder.Count)
            {
                _cyclePosition = 0;
                BuildCycle(completedId);
            }

            return UniTask.CompletedTask;
        }

        private void BuildCycle(int lastPlayedId)
        {
            _cycleOrder.Clear();
            _cycleOrder.AddRange(ROTATION_LEVEL_IDS);

            for (int i = _cycleOrder.Count - 1; i > 0; i--)
            {
                int swapIndex = _random.Next(i + 1);
                (_cycleOrder[i], _cycleOrder[swapIndex]) = (_cycleOrder[swapIndex], _cycleOrder[i]);
            }

            if (_cycleOrder[0] == lastPlayedId)
                (_cycleOrder[0], _cycleOrder[1]) = (_cycleOrder[1], _cycleOrder[0]);
        }
    }
}
