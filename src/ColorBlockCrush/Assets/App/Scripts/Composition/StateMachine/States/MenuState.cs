using System.Threading;
using App.Scripts.UI.Services.Loader;
using App.Scripts.UI.Services.UIService;
using App.Scripts.UI.Toolkit.Screens;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Composition.StateMachine.States
{
    public class MenuState : IState, IEnterableAsyncState
    {
        private readonly IUIService _uiService;
        private readonly ILoaderService _loaderService;

        public MenuState(IUIService uiService, ILoaderService loaderService)
        {
            _uiService = uiService;
            _loaderService = loaderService;
        }

        public async UniTask<Next> EnterAsync(CancellationToken ct = default)
        {
            _loaderService.Hide();

            await _uiService.ShowScreenAsync<MenuScreen>(ct: ct);

            return Next.None;
        }
    }
}
