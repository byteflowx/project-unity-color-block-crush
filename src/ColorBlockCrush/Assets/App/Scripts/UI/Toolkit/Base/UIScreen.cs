namespace App.Scripts.UI.Toolkit.Base
{
    public abstract class UIScreen : UIView
    {
        protected override string VisibleClass => "screen--visible";
        protected override string HiddenClass => "screen--hidden";

        public virtual bool ShowsBackgroundOverlay => true;
    }
}