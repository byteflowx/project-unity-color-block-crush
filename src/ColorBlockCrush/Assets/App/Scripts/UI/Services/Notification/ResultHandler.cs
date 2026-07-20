using System;
using System.Collections.Generic;
using System.Threading;
using App.Scripts.Core.Common;
using App.Scripts.Core.Enums;
using App.Scripts.UI.Services.Loader;
using Cysharp.Threading.Tasks;

namespace App.Scripts.UI.Services.Notification
{
    public sealed class ResultHandler<T>
    {
        private enum ErrorMode { None, Toast, Popup, RetryPrompt }

        private readonly INotificationService _notifications;
        private readonly ILoaderService _loaderService;
        private readonly UniTask<Result<T>> _task;
        private readonly Func<CancellationToken, UniTask<Result<T>>> _factory;
        private readonly bool _hasFactory;
        private readonly Dictionary<string, Func<UniTask>> _errorHandlers = new();

        private Func<T, UniTask> _successHandler;
        private ErrorMode _errorMode = ErrorMode.None;

        private bool _withLoader;

        public ResultHandler(INotificationService notifications, ILoaderService loaderService, UniTask<Result<T>> task)
        {
            _notifications = notifications;
            _loaderService = loaderService;
            _task = task;
            _hasFactory = false;
        }

        public ResultHandler(INotificationService notifications, ILoaderService loaderService, Func<CancellationToken, UniTask<Result<T>>> factory)
        {
            _notifications = notifications;
            _loaderService = loaderService;
            _factory = factory;
            _hasFactory = true;
        }

        public ResultHandler<T> OnError(string errorCode, Action handler)
        {
            _errorHandlers[errorCode] = () => { handler(); return UniTask.CompletedTask; };
            return this;
        }

        public ResultHandler<T> OnError(string errorCode, Func<UniTask> handler)
        {
            _errorHandlers[errorCode] = handler;
            return this;
        }

        public ResultHandler<T> OnSuccess(Action<T> handler)
        {
            _successHandler = value => { handler(value); return UniTask.CompletedTask; };
            return this;
        }

        public ResultHandler<T> OnSuccess(Func<T, UniTask> handler)
        {
            _successHandler = handler;
            return this;
        }

        public ResultHandler<T> ToastOnError()
        {
            _errorMode = ErrorMode.Toast;
            return this;
        }

        public ResultHandler<T> PopupOnError()
        {
            _errorMode = ErrorMode.Popup;
            return this;
        }

        public ResultHandler<T> PromptRetryOnError()
        {
            if (!_hasFactory)
                throw new InvalidOperationException("PromptRetryOnError requires Handle(factory) overload — a one-shot UniTask cannot be re-executed.");

            _errorMode = ErrorMode.RetryPrompt;
            return this;
        }

        public ResultHandler<T> WithLoader()
        {
            _withLoader = true;
            return this;
        }

        public async UniTask<bool> ExecuteAsync(CancellationToken ct = default)
        {
            using IDisposable loaderScope = _withLoader && _loaderService != null
                ? _loaderService.ShowScope()
                : null;

            if (_errorMode == ErrorMode.RetryPrompt)
                return await ExecuteWithRetryPromptAsync(ct);

            Result<T> result = _hasFactory ? await _factory(ct) : await _task;
            
            return await HandleResultAsync(result, ct);
        }

        private async UniTask<bool> ExecuteWithRetryPromptAsync(CancellationToken ct)
        {
            while (true)
            {
                Result<T> result = await _factory(ct);

                if (result.IsSuccess)
                {
                    if (_successHandler != null)
                        await _successHandler(result.Value);

                    return true;
                }

                if (result.Error.Underlying != null)
                    Log.Exception(result.Error.Underlying);

                if (_errorHandlers.TryGetValue(result.Error.Code, out Func<UniTask> handler))
                {
                    await handler();

                    return false;
                }

                ErrorAction action = await _notifications.ShowPopupAsync(
                    result.Error.Message,
                    ErrorAction.Retry | ErrorAction.Skip,
                    ct);

                if (action == ErrorAction.Retry)
                    continue;

                return false;
            }
        }

        private async UniTask<bool> HandleResultAsync(Result<T> result, CancellationToken ct)
        {
            if (result.IsSuccess)
            {
                if (_successHandler != null)
                    await _successHandler(result.Value);
                
                return true;
            }

            if (result.Error.Underlying != null)
                Log.Exception(result.Error.Underlying);

            if (_errorHandlers.TryGetValue(result.Error.Code, out Func<UniTask> handler))
            {
                await handler();
                
                return false;
            }

            switch (_errorMode)
            {
                case ErrorMode.Toast:
                    _notifications.ShowError(result.Error);
                    break;
                case ErrorMode.Popup:
                    await _notifications.ShowPopupAsync(result.Error.Message, ErrorAction.Skip, ct);
                    break;
            }

            return false;
        }
    }
}
