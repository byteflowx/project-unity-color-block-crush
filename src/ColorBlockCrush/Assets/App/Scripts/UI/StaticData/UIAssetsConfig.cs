using System;
using System.Collections.Generic;
using System.Linq;
using App.Scripts.Core.Enums;
using App.Scripts.Engine.AssetReferences;
using App.Scripts.UI.Toolkit.Base;
using UnityEngine;

namespace App.Scripts.UI.StaticData
{
    [CreateAssetMenu(fileName = "UIAssetsConfig", menuName = "App/UI Assets Config")]
    public sealed class UIAssetsConfig : ScriptableObject
    {
        [Header("Infrastructure")]
        [SerializeField] private ThemeSettings _themeSettings;

        [Header("Main Layout")]
        [SerializeField] private AssetReferenceVisualTreeAsset _mainLayout;

        [Header("Screens")]
        [SerializeField] private List<ScreenEntry> _screens;

        [Header("Popups")]
        [SerializeField] private List<PopupEntry> _popups;

        [Serializable]
        public sealed class ScreenEntry
        {
            public ScreenId Id;
            public AssetReferenceVisualTreeAsset Asset;
        }

        [Serializable]
        public sealed class PopupEntry
        {
            public PopupId Id;
            public AssetReferenceVisualTreeAsset Asset;
        }

        public ThemeSettings ThemeSettings => _themeSettings;
        public AssetReferenceVisualTreeAsset MainLayout => _mainLayout;
        public IReadOnlyList<ScreenEntry> Screens => _screens;
        public IReadOnlyList<PopupEntry> Popups => _popups;

        private Dictionary<ScreenId, AssetReferenceVisualTreeAsset> _screenCache;
        private Dictionary<PopupId, AssetReferenceVisualTreeAsset> _popupCache;
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized && _screenCache != null && _popupCache != null)
                return;

            _screenCache = _screens.ToDictionary(x => x.Id, x => x.Asset);
            _popupCache = _popups.ToDictionary(x => x.Id, x => x.Asset);
            _isInitialized = true;
        }

        public AssetReferenceVisualTreeAsset GetScreen(ScreenId id)
        {
            ThrowIfNotInitialized();

            if (!_screenCache.TryGetValue(id, out AssetReferenceVisualTreeAsset asset))
            {
                throw new KeyNotFoundException($"Screen '{id}' not configured in UIAssetsConfig");
            }

            return asset;
        }

        public AssetReferenceVisualTreeAsset GetScreen<T>() where T : UIScreen
        {
            string enumName = typeof(T).Name.Replace("Screen", "");

            return !Enum.TryParse(enumName, out ScreenId id) ? throw new ArgumentException($"No ScreenId for '{typeof(T).Name}'. Expected enum value: '{enumName}'") : GetScreen(id);
        }

        public AssetReferenceVisualTreeAsset GetPopup(PopupId id)
        {
            ThrowIfNotInitialized();

            if (!_popupCache.TryGetValue(id, out AssetReferenceVisualTreeAsset asset))
            {
                throw new KeyNotFoundException($"Popup '{id}' not configured in UIAssetsConfig");
            }

            return asset;
        }

        public AssetReferenceVisualTreeAsset GetPopup<T>() where T : UIPopup
        {
            string enumName = typeof(T).Name.Replace("Popup", "");

            return !Enum.TryParse(enumName, out PopupId id) ? throw new ArgumentException($"No PopupId for '{typeof(T).Name}'. Expected enum value: '{enumName}'") : GetPopup(id);
        }

        private void ThrowIfNotInitialized()
        {
            if (!_isInitialized || _screenCache == null || _popupCache == null)
            {
                throw new InvalidOperationException("UIAssetsConfig not initialized. Call Initialize() first.");
            }
        }
    }
}