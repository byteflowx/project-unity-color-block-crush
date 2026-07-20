using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace App.Scripts.AppFlow
{
    public interface IGameSessionService
    {
        ReadOnlyReactiveProperty<int> LevelId { get; }
        ReadOnlyReactiveProperty<float> Progress { get; }

        Observable<Unit> OnLevelCompleted { get; }
        Observable<Unit> OnLevelFailed { get; }

        UniTask RestartAsync(CancellationToken ct = default);
        UniTask NextLevelAsync(CancellationToken ct = default);
    }
}
