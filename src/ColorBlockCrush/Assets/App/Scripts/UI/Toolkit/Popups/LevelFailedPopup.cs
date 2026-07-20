using App.Scripts.AppFlow;
using App.Scripts.Core.Common;
using App.Scripts.UI.Constants;
using App.Scripts.UI.Services.UIService;
using App.Scripts.UI.Toolkit.Base;
using App.Scripts.UI.Toolkit.Controls;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Popups
{
    public sealed class LevelFailedPopup : UIPopup
    {
        private readonly IGameSessionService _session;
        private readonly IUIService _uiService;

        private RoundedButton _retryButton;

        public override bool CloseOnOverlayClick => false;

        public LevelFailedPopup(IGameSessionService session, IUIService uiService)
        {
            _session = session;
            _uiService = uiService;
        }

        protected override void SetVisualElements()
        {
            _retryButton = Root.Q<RoundedButton>(UxmlNames.LevelFailed.RETRY_BUTTON);
        }

        protected override void RegisterCallbacks()
        {
            Events.RegisterCallback<ClickEvent>(_retryButton, OnRetryClicked);
        }

        protected override void OnShow()
        {
            _retryButton.SetEnabled(true);
        }

        private void OnRetryClicked()
        {
            _retryButton.SetEnabled(false);
            _uiService.ClosePopup<LevelFailedPopup>();
            _session.RestartAsync().Forget(Log.Exception);
        }
    }
}
