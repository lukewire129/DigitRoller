using System;
using System.Timers;
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
            rollTextHour.Numeric = tm.Hour.ToString ("D2");
            rollTextMin.Numeric = tm.Minute.ToString ("D2");
            rollTextSec.Numeric = tm.Second.ToString("D2");
        }
    }
}
