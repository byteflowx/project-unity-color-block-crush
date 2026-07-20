using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace App.Scripts.Composition.StateMachine
{
    public interface IState { }

    public interface IEnterableAsyncState
    {
        UniTask<Next> EnterAsync(CancellationToken ct = default);
    }

    public interface IExitableAsyncState
    {
        UniTask ExitAsync(CancellationToken ct = default);
    }

    public readonly struct Next
    {
        public readonly Type StateType;

        private Next(Type stateType) => StateType = stateType;

        public static Next To<TState>() where TState : IState => new(typeof(TState));

        public static readonly Next None = default;
    }
}