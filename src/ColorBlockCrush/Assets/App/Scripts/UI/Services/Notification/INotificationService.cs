using System;
using System.Threading;
using App.Scripts.Core.Common;
using App.Scripts.Core.Enums;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.Notification
{
    public interface INotificationService
    {
        void Initialize(VisualElement toastContainer, VisualElement errorContainer);

        void ShowToast(string key, ToastType type = ToastType.Info, float duration = 3f);

        UniTask<ErrorAction> ShowPopupAsync(string key, ErrorAction actions, CancellationToken ct = default);

        void ShowError(ServiceError error);

        UniTask<bool> TryAsync<T>(
            UniTask<Result<T>> task,
            Action<T> onSuccess = null,
            CancellationToken ct = default);

        ResultHandler<T> Handle<T>(UniTask<Result<T>> task);

        ResultHandler<T> Handle<T>(Func<CancellationToken, UniTask<Result<T>>> factory);
    }
}
