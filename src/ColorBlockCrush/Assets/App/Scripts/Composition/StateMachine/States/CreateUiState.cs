using System.Threading;
using App.Scripts.UI.Constants;
using App.Scripts.UI.Services.Loader;
using App.Scripts.UI.Services.Notification;
using App.Scripts.UI.Services.Theme;
using App.Scripts.UI.Services.UIService;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace App.Scripts.Composition.StateMachine.States
{
    public class CreateUiState : IState, IEnterableAsyncState
    {
        private readonly IUIService _uiService;
        private readonly ILoaderService _loaderService;
        private readonly INotificationService _notificationService;
        private readonly IThemeService _themeService;

        public CreateUiState(
            IUIService uiService,
            ILoaderService loaderService,
            INotificationService notificationService,
            IThemeService themeService)
        {
            _uiService = uiService;
            _loaderService = loaderService;
            _notificationService = notificationService;
            _themeService = themeService;
        }

        public async UniTask<Next> EnterAsync(CancellationToken ct = default)
        {
            await _uiService.InitializeAsync(ct);
            _themeService.Initialize();

            VisualElement loaderContainer = _uiService.Root.Q<VisualElement>(UxmlNames.Containers.LOADER);
            _loaderService.Initialize(loaderContainer);

            VisualElement errorContainer = _uiService.Root.Q<VisualElement>(UxmlNames.Containers.ERROR);
            VisualElement toastContainer = _uiService.Root.Q<VisualElement>(UxmlNames.Containers.TOAST);
            _notificationService.Initialize(toastContainer, errorContainer);

            _loaderService.Show();

            return Next.To<FromBootToMenuState>();
        }
    }
}
