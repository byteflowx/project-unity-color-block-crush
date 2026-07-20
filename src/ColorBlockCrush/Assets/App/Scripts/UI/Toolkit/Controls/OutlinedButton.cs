using App.Scripts.Core.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class OutlinedButton : VisualElement
    {
        private const string ROOT_CLASS = "outlined-btn";
        private const string LABEL_CLASS = "outlined-btn__label";
        private const string PRESSED_CLASS = "outlined-btn--pressed";
        private const string SIZE_SMALL_CLASS = "outlined-btn--sm";
        private const string SIZE_LARGE_CLASS = "outlined-btn--lg";

        private static readonly CustomStyleProperty<Color> BorderColorProperty = new("--outlined-btn-border");
        private static readonly CustomStyleProperty<float> BorderWidthProperty = new("--outlined-btn-border-width");
        private static readonly CustomStyleProperty<float> BorderRadiusProperty = new("--outlined-btn-radius");

        private readonly Label _label;

        private Color _borderColor;
        private float _borderWidth;
        private float _borderRadius;
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

        public OutlinedButton() : this(string.Empty) { }

        public OutlinedButton(string text)
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
            if (evt.customStyle.TryGetValue(BorderColorProperty, out Color borderColor))
                _borderColor = borderColor;

            if (evt.customStyle.TryGetValue(BorderWidthProperty, out float borderWidth))
                _borderWidth = borderWidth;

            if (evt.customStyle.TryGetValue(BorderRadiusProperty, out float borderRadius))
                _borderRadius = borderRadius;

            MarkDirtyRepaint();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
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
            DrawRoundedRectOutline(painter, rect, _borderRadius, _borderColor, _borderWidth);
        }

        private void DrawRoundedRectOutline(Painter2D painter, Rect rect, float radius, Color color, float lineWidth)
        {
            float halfLine = lineWidth * 0.5f;
            Rect insetRect = new Rect(
                rect.x + halfLine,
                rect.y + halfLine,
                rect.width - lineWidth,
                rect.height - lineWidth
            );

            radius = Mathf.Min(radius, insetRect.width * 0.5f, insetRect.height * 0.5f);

            painter.strokeColor = color;
            painter.lineWidth = lineWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            painter.BeginPath();

            painter.MoveTo(new Vector2(insetRect.x + radius, insetRect.y));

            painter.LineTo(new Vector2(insetRect.xMax - radius, insetRect.y));
            painter.ArcTo(
                new Vector2(insetRect.xMax, insetRect.y),
                new Vector2(insetRect.xMax, insetRect.y + radius),
                radius
            );

            painter.LineTo(new Vector2(insetRect.xMax, insetRect.yMax - radius));
            painter.ArcTo(
                new Vector2(insetRect.xMax, insetRect.yMax),
                new Vector2(insetRect.xMax - radius, insetRect.yMax),
                radius
            );

            painter.LineTo(new Vector2(insetRect.x + radius, insetRect.yMax));
            painter.ArcTo(
                new Vector2(insetRect.x, insetRect.yMax),
                new Vector2(insetRect.x, insetRect.yMax - radius),
                radius
            );

            painter.LineTo(new Vector2(insetRect.x, insetRect.y + radius));
            painter.ArcTo(
                new Vector2(insetRect.x, insetRect.y),
                new Vector2(insetRect.x + radius, insetRect.y),
                radius
            );

            painter.ClosePath();
            painter.Stroke();
        }
    }
}