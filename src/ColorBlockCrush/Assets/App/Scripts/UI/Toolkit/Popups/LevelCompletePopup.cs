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
    public sealed class LevelCompletePopup : UIPopup
    {
        private readonly IGameSessionService _session;
        private readonly IUIService _uiService;

        private RoundedButton _nextButton;

        public override bool CloseOnOverlayClick => false;

        public LevelCompletePopup(IGameSessionService session, IUIService uiService)
        {
            _session = session;
            _uiService = uiService;
        }

        protected override void SetVisualElements()
        {
            _nextButton = Root.Q<RoundedButton>(UxmlNames.LevelComplete.NEXT_BUTTON);
        }

        protected override void RegisterCallbacks()
        {
            Events.RegisterCallback<ClickEvent>(_nextButton, OnNextClicked);
        }

        protected override void OnShow()
        {
            _nextButton.SetEnabled(true);
        }

        private void OnNextClicked()
        {
            _nextButton.SetEnabled(false);
            _uiService.ClosePopup<LevelCompletePopup>();
            _session.NextLevelAsync().Forget(Log.Exception);
        }
    }
}
