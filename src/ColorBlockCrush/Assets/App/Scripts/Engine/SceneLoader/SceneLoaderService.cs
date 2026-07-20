using System;
using System.Threading;
using App.Scripts.Core.Common;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;

namespace App.Scripts.Engine.SceneLoader
{
    public sealed class SceneLoaderService : ISceneLoaderService, IDisposable
    {
        private readonly Subject<string> _onSceneLoaded = new();
        private bool _isProcessing;

        public Observable<string> OnSceneLoaded => _onSceneLoaded;

        public async UniTask LoadAsync(string sceneName, CancellationToken ct = default)
        {
            if (_isProcessing)
            {
                Log.Error($"LoadAsync({sceneName}) ignored — another scene load is in progress");
                return;
            }

            _isProcessing = true;

            try
            {
                await SceneManager.LoadSceneAsync(sceneName).ToUniTask(cancellationToken: ct);
                _onSceneLoaded.OnNext(sceneName);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void Dispose()
        {
            _onSceneLoaded.Dispose();
        }
    }
}