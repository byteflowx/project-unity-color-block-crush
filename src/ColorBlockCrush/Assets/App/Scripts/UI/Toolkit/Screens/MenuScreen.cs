using App.Scripts.AppFlow;
using App.Scripts.Core.Common;
using App.Scripts.UI.Constants;
using App.Scripts.UI.Services.UIService;
using App.Scripts.UI.Toolkit.Base;
using App.Scripts.UI.Toolkit.Controls;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Screens
{
    public sealed class MenuScreen : UIScreen
    {
        private readonly IAppFlowService _appFlow;
        private RoundedButton _playButton;

        public MenuScreen(IAppFlowService appFlow)
        {
            _appFlow = appFlow;
        }

        protected override void SetVisualElements()
        {
            _playButton = Root.Q<RoundedButton>(UxmlNames.Menu.PLAY_BUTTON);
        }

        protected override void RegisterCallbacks()
        {
            Events.RegisterCallback<ClickEvent>(_playButton, OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            _appFlow.ToGameAsync().Forget(Log.Exception);
        }
    }
}
