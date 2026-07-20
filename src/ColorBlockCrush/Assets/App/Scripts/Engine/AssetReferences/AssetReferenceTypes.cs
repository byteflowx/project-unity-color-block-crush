using System;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace App.Scripts.Engine.AssetReferences
{
    [Serializable]
    public sealed class AssetReferenceVisualTreeAsset : AssetReferenceT<VisualTreeAsset>
    {
        public AssetReferenceVisualTreeAsset(string guid) : base(guid) { }
    }

    [Serializable]
    public sealed class AssetReferencePanelSettings : AssetReferenceT<PanelSettings>
    {
        public AssetReferencePanelSettings(string guid) : base(guid) { }
    }

    [Serializable]
    public sealed class AssetReferenceThemeStyleSheet : AssetReferenceT<ThemeStyleSheet>
    {
        public AssetReferenceThemeStyleSheet(string guid) : base(guid) { }
    }

    [Serializable]
    public sealed class AssetReferenceStyleSheet : AssetReferenceT<StyleSheet>
    {
        public AssetReferenceStyleSheet(string guid) : base(guid) { }
    }
}
