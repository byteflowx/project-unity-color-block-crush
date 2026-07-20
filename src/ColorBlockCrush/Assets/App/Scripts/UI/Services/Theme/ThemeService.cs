using System;
using App.Scripts.Core.Enums;
using App.Scripts.UI.Orientation;
using App.Scripts.UI.Services.UIService;
using App.Scripts.UI.StaticData;
using R3;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.Theme
{
    public sealed class ThemeService : IThemeService, IDisposable
    {
        private const string CLASS_PORTRAIT = "orientation-portrait";
        private const string CLASS_LANDSCAPE = "orientation-landscape";

        private readonly IUIService _uiService;
        private readonly LayoutOrientationComponent _orientationService;
        private DisposableBag _disposables;

        public ThemeService(IUIService uiService, LayoutOrientationComponent orientationService)
        {
            _uiService = uiService;
            _orientationService = orientationService;
        }

        public void Initialize()
        {
            _orientationService.CurrentOrientation
                .Subscribe(ApplyTheme)
                .AddTo(ref _disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void ApplyTheme(LayoutOrientation orientation)
        {
            UIDocument uiDocument = _uiService.UIDocument;

            if (uiDocument == null)
                return;

            VisualElement root = uiDocument.rootVisualElement;

            if (root == null)
                return;

            bool isPortrait = orientation == LayoutOrientation.Portrait;
            root.EnableInClassList(CLASS_PORTRAIT, isPortrait);
            root.EnableInClassList(CLASS_LANDSCAPE, !isPortrait);

            ThemeSettings settings = _uiService.ThemeSettings;

            if (settings == null)
                return;

            LayoutOrientation otherOrientation = isPortrait ? LayoutOrientation.Landscape : LayoutOrientation.Portrait;
            ThemeSettings.OrientationTheme target = settings.Resolve(orientation);
            ThemeSettings.OrientationTheme other = settings.Resolve(otherOrientation);

            SwapThemeStyleSheet(root, other.ThemeStyleSheet, target.ThemeStyleSheet);

            PanelSettings panelSettings = uiDocument.panelSettings;

            if (panelSettings == null)
                return;

            panelSettings.referenceResolution = target.ReferenceResolution;
            panelSettings.match = target.Match;
        }

        private static void SwapThemeStyleSheet(VisualElement root, ThemeStyleSheet toRemove, ThemeStyleSheet toAdd)
        {
            if (toRemove != null && root.styleSheets.Contains(toRemove))
                root.styleSheets.Remove(toRemove);

            if (toAdd != null && !root.styleSheets.Contains(toAdd))
                root.styleSheets.Add(toAdd);
        }
    }
}
