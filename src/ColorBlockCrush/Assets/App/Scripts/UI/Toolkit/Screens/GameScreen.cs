using App.Scripts.AppFlow;
using App.Scripts.Core.Common;
using App.Scripts.UI.Constants;
using App.Scripts.UI.Toolkit.Base;
using App.Scripts.UI.Toolkit.Controls;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Screens
{
    public sealed class GameScreen : UIScreen
    {
        private readonly IGameSessionService _session;

        private DisposableBag _disposables;
        private Label _levelLabel;
        private RoundedButton _restartButton;

        public override bool ShowsBackgroundOverlay => false;

        public GameScreen(IGameSessionService session)
        {
            _session = session;
        }

        protected override void SetVisualElements()
        {
            ScreenHeader header = Root.Q<ScreenHeader>(UxmlNames.Game.HEADER);
            _levelLabel = Root.Q<Label>(UxmlNames.Game.LEVEL_LABEL);
            _restartButton = Root.Q<RoundedButton>(UxmlNames.Game.RESTART_BUTTON);

            header.LeftContainer.Add(_levelLabel);
            header.RightContainer.Add(_restartButton);
        }

        protected override void RegisterCallbacks()
        {
            Events.RegisterCallback<ClickEvent>(_restartButton, OnRestartClicked);

            _session.LevelId.Subscribe(this, static (value, self) => self.SetLevel(value)).AddTo(ref _disposables);
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            base.Dispose();
        }

        private void SetLevel(int levelId)
        {
            _levelLabel.text = $"LEVEL {levelId}";
        }

        private void OnRestartClicked()
        {
            _session.RestartAsync().Forget(Log.Exception);
        }
    }
}
