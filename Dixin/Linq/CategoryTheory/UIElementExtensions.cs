namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;

    [Pure]
    public static class UIElementExtensions
    {
        public static IObservable<EventPattern<MouseEventArgs>> MouseDrag
            (this UIElement element) =>
                from _ in Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseDown))
                from @event in Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseMove))
                    .TakeUntil(Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseUp)))
                select @event;
    }
}