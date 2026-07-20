using System;
using App.Scripts.Core.Common;
using App.Scripts.Engine.Time;
using App.Scripts.UI.Toolkit.Components;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.Loader
{
    public sealed class LoaderService : ILoaderService, IDisposable
    {
        private readonly IUnityTimeService _timeService;
        private LoaderView _loaderView;

        private int _scopeCount;
        private bool _isPinned;

        public bool IsVisible => _loaderView?.IsVisible ?? false;
        public int ActiveCount => _scopeCount + (_isPinned ? 1 : 0);

        private bool ShouldBeVisible => _isPinned || _scopeCount > 0;

        public LoaderService(IUnityTimeService timeService)
        {
            _timeService = timeService;
        }

        public void Initialize(VisualElement loaderContainer)
        {
            if (_loaderView != null)
            {
                Log.Warning("LoaderService already initialized");
                return;
            }

            _loaderView = new LoaderView(loaderContainer, _timeService);
            Log.Info("LoaderService initialized");
        }

        public void Show()
        {
            _isPinned = true;
            SyncVisibility();
        }

        public void Hide()
        {
            _isPinned = false;
            SyncVisibility();
        }

        public IDisposable ShowScope()
        {
            if (_loaderView == null)
            {
                Log.Error("LoaderService not initialized. Call Initialize() first.");
                return new NullScope();
            }

            _scopeCount++;
            SyncVisibility();
            return new Scope(this);
        }

        public void Dispose()
        {
            _loaderView?.Dispose();
            _loaderView = null;
        }

        private void ReleaseScope()
        {
            _scopeCount = Math.Max(0, _scopeCount - 1);
            SyncVisibility();
        }

        private void SyncVisibility()
        {
            if (_loaderView == null)
                return;

            if (ShouldBeVisible)
            {
                if (!_loaderView.IsVisible)
                    _loaderView.Show();
            }
            else if (_loaderView.IsVisible)
            {
                _loaderView.Hide();
            }
        }

        private sealed class Scope : IDisposable
        {
            private readonly LoaderService _service;
            private bool _disposed;

            public Scope(LoaderService service)
            {
                _service = service;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _service.ReleaseScope();
            }
        }

        private sealed class NullScope : IDisposable
        {
            public void Dispose() { }
        }
    }
}
