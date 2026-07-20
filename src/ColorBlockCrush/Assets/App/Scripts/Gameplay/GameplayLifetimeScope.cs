using App.Scripts.Gameplay.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace App.Scripts.Gameplay
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayPresenter _presenter;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_presenter);
        }
    }
}
