using System;
using System.Collections.Generic;
using System.Threading;
using App.Scripts.Core.Common;
using App.Scripts.Core.Constants;
using App.Scripts.Engine.Addressable;
using App.Scripts.UI.Constants;
using App.Scripts.UI.Factory;
using App.Scripts.UI.StaticData;
using App.Scripts.UI.Toolkit.Base;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.UIService
{
    public sealed class UIService : IUIService, IDisposable
    {
        private const string POPUP_OVERLAY_VISIBLE_CLASS = "popup-overlay--visible";
        private const string SCREEN_OVERLAY_VISIBLE_CLASS = "screen-overlay--visible";

        private readonly IAddressableService _addressableService;
        private readonly IUIViewFactory _uiViewFactory;

        private UIAssetsConfig _assetsConfig;
        private string _currentOverlayClass;
        private VisualElement _popupOverlay;

        private readonly Dictionary<Type, UIScreen> _screens = new();
        private readonly Dictionary<Type, UIPopup> _popups = new();
        private readonly Stack<UIScreen> _screenHistory = new();
        private readonly Stack<UIPopup> _popupStack = new();
        private readonly ReactiveProperty<UIScreen> _currentScreen = new(null);

        private bool _isShowingScreen;
        private bool _isShowingPopup;

        public UIDocument UIDocument { get; }
        public ThemeSettings ThemeSettings { get; private set; }
        public VisualElement Root { get; private set; }
        public VisualElement ScreensContainer { get; private set; }
        public VisualElement PopupsContainer { get; private set; }
        public VisualElement ScreenOverlay { get; private set; }

        public ReadOnlyReactiveProperty<UIScreen> CurrentScreen => _currentScreen;
        public bool HasOpenPopups => _popupStack.Count > 0;

        public UIService(UIDocument uiDocument, IAddressableService addressableService, IUIViewFactory uiViewFactory)
        {
            UIDocument = uiDocument;
            _addressableService = addressableService;
            _uiViewFactory = uiViewFactory;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _assetsConfig = await _addressableService.Load<UIAssetsConfig>(AddressableConstants.UI_ASSETS_CONFIG, ct);
            _assetsConfig.Initialize();

            ThemeSettings = _assetsConfig.ThemeSettings;

            VisualTreeAsset mainLayoutAsset = await _addressableService.Load<VisualTreeAsset>(_assetsConfig.MainLayout, ct);

            Root = UIDocument.rootVisualElement;
            mainLayoutAsset.CloneTree(Root);

            CacheContainerReferences();
            SetupPopupOverlay();
        }

        public void SetScreenOverlayClass(string className)
        {
            if (ScreenOverlay == null)
                return;

            if (!string.IsNullOrEmpty(_currentOverlayClass))
                ScreenOverlay.RemoveFromClassList(_currentOverlayClass);

            if (!string.IsNullOrEmpty(className))
                ScreenOverlay.AddToClassList(className);

            _currentOverlayClass = className;
        }

        public async UniTask ShowScreenAsync<TScreen>(bool addToHistory = true, CancellationToken ct = default) where TScreen : UIScreen
        {
            if (_isShowingScreen)
            {
                Log.Warning($"ShowScreenAsync<{typeof(TScreen).Name}> ignored — another screen transition is in progress");
                return;
            }

            _isShowingScreen = true;
            try
            {
                Type screenType = typeof(TScreen);

                if (!_screens.TryGetValue(screenType, out UIScreen screen))
                {
                    screen = await LoadScreenAsync<TScreen>(ct);
                    _screens[screenType] = screen;
                }

                UIScreen previous = _currentScreen.CurrentValue;
                if (previous != null)
                {
                    if (addToHistory)
                        _screenHistory.Push(previous);

                    previous.Hide();
                }

                if (screen.ShowsBackgroundOverlay)
                    ScreenOverlay?.AddToClassList(SCREEN_OVERLAY_VISIBLE_CLASS);
                else
                    ScreenOverlay?.RemoveFromClassList(SCREEN_OVERLAY_VISIBLE_CLASS);

                screen.Show();
                _currentScreen.Value = screen;
            }
            finally
            {
                _isShowingScreen = false;
            }
        }

        public void CloseCurrentScreen()
        {
            UIScreen current = _currentScreen.CurrentValue;
            if (current != null)
            {
                current.HideImmediately();
                _currentScreen.Value = null;
            }

            SetScreenOverlayClass(null);
            ScreenOverlay?.RemoveFromClassList(SCREEN_OVERLAY_VISIBLE_CLASS);
        }

        public void CloseAll()
        {
            CloseAllPopups();
            CloseCurrentScreen();
            ClearHistory();
        }

        public void Back()
        {
            if (_screenHistory.Count == 0)
                return;

            _currentScreen.CurrentValue?.Hide();
            UIScreen previous = _screenHistory.Pop();
            previous.Show();
            _currentScreen.Value = previous;
        }

        public void ClearHistory()
        {
            _screenHistory.Clear();
        }

        public async UniTask ShowPopupAsync<TPopup>(CancellationToken ct = default) where TPopup : UIPopup
        {
            if (_isShowingPopup)
            {
                Log.Warning($"ShowPopupAsync<{typeof(TPopup).Name}> ignored — another popup transition is in progress");
                return;
            }

            _isShowingPopup = true;
            try
            {
                Type popupType = typeof(TPopup);

                if (!_popups.TryGetValue(popupType, out UIPopup popup))
                {
                    popup = await LoadPopupAsync<TPopup>(ct);
                    _popups[popupType] = popup;
                }

                if (_popupStack.Contains(popup))
                    return;

                if (popup.ShowOverlay)
                    ShowOverlay();

                _popupStack.Push(popup);
                popup.Show();
            }
            finally
            {
                _isShowingPopup = false;
            }
        }

        public void CloseTopPopup()
        {
            if (_popupStack.Count == 0)
                return;

            UIPopup popup = _popupStack.Pop();
            popup.Hide();

            UpdateOverlayVisibility();
        }

        public void CloseAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                UIPopup popup = _popupStack.Pop();
                popup.HideImmediately();
            }

            HideOverlay();
        }

        public void ClosePopup<TPopup>() where TPopup : UIPopup
        {
            Type popupType = typeof(TPopup);

            if (!_popups.TryGetValue(popupType, out UIPopup popup))
                return;

            if (!_popupStack.Contains(popup))
                return;

            Stack<UIPopup> tempStack = new();

            while (_popupStack.Count > 0)
            {
                UIPopup current = _popupStack.Pop();

                if (current == popup)
                {
                    current.Hide();
                    break;
                }
                tempStack.Push(current);
            }

            while (tempStack.Count > 0)
                _popupStack.Push(tempStack.Pop());

            UpdateOverlayVisibility();
        }

        private async UniTask<TScreen> LoadScreenAsync<TScreen>(CancellationToken ct) where TScreen : UIScreen
        {
            AssetReference assetReference = _assetsConfig.GetScreen<TScreen>();
            VisualTreeAsset visualTreeAsset = await _addressableService.Load<VisualTreeAsset>(assetReference, ct);

            visualTreeAsset.CloneTree(ScreensContainer, out int firstIndex, out _);
            VisualElement screenRoot = ScreensContainer[firstIndex];

            return _uiViewFactory.Create<TScreen>(screenRoot);
        }

        private async UniTask<TPopup> LoadPopupAsync<TPopup>(CancellationToken ct) where TPopup : UIPopup
        {
            AssetReference assetReference = _assetsConfig.GetPopup<TPopup>();
            VisualTreeAsset visualTreeAsset = await _addressableService.Load<VisualTreeAsset>(assetReference, ct);

            visualTreeAsset.CloneTree(PopupsContainer, out int firstIndex, out _);
            VisualElement popupRoot = PopupsContainer[firstIndex];

            return _uiViewFactory.Create<TPopup>(popupRoot);
        }

        private void CacheContainerReferences()
        {
            ScreensContainer = Root.Q<VisualElement>(UxmlNames.Containers.SCREENS_CONTENT);
            PopupsContainer = Root.Q<VisualElement>(UxmlNames.Containers.POPUPS_CONTENT);
            ScreenOverlay = Root.Q<VisualElement>(UxmlNames.Containers.SCREEN_OVERLAY);
        }

        private void SetupPopupOverlay()
        {
            _popupOverlay = Root.Q<VisualElement>(UxmlNames.Containers.POPUP_OVERLAY);

            if (_popupOverlay != null)
                _popupOverlay.RegisterCallback<ClickEvent>(OnOverlayClicked);
        }

        private void OnOverlayClicked(ClickEvent evt)
        {
            if (_popupStack.Count == 0)
                return;

            UIPopup topPopup = _popupStack.Peek();
            if (topPopup.CloseOnOverlayClick)
                CloseTopPopup();
        }

        private void ShowOverlay()
        {
            _popupOverlay?.AddToClassList(POPUP_OVERLAY_VISIBLE_CLASS);
        }

        private void HideOverlay()
        {
            _popupOverlay?.RemoveFromClassList(POPUP_OVERLAY_VISIBLE_CLASS);
        }

        private void UpdateOverlayVisibility()
        {
            if (_popupStack.Count == 0)
            {
                HideOverlay();
                return;
            }

            bool hasPopupWithOverlay = false;

            foreach (UIPopup popup in _popupStack)
            {
                if (popup.ShowOverlay)
                {
                    hasPopupWithOverlay = true;
                    break;
                }
            }

            if (hasPopupWithOverlay)
                ShowOverlay();
            else
                HideOverlay();
        }

        public void Dispose()
        {
            if (_popupOverlay != null)
                _popupOverlay.UnregisterCallback<ClickEvent>(OnOverlayClicked);

            foreach (UIScreen screen in _screens.Values)
                screen.Dispose();

            foreach (UIPopup popup in _popups.Values)
                popup.Dispose();

            _screens.Clear();
            _popups.Clear();
            _screenHistory.Clear();
            _popupStack.Clear();
            _currentOverlayClass = null;
            _currentScreen.Dispose();
        }
    }
}
