using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _side = 10;
        private int _offset = 200;
        private readonly CellCalculation.Manager _manager = new CellCalculation.Manager();

        public MainWindow()
        {
            InitializeComponent();
            _manager.Start(SetCellOnDispatcher);
        }

        protected override void OnClosed(EventArgs e)
        {
            _manager.Dispose();
            base.OnClosed(e);
        }

        private void SetCellOnDispatcher(int x, int y, bool active)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                // Use the Dispatcher to push this to the UI thread
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    HandleRect(x, y, active);
                }));
            });
        }

        private void HandleRect(int x, int y, bool active)
        {
            Rectangle myRect = MyCanvas.Children.OfType<Rectangle>().FirstOrDefault(
                        rect =>
                            Canvas.GetTop(rect) == _offset + _side * y && Canvas.GetLeft(rect) == _offset + _side * x);

            if (myRect == null)
                if (active)
                    AddRec(x, y);
                else
                    return;

            if (active)
                return;

            MyCanvas.Children.Remove(myRect);
        }

        private void AddRec(int x, int y)
        {
            Rectangle rec = new Rectangle {Width = _side, Height = _side, Fill = Brushes.Black};
            MyCanvas.Children.Add(rec);
            Canvas.SetTop(rec, _offset + y * _side);
            Canvas.SetLeft(rec, _offset + x * _side);
        }
    }
}
