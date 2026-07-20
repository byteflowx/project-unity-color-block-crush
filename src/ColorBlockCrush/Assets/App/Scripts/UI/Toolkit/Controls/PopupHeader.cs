using UnityEngine.UIElements;

namespace App.Scripts.UI.Toolkit.Controls
{
    [UxmlElement]
    public partial class PopupHeader : VisualElement
    {
        private const string ROOT_CLASS = "popup-header";
        private const string LEFT_CLASS = "popup-header__left";
        private const string TITLE_CLASS = "popup-header__title";
        private const string RIGHT_CLASS = "popup-header__right";

        private readonly VisualElement _leftContainer;
        private readonly Label _titleLabel;
        private readonly VisualElement _rightContainer;

        private string _title = "Title";

        public VisualElement LeftContainer => _leftContainer;
        public VisualElement RightContainer => _rightContainer;

        [UxmlAttribute]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                _titleLabel.text = value;
            }
        }

        public PopupHeader()
        {
            AddToClassList(ROOT_CLASS);

            _leftContainer = new VisualElement { name = "popup-header__left" };
            _leftContainer.AddToClassList(LEFT_CLASS);
            _leftContainer.pickingMode = PickingMode.Ignore;
            Add(_leftContainer);

            _titleLabel = new Label(_title) { name = "popup-header__title" };
            _titleLabel.AddToClassList(TITLE_CLASS);
            _titleLabel.pickingMode = PickingMode.Ignore;
            Add(_titleLabel);

            _rightContainer = new VisualElement { name = "popup-header__right" };
            _rightContainer.AddToClassList(RIGHT_CLASS);
            _rightContainer.pickingMode = PickingMode.Ignore;
            Add(_rightContainer);
        }
    }
}