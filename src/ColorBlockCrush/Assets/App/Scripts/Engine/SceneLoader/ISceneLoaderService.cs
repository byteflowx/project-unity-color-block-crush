using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace App.Scripts.Engine.SceneLoader
{
    public interface ISceneLoaderService
    {
        Observable<string> OnSceneLoaded { get; }
        UniTask LoadAsync(string sceneName, CancellationToken ct = default);
    }
}