using System;
using System.Threading;
using App.Scripts.Core.Constants;
using App.Scripts.Core.Enums;
using App.Scripts.UI.Constants;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Components
{
    public sealed class ErrorView : IDisposable
    {
        private const string HIDDEN_CLASS = "error--hidden";
        private const string VISIBLE_CLASS = "error--visible";

        private readonly VisualElement _root;
        private readonly Label _messageLabel;
        private readonly Button _retryButton;
        private readonly Button _skipButton;

        private UniTaskCompletionSource<ErrorAction> _tcs;
        private CancellationTokenRegistration _ctRegistration;

        public ErrorView(VisualElement errorContainer)
        {
            _root = errorContainer;
            _messageLabel = _root.Q<Label>(UxmlNames.Error.MESSAGE);
            _retryButton = _root.Q<Button>(UxmlNames.Error.RETRY_BUTTON);
            _skipButton = _root.Q<Button>(UxmlNames.Error.SKIP_BUTTON);

            _retryButton.text = "Retry";
            _skipButton.text = "Skip";

            _retryButton.clicked += OnRetryClicked;
            _skipButton.clicked += OnSkipClicked;

            _root.AddToClassList(HIDDEN_CLASS);
            _root.style.display = DisplayStyle.None;
        }

        public UniTask<ErrorAction> ShowAsync(string message, ErrorAction availableActions, CancellationToken ct)
        {
            EndActiveSession();

            UniTaskCompletionSource<ErrorAction> tcs = new();
            _tcs = tcs;

            _messageLabel.text = message;

            bool showRetry = (availableActions & ErrorAction.Retry) != 0;
            bool showSkip = (availableActions & ErrorAction.Skip) != 0;

            _retryButton.style.display = showRetry ? DisplayStyle.Flex : DisplayStyle.None;
            _skipButton.style.display = showSkip ? DisplayStyle.Flex : DisplayStyle.None;

            _root.style.display = DisplayStyle.Flex;
            _root.RemoveFromClassList(HIDDEN_CLASS);
            _root.AddToClassList(VISIBLE_CLASS);

            _ctRegistration = ct.Register(() =>
            {
                tcs.TrySetCanceled();
                Hide();
            });

            return tcs.Task;
        }

        public void Dispose()
        {
            EndActiveSession();
            Hide();
            _retryButton.clicked -= OnRetryClicked;
            _skipButton.clicked -= OnSkipClicked;
        }

        private void OnRetryClicked() => CompleteSession(ErrorAction.Retry);

        private void OnSkipClicked() => CompleteSession(ErrorAction.Skip);

        private void CompleteSession(ErrorAction action)
        {
            _ctRegistration.Dispose();
            _ctRegistration = default;

            UniTaskCompletionSource<ErrorAction> tcs = _tcs;
            _tcs = null;

            Hide();
            tcs?.TrySetResult(action);
        }

        private void EndActiveSession()
        {
            _ctRegistration.Dispose();
            _ctRegistration = default;
            _tcs?.TrySetCanceled();
            _tcs = null;
        }

        private void Hide()
        {
            _root.RemoveFromClassList(VISIBLE_CLASS);
            _root.AddToClassList(HIDDEN_CLASS);
            _root.style.display = DisplayStyle.None;
        }
    }
}