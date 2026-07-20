using App.Scripts.Core.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class CircleButton : VisualElement
    {
        private const string ROOT_CLASS = "circle-btn";
        private const string ICON_CLASS = "circle-btn__icon";
        private const string PRESSED_CLASS = "circle-btn--pressed";
        private const string SIZE_SMALL_CLASS = "circle-btn--sm";
        private const string SIZE_LARGE_CLASS = "circle-btn--lg";

        private static readonly CustomStyleProperty<Color> BackgroundColorProperty = new("--circle-btn-background");
        private static readonly CustomStyleProperty<Color> BorderColorProperty = new("--circle-btn-border");
        private static readonly CustomStyleProperty<float> BorderWidthProperty = new("--circle-btn-border-width");

        private readonly Image _iconImage;

        private Color _backgroundColor;
        private Color _borderColor;
        private float _borderWidth;
        private ControlSize _size = ControlSize.Medium;
        private Sprite _icon;

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
        public Sprite Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                _iconImage.sprite = value;
            }
        }

        public CircleButton()
        {
            AddToClassList(ROOT_CLASS);

            _iconImage = new Image();
            _iconImage.AddToClassList(ICON_CLASS);
            _iconImage.pickingMode = PickingMode.Ignore;
            Add(_iconImage);

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

            if (evt.customStyle.TryGetValue(BorderColorProperty, out Color borderColor))
                _borderColor = borderColor;

            if (evt.customStyle.TryGetValue(BorderWidthProperty, out float borderWidth))
                _borderWidth = borderWidth;

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
            Vector2 center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

            float fillRadius = radius - _borderWidth * 0.5f;
            painter.fillColor = _backgroundColor;
            painter.BeginPath();
            painter.Arc(center, fillRadius, 0f, 360f);
            painter.Fill();

            if (_borderWidth > 0)
            {
                painter.strokeColor = _borderColor;
                painter.lineWidth = _borderWidth;
                painter.BeginPath();
                painter.Arc(center, fillRadius, 0f, 360f);
                painter.Stroke();
            }
        }
    }
}