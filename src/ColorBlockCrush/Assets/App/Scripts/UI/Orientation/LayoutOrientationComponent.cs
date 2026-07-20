using App.Scripts.Core.Enums;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Orientation
{
    public sealed class LayoutOrientationComponent : MonoBehaviour
    {
        private const float THRESHOLD = 1.2f;

        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _root;
        private Vector2 _lastResolution;
        private readonly ReactiveProperty<LayoutOrientation> _currentOrientation = new(LayoutOrientation.Portrait);
        public ReadOnlyReactiveProperty<LayoutOrientation> CurrentOrientation => _currentOrientation;

        
        private void OnEnable()
        {
            if (_uiDocument == null)
                return;

            _root = _uiDocument.rootVisualElement;

            if (_root != null)
            {
                _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                UpdateFromScreenSize();
            }
        }

        private void OnDisable()
        {
            _root?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnDestroy()
        {
            _currentOrientation.Dispose();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateResolution();
        }

        private void UpdateFromScreenSize()
        {
            Vector2 screenSize = new(Screen.width, Screen.height);
            _currentOrientation.Value = CalculateOrientation(screenSize);
        }

        private void UpdateResolution()
        {
            if (_root == null)
                return;

            Vector2 newResolution = new(_root.resolvedStyle.width, _root.resolvedStyle.height);

            if (newResolution.x <= 0 || newResolution.y <= 0)
                return;

            if (_lastResolution == newResolution)
                return;

            _lastResolution = newResolution;
            _currentOrientation.Value = CalculateOrientation(newResolution);
        }

        private static LayoutOrientation CalculateOrientation(Vector2 resolution)
        {
            if (resolution.y <= 0)
                return LayoutOrientation.Portrait;

            float aspectRatio = resolution.x / resolution.y;
            return aspectRatio >= THRESHOLD ? LayoutOrientation.Landscape : LayoutOrientation.Portrait;
        }
    }
}
