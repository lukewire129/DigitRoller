using System;
using System.Windows;
using System.Windows.Threading;

namespace DigitRollExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent ();
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new System.TimeSpan (0, 0, 1);
            timer.Start ();
        }

        private void Timer_Tick(object? sender, System.EventArgs e)
        {
            DateTime tm = DateTime.Now;
            rollTextHour.Number = tm.Hour.ToString ("D2");
            rollTextMin.Number = tm.Minute.ToString ("D2");
            rollTextSec.Number = tm.Second.ToString("D2");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChangeText.Number = InputText.Text;
        }
    }
}
