using System;
using System.Threading;
using App.Scripts.Core.Common;
using App.Scripts.Core.Enums;
using App.Scripts.UI.Services.Loader;
using App.Scripts.UI.Toolkit.Components;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.Notification
{
    public sealed class NotificationService : INotificationService, IDisposable
    {
        private readonly ILoaderService _loaderService;
        private ToastView _toastView;
        private ErrorView _errorView;

        public NotificationService(ILoaderService loaderService)
        {
            _loaderService = loaderService;
        }

        public void Initialize(VisualElement toastContainer, VisualElement errorContainer)
        {
            _toastView = new ToastView(toastContainer);
            _errorView = new ErrorView(errorContainer);
        }

        public void ShowToast(string message, ToastType type = ToastType.Info, float duration = 3f)
        {
            _toastView.Show(message, type, duration);
        }

        public UniTask<ErrorAction> ShowPopupAsync(string message, ErrorAction actions, CancellationToken ct = default)
        {
            Log.Error($"Error popup: {message}");

            return _errorView.ShowAsync(message, actions, ct);
        }

        public void ShowError(ServiceError error)
        {
            switch (error.Severity)
            {
                case ErrorSeverity.Info:
                    ShowToast(error.Message, ToastType.Info);
                    break;
                case ErrorSeverity.Warning:
                    ShowToast(error.Message, ToastType.Warning);
                    break;
                case ErrorSeverity.Error:
                    ShowToast(error.Message, ToastType.Error);
                    break;
                case ErrorSeverity.Critical:
                    ShowPopupAsync(error.Message, ErrorAction.Skip).Forget(Log.Exception);
                    break;
            }
        }

        public UniTask<bool> TryAsync<T>(
            UniTask<Result<T>> task,
            Action<T> onSuccess = null,
            CancellationToken ct = default)
        {
            ResultHandler<T> handler = new(this, _loaderService, task);
            handler.ToastOnError();
            if (onSuccess != null)
                handler.OnSuccess(onSuccess);
            return handler.ExecuteAsync(ct);
        }

        public ResultHandler<T> Handle<T>(UniTask<Result<T>> task) =>
            new(this, _loaderService, task);

        public ResultHandler<T> Handle<T>(Func<CancellationToken, UniTask<Result<T>>> factory) =>
            new(this, _loaderService, factory);

        public void Dispose()
        {
            _toastView?.Dispose();
            _errorView?.Dispose();
            _toastView = null;
            _errorView = null;
        }
    }
}
