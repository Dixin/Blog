namespace Tutorial.CategoryTheory
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;

    public static class UIElementExtensions
    {
#if NETFX
        public static IObservable<EventPattern<MouseEventArgs>> MouseDrag
            (this UIElement element) =>
                from _ in Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseDown))
                from @event in Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseMove))
                    .TakeUntil(Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseUp)))
                select @event;
#endif
    }
}