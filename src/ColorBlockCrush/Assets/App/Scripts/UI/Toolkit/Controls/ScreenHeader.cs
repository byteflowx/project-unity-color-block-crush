using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class ScreenHeader : VisualElement
    {
        private const string ROOT_CLASS = "screen-header";
        private const string LEFT_CLASS = "screen-header__left";
        private const string RIGHT_CLASS = "screen-header__right";

        private readonly VisualElement _leftContainer;
        private readonly VisualElement _rightContainer;

        public VisualElement LeftContainer => _leftContainer;
        public VisualElement RightContainer => _rightContainer;

        public ScreenHeader()
        {
            AddToClassList(ROOT_CLASS);

            _leftContainer = new VisualElement { name = "screen-header__left" };
            _leftContainer.AddToClassList(LEFT_CLASS);
            _leftContainer.pickingMode = PickingMode.Ignore;
            Add(_leftContainer);

            _rightContainer = new VisualElement { name = "screen-header__right" };
            _rightContainer.AddToClassList(RIGHT_CLASS);
            _rightContainer.pickingMode = PickingMode.Ignore;
            Add(_rightContainer);
        }
    }
}
