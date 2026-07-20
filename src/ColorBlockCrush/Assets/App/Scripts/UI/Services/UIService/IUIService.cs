using System.Threading;
using App.Scripts.UI.StaticData;
using App.Scripts.UI.Toolkit.Base;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.UIService
{
    public interface IUIService
    {
        UIDocument UIDocument { get; }
        ThemeSettings ThemeSettings { get; }
        VisualElement Root { get; }
        VisualElement ScreensContainer { get; }
        VisualElement PopupsContainer { get; }
        VisualElement ScreenOverlay { get; }

        ReadOnlyReactiveProperty<UIScreen> CurrentScreen { get; }
        bool HasOpenPopups { get; }

        UniTask InitializeAsync(CancellationToken ct);
        void SetScreenOverlayClass(string className);

        UniTask ShowScreenAsync<TScreen>(bool addToHistory = true, CancellationToken ct = default) where TScreen : UIScreen;
        void CloseCurrentScreen();
        void Back();
        void ClearHistory();

        UniTask ShowPopupAsync<TPopup>(CancellationToken ct = default) where TPopup : UIPopup;
        void CloseTopPopup();
        void CloseAllPopups();
        void ClosePopup<TPopup>() where TPopup : UIPopup;
        void CloseAll();
    }
}