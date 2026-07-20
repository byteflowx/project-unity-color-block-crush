using System;
using App.Scripts.Core.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.StaticData
{
    [CreateAssetMenu(fileName = "ThemeSettings", menuName = "App/Theme Settings")]
    public sealed class ThemeSettings : ScriptableObject
    {
        [SerializeField] private OrientationTheme _portrait;
        [SerializeField] private OrientationTheme _landscape;

        public OrientationTheme Portrait => _portrait;
        public OrientationTheme Landscape => _landscape;

        public OrientationTheme Resolve(LayoutOrientation orientation)
            => orientation == LayoutOrientation.Portrait ? _portrait : _landscape;

        [Serializable]
        public sealed class OrientationTheme
        {
            [SerializeField] private ThemeStyleSheet _themeStyleSheet;
            [SerializeField] private Vector2Int _referenceResolution = new(1080, 1920);
            [SerializeField, Range(0f, 1f)] private float _match = 1f;

            public ThemeStyleSheet ThemeStyleSheet => _themeStyleSheet;
            public Vector2Int ReferenceResolution => _referenceResolution;
            public float Match => _match;
        }
    }
}
