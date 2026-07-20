using System.Threading;
using App.Scripts.Core.Constants;
using App.Scripts.Engine.SceneLoader;
using App.Scripts.UI.Services.UIService;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Composition.StateMachine.States
{
    public class FromMenuToGameState : IState, IEnterableAsyncState
    {
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IUIService _uiService;

        public FromMenuToGameState(ISceneLoaderService sceneLoader, IUIService uiService)
        {
            _sceneLoader = sceneLoader;
            _uiService = uiService;
        }

        public async UniTask<Next> EnterAsync(CancellationToken ct = default)
        {
            _uiService.CloseAll();

            await _sceneLoader.LoadAsync(SceneName.GAMEPLAY_SCENE_NAME, ct);

            return Next.To<GameState>();
        }
    }
}
