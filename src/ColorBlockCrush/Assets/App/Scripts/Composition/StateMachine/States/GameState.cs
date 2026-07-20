using System;
using System.Threading;
using App.Scripts.AppFlow;
using App.Scripts.Core.Common;
using App.Scripts.UI.Services.Loader;
using App.Scripts.UI.Services.UIService;
using App.Scripts.UI.Toolkit.Popups;
using App.Scripts.UI.Toolkit.Screens;
using Cysharp.Threading.Tasks;
using R3;

namespace App.Scripts.Composition.StateMachine.States
{
    public class GameState : IState, IEnterableAsyncState, IDisposable
    {
        private readonly ILoaderService _loaderService;
        private readonly IUIService _uiService;
        private readonly IGameSessionService _session;

        private DisposableBag _disposables;
        private bool _subscribed;

        public GameState(ILoaderService loaderService, IUIService uiService, IGameSessionService session)
        {
            _loaderService = loaderService;
            _uiService = uiService;
            _session = session;
        }

        public async UniTask<Next> EnterAsync(CancellationToken ct = default)
        {
            _loaderService.Hide();

            Subscribe();

            await _uiService.ShowScreenAsync<GameScreen>(ct: ct);

            return Next.None;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void Subscribe()
        {
            if (_subscribed)
                return;

            _subscribed = true;

            _session.OnLevelCompleted.Subscribe(this, static (_, self) => self.ShowCompletePopup()).AddTo(ref _disposables);
            _session.OnLevelFailed.Subscribe(this, static (_, self) => self.ShowFailedPopup()).AddTo(ref _disposables);
        }

        private void ShowCompletePopup()
        {
            _uiService.ShowPopupAsync<LevelCompletePopup>().Forget(Log.Exception);
        }

        private void ShowFailedPopup()
        {
            _uiService.ShowPopupAsync<LevelFailedPopup>().Forget(Log.Exception);
        }
    }
}
