using System.Windows;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainView = new MainWindow();
            mainView.Show();
        }
    }
}
