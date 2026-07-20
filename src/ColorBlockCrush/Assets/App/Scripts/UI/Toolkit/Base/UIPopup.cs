namespace App.Scripts.UI.Toolkit.Base
{
    public abstract class UIPopup : UIView
    {
        protected override string VisibleClass => "popup--visible";
        protected override string HiddenClass => "popup--hidden";

        public virtual bool CloseOnOverlayClick => true;
        public virtual bool ShowOverlay => true;
    }
}