using System.Threading;
using App.Scripts.AppFlow;
using App.Scripts.Composition.StateMachine;
using App.Scripts.Composition.StateMachine.States;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Composition.Flow
{
    public sealed class AppFlowService : IAppFlowService
    {
        private readonly AppStateMachine _stateMachine;
        private bool _booted;

        public AppFlowService(AppStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public UniTask BootAsync(CancellationToken ct = default)
        {
            if (_booted)
                return UniTask.CompletedTask;

            _booted = true;
            return _stateMachine.SwitchStateAsync<CreateUiState>(ct);
        }

        public UniTask ToMenuAsync(CancellationToken ct = default)
        {
            return _stateMachine.SwitchStateAsync<FromGameToMenuState>(ct);
        }

        public UniTask ToGameAsync(CancellationToken ct = default)
        {
            return _stateMachine.SwitchStateAsync<FromMenuToGameState>(ct);
        }
    }
}
