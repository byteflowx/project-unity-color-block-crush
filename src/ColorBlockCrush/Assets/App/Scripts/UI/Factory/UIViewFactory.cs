using System;
using App.Scripts.UI.Services.Notification;
using App.Scripts.UI.Toolkit.Base;
using UnityEngine.UIElements;

namespace App.Scripts.UI.Factory
{
    public sealed class UIViewFactory : IUIViewFactory
    {
        private readonly Func<Type, UIView> _create;
        private readonly INotificationService _notifications;

        public UIViewFactory(Func<Type, UIView> create, INotificationService notifications)
        {
            _create = create;
            _notifications = notifications;
        }

        public TView Create<TView>(VisualElement root) where TView : UIView
        {
            TView view = (TView)_create(typeof(TView));
            view.Initialize(root, _notifications);
            return view;
        }
    }
}
