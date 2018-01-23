namespace Tutorial.Uwp
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class MainPage : Page
    {
        public MainPage() => this.InitializeComponent();

        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            SynchronizationContext synchronizationContext1 = SynchronizationContext.Current;
            ExecutionContext executionContext1 = ExecutionContext.Capture();
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(continueOnCapturedContext: true);
            // Equivalent to: await Task.Delay(TimeSpan.FromSeconds(1));

            // Continuation is executed with captured runtime context.
            SynchronizationContext synchronizationContext2 = SynchronizationContext.Current;
            Debug.WriteLine(synchronizationContext1 == synchronizationContext2); // True
            this.Button.Background = new SolidColorBrush(Colors.Blue); // UI update works.
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(continueOnCapturedContext: false);

            // Continuation is executed without captured runtime context.
            SynchronizationContext synchronizationContext3 = SynchronizationContext.Current;
            Debug.WriteLine(synchronizationContext1 == synchronizationContext3); // False
            this.Button.Background = new SolidColorBrush(Colors.Yellow); // UI update fails.
            // Exception: The application called an interface that was marshalled for a different thread.
        }
    }
}
