using System;
using System.Collections.Generic;
using System.Threading;
using App.Scripts.Core.Common;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Composition.StateMachine
{
    public sealed class AppStateMachine : IDisposable
    {
        private readonly Dictionary<Type, IState> _states;
        private readonly CancellationTokenSource _lifetimeCts = new();

        private IState _currentState;
        private bool _isRunning;

        public AppStateMachine(IReadOnlyList<IState> states)
        {
            _states = new Dictionary<Type, IState>(states.Count);

            foreach (IState state in states)
                _states[state.GetType()] = state;
        }

        public async UniTask SwitchStateAsync<TState>(CancellationToken ct = default) where TState : IState
        {
            ct.ThrowIfCancellationRequested();

            if (_isRunning)
            {
                Log.Warning($"Switch to {typeof(TState).Name} ignored — a transition is already in progress");
                return;
            }

            _isRunning = true;

            try
            {
                Type next = typeof(TState);

                while (next != null)
                {
                    if (!_states.TryGetValue(next, out IState state))
                    {
                        Log.Error($"State {next.Name} is not registered!");
                        return;
                    }

                    if (_currentState is IExitableAsyncState exitable)
                        await exitable.ExitAsync(_lifetimeCts.Token);

                    _currentState = state;

                    Log.Info($"Enter {state.GetType().Name}");

                    next = state is IEnterableAsyncState enterable
                        ? (await enterable.EnterAsync(_lifetimeCts.Token)).StateType
                        : null;
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
        }
    }
}
