using System;
using System.Threading;
using App.Scripts.AppFlow;
using App.Scripts.Gameplay.Levels;
using App.Scripts.Gameplay.View;
using Cysharp.Threading.Tasks;
using R3;

namespace App.Scripts.Gameplay
{
    public sealed class GameSessionService : IGameSessionService, IDisposable
    {
        private readonly ILevelService _levelService;

        private readonly ReactiveProperty<int> _levelId = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<float> _progress = new ReactiveProperty<float>(0f);
        private readonly Subject<Unit> _levelCompleted = new Subject<Unit>();
        private readonly Subject<Unit> _levelFailed = new Subject<Unit>();

        private DisposableBag _presenterSubscriptions;
        private GameplayPresenter _presenter;
        private bool _isTransitioning;

        public GameSessionService(ILevelService levelService)
        {
            _levelService = levelService;
        }

        public ReadOnlyReactiveProperty<int> LevelId => _levelId;
        public ReadOnlyReactiveProperty<float> Progress => _progress;
        public Observable<Unit> OnLevelCompleted => _levelCompleted;
        public Observable<Unit> OnLevelFailed => _levelFailed;

        public void Attach(GameplayPresenter presenter)
        {
            Detach();

            _presenter = presenter;

            presenter.OnWon.Subscribe(this, static (_, self) => self.HandleWon()).AddTo(ref _presenterSubscriptions);
            presenter.OnLost.Subscribe(this, static (_, self) => self.HandleLost()).AddTo(ref _presenterSubscriptions);
            presenter.Progress.Subscribe(this, static (value, self) => self.SetProgress(value)).AddTo(ref _presenterSubscriptions);
        }

        public void Detach()
        {
            _presenterSubscriptions.Dispose();
            _presenterSubscriptions = new DisposableBag();
            _presenter = null;
        }

        public void NotifyLevelStarted()
        {
            _levelId.Value = _levelService.CurrentLevelId;
            _progress.Value = 0f;
            _isTransitioning = false;
        }

        public async UniTask RestartAsync(CancellationToken ct = default)
        {
            if (_isTransitioning || _presenter == null)
                return;

            _isTransitioning = true;

            await _presenter.StartLevelAsync(ct);
            NotifyLevelStarted();
        }

        public async UniTask NextLevelAsync(CancellationToken ct = default)
        {
            if (_isTransitioning || _presenter == null)
                return;

            _isTransitioning = true;

            await _levelService.AdvanceAsync(ct);
            await _presenter.StartLevelAsync(ct);
            NotifyLevelStarted();
        }

        public void Dispose()
        {
            Detach();
            _levelId.Dispose();
            _progress.Dispose();
            _levelCompleted.Dispose();
            _levelFailed.Dispose();
        }

        private void SetProgress(float value)
        {
            _progress.Value = value;
        }

        private void HandleWon()
        {
            _levelCompleted.OnNext(Unit.Default);
        }

        private void HandleLost()
        {
            _levelFailed.OnNext(Unit.Default);
        }
    }
}
