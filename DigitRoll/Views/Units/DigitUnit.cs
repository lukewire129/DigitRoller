using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DigitRoll.Views.Units
{
    public class DigitUnit : ContentControl
    {
        public string Text
        {
            get { return (string)GetValue (TextProperty); }
            set { SetValue (TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Number.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register ("Text", typeof (string), typeof (DigitUnit), new PropertyMetadata (null, NumberChanged));
        public string Number
        {
            get { return (string)GetValue (NumberProperty); }
            set { SetValue (NumberProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Number.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register ("Number", typeof (string), typeof (DigitUnit), new PropertyMetadata (null, NumberChanged));

        private static void NumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DigitUnit digit = (DigitUnit)d;

            int? totalMove = digit.IsNumberHight (e.NewValue.ToString ());
            if (totalMove == null)
            {
                Application.Current.Dispatcher.Invoke (() =>
                {
                    digit.Text = "0";
                });
                return;
            }
            if (totalMove.Value == 0)
                return;
         
            Application.Current.Dispatcher.Invoke (() =>
            {
                int speed = 300 / totalMove.Value;
                int nowData = digit.GetContent ().Value + 1;
                if (nowData >= 10)
                    nowData = 0;
                digit.StartRollUpAnimation ((nowData).ToString (), speed / 2, totalMove.Value - 1);
            });
        }
        private async void StartRollUpAnimation(string value, int delay, int maxCnt)
        {
            int _maxCnt = maxCnt;
            double targetY = -this.ActualHeight; // 텍스트를 위로 올릴 목표 위치
            string _val = value;
            int msec = delay;

            DoubleAnimation animation = GetAnimation (0, targetY, msec);

            animation.Completed += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke (async() =>
                {
                    this.Text = _val;
                    LowerToUp (msec);
                    //this.RenderTransform = new TranslateTransform (0, 0);
                    await Task.Delay (msec).ContinueWith ((x) =>
                    {
                        Application.Current.Dispatcher.Invoke (async () =>
                        {
                            if (_maxCnt == 0)
                                return;
                            int nowData = this.GetContent ().Value + 1;
                            if (nowData >= 10)
                                nowData = 0;
                            StartRollUpAnimation (nowData.ToString (), msec, _maxCnt - 1);
                        });
                    });
                });
            };

            this.RenderTransform = new TranslateTransform ();
            RenderTransform.BeginAnimation (TranslateTransform.YProperty, animation);
        }
        private DoubleAnimation GetAnimation(double from, double to, double msec)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds (msec), // 애니메이션 지속 시간
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
        }
        private void LowerToUp(int msec)
        {
            double targetY = this.ActualHeight; // 텍스트를 위로 올릴 목표 위치
            DoubleAnimation animation = GetAnimation (targetY, 0, msec);
            this.RenderTransform = new TranslateTransform ();
            RenderTransform.BeginAnimation (TranslateTransform.YProperty, animation);
        }
        TextBlock tb;
        static DigitUnit()
        {
            DefaultStyleKeyProperty.OverrideMetadata (typeof (DigitUnit), new FrameworkPropertyMetadata (typeof (DigitUnit)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate ();
            tb = GetTemplateChild("PART_RollingText") as TextBlock;
        }
        public DigitUnit()
        {
            this.SetBinding (NumberProperty, "Number");
        }
        public int? IsNumberHight(string newData)
        {
            if (GetContent () == null && newData == "0")
            {
                return null;
            }
            int _nowData = GetContent ().Value;
            int _newData = Convert.ToInt32 (newData);

            if (_newData >= _nowData)
                return _newData - _nowData;

            return 10 - _nowData + _newData;
        }

        public int? GetContent()
        {
            string num = this.Text;
            if (num == null)
                return null;
            return Convert.ToInt32 (num);
        }
    }
}
