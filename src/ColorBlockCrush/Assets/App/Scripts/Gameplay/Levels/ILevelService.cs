using System.Threading;
using App.Scripts.Domain.Match;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Gameplay.Levels
{
    public interface ILevelService
    {
        int CurrentLevelId { get; }

        UniTask InitializeAsync(CancellationToken ct = default);

        UniTask<LevelDefinition> LoadCurrentAsync(CancellationToken ct = default);

        UniTask AdvanceAsync(CancellationToken ct = default);
    }
}
