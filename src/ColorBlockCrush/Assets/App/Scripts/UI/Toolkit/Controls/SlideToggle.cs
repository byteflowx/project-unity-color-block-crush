using App.Scripts.Core.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class SlideToggle : BaseField<bool>
    {
        private const string ROOT_CLASS = "slide-toggle";
        private const string INPUT_CLASS = "slide-toggle__input";
        private const string KNOB_CLASS = "slide-toggle__knob";
        private const string CHECKED_CLASS = "slide-toggle__input--checked";
        private const string SIZE_SMALL_CLASS = "slide-toggle--sm";
        private const string SIZE_LARGE_CLASS = "slide-toggle--lg";

        private readonly VisualElement _input;
        private readonly VisualElement _knob;
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

        public SlideToggle() : this(null) { }

        public SlideToggle(string label) : base(label, new VisualElement())
        {
            AddToClassList(ROOT_CLASS);

            _input = this.Q(className: inputUssClassName);
            _input.AddToClassList(INPUT_CLASS);

            _knob = new VisualElement();
            _knob.AddToClassList(KNOB_CLASS);
            _input.Add(_knob);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<KeyDownEvent>(OnKeydownEvent);
            RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        private static void OnClick(ClickEvent evt)
        {
            SlideToggle slideToggle = evt.currentTarget as SlideToggle;
            slideToggle?.ToggleValue();
            evt.StopPropagation();
        }

        private static void OnSubmit(NavigationSubmitEvent evt)
        {
            SlideToggle slideToggle = evt.currentTarget as SlideToggle;
            slideToggle?.ToggleValue();
            evt.StopPropagation();
        }

        private static void OnKeydownEvent(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.KeypadEnter && evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.Space)
                return;

            SlideToggle slideToggle = evt.currentTarget as SlideToggle;
            slideToggle?.ToggleValue();
            evt.StopPropagation();
        }

        private void ToggleValue()
        {
            value = !value;
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            _input?.EnableInClassList(CHECKED_CLASS, newValue);
        }
    }
}
