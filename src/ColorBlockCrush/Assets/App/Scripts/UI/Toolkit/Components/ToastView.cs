using System;
using System.Threading;
using App.Scripts.UI.Constants;
using App.Scripts.Core.Enums;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Components
{
    public sealed class ToastView : IDisposable
    {
        private const string HIDDEN_CLASS = "toast--hidden";
        private const string VISIBLE_CLASS = "toast--visible";
        private const string INFO_CLASS = "toast--info";
        private const string SUCCESS_CLASS = "toast--success";
        private const string WARNING_CLASS = "toast--warning";
        private const string ERROR_CLASS = "toast--error";

        private static readonly string[] TypeClasses =
        {
            INFO_CLASS, SUCCESS_CLASS, WARNING_CLASS, ERROR_CLASS
        };

        private readonly VisualElement _root;
        private readonly Label _messageLabel;
        private CancellationTokenSource _hideCts;

        public ToastView(VisualElement toastContainer)
        {
            _root = toastContainer;
            _messageLabel = _root.Q<Label>(UxmlNames.Toast.MESSAGE);

            _root.AddToClassList(HIDDEN_CLASS);
            _root.style.display = DisplayStyle.None;
        }

        public void Show(string message, ToastType type, float duration)
        {
            _hideCts?.Cancel();
            _hideCts?.Dispose();
            _hideCts = new CancellationTokenSource();

            _messageLabel.text = message;

            foreach (string typeClass in TypeClasses)
                _root.RemoveFromClassList(typeClass);

            _root.AddToClassList(TypeClasses[(int)type]);

            _root.style.display = DisplayStyle.Flex;
            _root.RemoveFromClassList(HIDDEN_CLASS);
            _root.AddToClassList(VISIBLE_CLASS);

            HideAfterDelay(duration, _hideCts.Token).Forget();
        }

        private async UniTask HideAfterDelay(float duration, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
            Hide();
        }

        private void Hide()
        {
            _root.RemoveFromClassList(VISIBLE_CLASS);
            _root.AddToClassList(HIDDEN_CLASS);
            _root.style.display = DisplayStyle.None;
        }

        public void Dispose()
        {
            _hideCts?.Cancel();
            _hideCts?.Dispose();
            _hideCts = null;
        }
    }
}
