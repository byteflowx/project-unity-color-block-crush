using App.Scripts.Core.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class RoundedButton : VisualElement
    {
        private const string ROOT_CLASS = "rounded-btn";
        private const string LABEL_CLASS = "rounded-btn__label";
        private const string PRESSED_CLASS = "rounded-btn--pressed";
        private const string SIZE_SMALL_CLASS = "rounded-btn--sm";
        private const string SIZE_LARGE_CLASS = "rounded-btn--lg";

        private static readonly CustomStyleProperty<Color> BackgroundColorProperty = new("--rounded-btn-bg");
        private static readonly CustomStyleProperty<Color> ShadowColorProperty = new("--rounded-btn-shadow");
        private static readonly CustomStyleProperty<float> BorderRadiusProperty = new("--rounded-btn-radius");
        private static readonly CustomStyleProperty<float> ShadowHeightProperty = new("--rounded-btn-shadow-height");

        private readonly Label _label;

        private Color _backgroundColor;
        private Color _shadowColor;
        private float _borderRadius;
        private float _shadowHeight;
        private ControlSize _size = ControlSize.Medium;

        [UxmlAttribute]
        public ControlSize Size
        {
            get => _size;
            set
            {
                _size = value;
                EnableInClassList(SIZE_SMALL_CLASS, value == ControlSize.Small);
                EnableInClassList(SIZE_LARGE_CLASS, value == ControlSize.Large);
            }
        }

        [UxmlAttribute]
        public string Text
        {
            get => _label?.text ?? string.Empty;
            set
            {
                if (_label != null)
                    _label.text = value;
            }
        }

        public RoundedButton() : this(string.Empty) { }

        public RoundedButton(string text)
        {
            AddToClassList(ROOT_CLASS);

            _label = new Label(text);
            _label.AddToClassList(LABEL_CLASS);
            _label.pickingMode = PickingMode.Ignore;
            Add(_label);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            focusable = true;
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(BackgroundColorProperty, out Color bgColor))
                _backgroundColor = bgColor;

            if (evt.customStyle.TryGetValue(ShadowColorProperty, out Color shadowColor))
                _shadowColor = shadowColor;

            if (evt.customStyle.TryGetValue(BorderRadiusProperty, out float borderRadius))
                _borderRadius = borderRadius;

            if (evt.customStyle.TryGetValue(ShadowHeightProperty, out float shadowHeight))
                _shadowHeight = shadowHeight;

            MarkDirtyRepaint();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || !enabledSelf)
                return;

            AddToClassList(PRESSED_CLASS);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 0)
                return;

            RemoveFromClassList(PRESSED_CLASS);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            RemoveFromClassList(PRESSED_CLASS);
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            Rect rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0)
                return;

            Painter2D painter = context.painter2D;
            float effectiveShadowHeight = enabledSelf ? _shadowHeight : 0f;

            if (effectiveShadowHeight > 0)
            {
                Rect shadowRect = new Rect(
                    rect.x,
                    rect.y + effectiveShadowHeight,
                    rect.width,
                    rect.height - effectiveShadowHeight
                );
                DrawRoundedRect(painter, shadowRect, _borderRadius, _shadowColor);
            }

            Rect mainRect = new Rect(
                rect.x,
                rect.y,
                rect.width,
                rect.height - effectiveShadowHeight
            );
            DrawRoundedRect(painter, mainRect, _borderRadius, _backgroundColor);
        }

        private void DrawRoundedRect(Painter2D painter, Rect rect, float radius, Color color)
        {
            radius = Mathf.Min(radius, rect.width * 0.5f, rect.height * 0.5f);

            painter.fillColor = color;
            painter.BeginPath();

            painter.MoveTo(new Vector2(rect.x + radius, rect.y));

            painter.LineTo(new Vector2(rect.xMax - radius, rect.y));
            painter.ArcTo(
                new Vector2(rect.xMax, rect.y),
                new Vector2(rect.xMax, rect.y + radius),
                radius
            );

            painter.LineTo(new Vector2(rect.xMax, rect.yMax - radius));
            painter.ArcTo(
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMax - radius, rect.yMax),
                radius
            );

            painter.LineTo(new Vector2(rect.x + radius, rect.yMax));
            painter.ArcTo(
                new Vector2(rect.x, rect.yMax),
                new Vector2(rect.x, rect.yMax - radius),
                radius
            );

            painter.LineTo(new Vector2(rect.x, rect.y + radius));
            painter.ArcTo(
                new Vector2(rect.x, rect.y),
                new Vector2(rect.x + radius, rect.y),
                radius
            );

            painter.ClosePath();
            painter.Fill();
        }
    }
}