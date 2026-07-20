using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class SafeArea : VisualElement
    {
        [UxmlAttribute("collapse-margins")]
        public bool CollapseMargins { get; set; } = true;

        [UxmlAttribute("exclude-left")]
        public bool ExcludeLeft { get; set; }

        [UxmlAttribute("exclude-right")]
        public bool ExcludeRight { get; set; }

        [UxmlAttribute("exclude-top")]
        public bool ExcludeTop { get; set; }

        [UxmlAttribute("exclude-bottom")]
        public bool ExcludeBottom { get; set; }

        private readonly struct SafeAreaOffset
        {
            public readonly float Left;
            public readonly float Right;
            public readonly float Top;
            public readonly float Bottom;

            public SafeAreaOffset(float left, float right, float top, float bottom)
            {
                Left = left;
                Right = right;
                Top = top;
                Bottom = bottom;
            }

            public bool Equals(SafeAreaOffset other)
            {
                const float tolerance = 0.01f;
                return Mathf.Abs(Left - other.Left) < tolerance &&
                       Mathf.Abs(Right - other.Right) < tolerance &&
                       Mathf.Abs(Top - other.Top) < tolerance &&
                       Mathf.Abs(Bottom - other.Bottom) < tolerance;
            }
        }

        private readonly VisualElement _contentContainer;
        private SafeAreaOffset _lastAppliedOffset;

        public override VisualElement contentContainer => _contentContainer;

        public SafeArea()
        {
            style.position = Position.Absolute;
            style.top = 0;
            style.bottom = 0;
            style.left = 0;
            style.right = 0;
            pickingMode = PickingMode.Ignore;

            _contentContainer = new VisualElement
            {
                name = "safe-area-content",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 0
                }
            };
            hierarchy.Add(_contentContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ApplySafeArea();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (panel == null || panel.contextType != ContextType.Player)
                return;

            SafeAreaOffset safeAreaOffset = CalculateSafeAreaOffset();

            if (safeAreaOffset.Equals(_lastAppliedOffset))
                return;

            _lastAppliedOffset = safeAreaOffset;

            SafeAreaOffset marginOffset = GetMarginOffset();

            if (CollapseMargins)
            {
                _contentContainer.style.marginLeft = Mathf.Max(marginOffset.Left, safeAreaOffset.Left) - marginOffset.Left;
                _contentContainer.style.marginRight = Mathf.Max(marginOffset.Right, safeAreaOffset.Right) - marginOffset.Right;
                _contentContainer.style.marginTop = Mathf.Max(marginOffset.Top, safeAreaOffset.Top) - marginOffset.Top;
                _contentContainer.style.marginBottom = Mathf.Max(marginOffset.Bottom, safeAreaOffset.Bottom) - marginOffset.Bottom;
            }
            else
            {
                _contentContainer.style.marginLeft = safeAreaOffset.Left;
                _contentContainer.style.marginRight = safeAreaOffset.Right;
                _contentContainer.style.marginTop = safeAreaOffset.Top;
                _contentContainer.style.marginBottom = safeAreaOffset.Bottom;
            }
        }

        private SafeAreaOffset CalculateSafeAreaOffset()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 leftTop = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(safeArea.xMin, Screen.height - safeArea.yMax));
            Vector2 rightBottom = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(Screen.width - safeArea.xMax, safeArea.yMin));

            return new SafeAreaOffset(
                ExcludeLeft ? 0 : leftTop.x,
                ExcludeRight ? 0 : rightBottom.x,
                ExcludeTop ? 0 : leftTop.y,
                ExcludeBottom ? 0 : rightBottom.y
            );
        }

        private SafeAreaOffset GetMarginOffset()
        {
            return new SafeAreaOffset(
                resolvedStyle.marginLeft,
                resolvedStyle.marginRight,
                resolvedStyle.marginTop,
                resolvedStyle.marginBottom
            );
        }
    }
}
