using System;
using System.Collections.Generic;
using System.Threading;
using App.Scripts.Core.Common;
using App.Scripts.UI.Services.Notification;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Base
{
    public abstract class UIView : IDisposable
    {
        protected VisualElement Root { get; private set; }
        protected EventRegistry Events { get; private set; }
        protected INotificationService Notifications { get; private set; }

        protected abstract string VisibleClass { get; }
        protected abstract string HiddenClass { get; }

        private CancellationTokenSource _displayCts;

        public void Initialize(VisualElement root, INotificationService notifications)
        {
            Root = root;
            Notifications = notifications;
            Events = new EventRegistry();

            SetVisualElements();
            RegisterCallbacks();

            HideImmediately();
        }


        protected UniTask<bool> TryAsync<T>(
            UniTask<Result<T>> task,
            Action<T> onSuccess = null,
            CancellationToken ct = default)
            => Notifications.TryAsync(task, onSuccess, ct);

        protected ResultHandler<T> Handle<T>(UniTask<Result<T>> task)
            => Notifications.Handle(task);

        protected ResultHandler<T> Handle<T>(Func<CancellationToken, UniTask<Result<T>>> factory)
            => Notifications.Handle(factory);

        protected abstract void SetVisualElements();
        protected abstract void RegisterCallbacks();

        public virtual void Show()
        {
            Root.style.display = DisplayStyle.Flex;
            Root.RemoveFromClassList(HiddenClass);
            Root.AddToClassList(VisibleClass);
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            Root.RemoveFromClassList(VisibleClass);
            Root.AddToClassList(HiddenClass);
            Root.style.display = DisplayStyle.None;
        }

        public void HideImmediately()
        {
            Root.style.display = DisplayStyle.None;
            Root.RemoveFromClassList(VisibleClass);
            Root.RemoveFromClassList(HiddenClass);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected virtual async UniTask ShowAsync(float delay = 0f, CancellationToken ct = default)
        {
            CancelDisplayTransition();
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _displayCts = cts;

            try
            {
                if (delay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token);
                }

                Root.style.display = DisplayStyle.Flex;
                Root.RemoveFromClassList(HiddenClass);
                Root.AddToClassList(VisibleClass);
            }
            finally
            {
                if (_displayCts == cts)
                    _displayCts = null;

                cts.Dispose();
            }
        }

        protected virtual async UniTask HideAsync(float delay = 0f, CancellationToken ct = default)
        {
            CancelDisplayTransition();
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _displayCts = cts;

            try
            {
                if (delay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token);
                }

                Root.RemoveFromClassList(VisibleClass);
                Root.AddToClassList(HiddenClass);
            }
            finally
            {
                if (_displayCts == cts)
                    _displayCts = null;

                cts.Dispose();
            }
        }

        private void CancelDisplayTransition()
        {
            _displayCts?.Cancel();
            _displayCts?.Dispose();
            _displayCts = null;
        }

        public virtual void Dispose()
        {
            CancelDisplayTransition();
            Events?.Dispose();
        }
    }
}
