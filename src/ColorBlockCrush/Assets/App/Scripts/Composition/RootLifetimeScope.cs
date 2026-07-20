using System;
using App.Scripts.AppFlow;
using App.Scripts.Composition.Bootstrap;
using App.Scripts.Composition.Flow;
using App.Scripts.Composition.StateMachine;
using App.Scripts.Composition.StateMachine.States;
using App.Scripts.Engine.Addressable;
using App.Scripts.Engine.Audio;
using App.Scripts.Engine.Haptics;
using App.Scripts.Engine.SceneLoader;
using App.Scripts.Engine.Time;
using App.Scripts.Gameplay;
using App.Scripts.Gameplay.Levels;
using App.Scripts.UI.Factory;
using App.Scripts.UI.Orientation;
using App.Scripts.UI.Services.Loader;
using App.Scripts.UI.Services.Notification;
using App.Scripts.UI.Services.Theme;
using App.Scripts.UI.Services.UIService;
using App.Scripts.UI.Toolkit.Base;
using App.Scripts.UI.Toolkit.Popups;
using App.Scripts.UI.Toolkit.Screens;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace App.Scripts.Composition
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private LayoutOrientationComponent _layoutOrientation;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_uiDocument);
            builder.RegisterComponent(_layoutOrientation);

            RegisterEngine(builder);
            RegisterDomain(builder);
            RegisterUi(builder);
            RegisterAppFlow(builder);

            builder.RegisterEntryPoint<AppBootstrap>();
        }

        private static void RegisterEngine(IContainerBuilder builder)
        {
            builder.Register<AddressableService>(Lifetime.Singleton).As<IAddressableService>();
            builder.Register<UnityTimeService>(Lifetime.Singleton).As<IUnityTimeService>();
            builder.Register<HapticsService>(Lifetime.Singleton).As<IHapticsService>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoaderService>();
        }

        private static void RegisterDomain(IContainerBuilder builder)
        {
            builder.Register<LevelService>(Lifetime.Singleton).As<ILevelService>();
            builder.Register<GameSessionService>(Lifetime.Singleton).As<IGameSessionService>().AsSelf();
        }

        private static void RegisterUi(IContainerBuilder builder)
        {
            builder.RegisterFactory<Type, UIView>(
                container => type => (UIView)container.Resolve(type),
                Lifetime.Singleton);

            builder.Register<UIViewFactory>(Lifetime.Singleton).As<IUIViewFactory>();
            builder.Register<LoaderService>(Lifetime.Singleton).As<ILoaderService>();
            builder.Register<NotificationService>(Lifetime.Singleton).As<INotificationService>();
            builder.Register<UIService>(Lifetime.Singleton).As<IUIService>();
            builder.Register<ThemeService>(Lifetime.Singleton).As<IThemeService>();

            builder.Register<MenuScreen>(Lifetime.Transient);
            builder.Register<GameScreen>(Lifetime.Transient);
            builder.Register<LevelCompletePopup>(Lifetime.Transient);
            builder.Register<LevelFailedPopup>(Lifetime.Transient);
        }

        private static void RegisterAppFlow(IContainerBuilder builder)
        {
            builder.Register<AppStateMachine>(Lifetime.Singleton).AsSelf();
            builder.Register<AppFlowService>(Lifetime.Singleton).As<IAppFlowService>();

            builder.Register<CreateUiState>(Lifetime.Singleton).As<IState>();
            builder.Register<FromBootToMenuState>(Lifetime.Singleton).As<IState>();
            builder.Register<FromMenuToGameState>(Lifetime.Singleton).As<IState>();
            builder.Register<FromGameToMenuState>(Lifetime.Singleton).As<IState>();
            builder.Register<GameState>(Lifetime.Singleton).As<IState>();
            builder.Register<MenuState>(Lifetime.Singleton).As<IState>();
        }
    }
}
