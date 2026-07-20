using System;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Services.Loader
{
    public interface ILoaderService
    {
        bool IsVisible { get; }
        int ActiveCount { get; }

        void Initialize(VisualElement loaderContainer);

        IDisposable ShowScope();

        void Show();
        void Hide();
    }
}