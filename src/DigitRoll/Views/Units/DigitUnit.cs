using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DigitRoll.Views.Units;

public class DigitUnit : ContentControl
{
    private TextBlock _textBlock;

    public string DisplayText
    {
        get { return (string)GetValue (DisplayTextProperty); }
        set { SetValue (DisplayTextProperty, value); }
    }

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register ("DisplayText", typeof (string), typeof (DigitUnit),
            new PropertyMetadata ("0"));

    static DigitUnit()
    {
        DefaultStyleKeyProperty.OverrideMetadata (typeof (DigitUnit),
            new FrameworkPropertyMetadata (typeof (DigitUnit)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate ();
        _textBlock = GetTemplateChild ("PART_RollingText") as TextBlock;
    }

    public void SetDigitImmediately(int digit)
    {
        // ✅ UI 스레드 체크 추가
        if (Application.Current?.Dispatcher.CheckAccess () == false)
        {
            Application.Current.Dispatcher.BeginInvoke (() => SetDigitImmediately (digit));
            return;
        }

        DisplayText = digit.ToString ();
        if (_textBlock != null)
        {
            _textBlock.RenderTransform = new TranslateTransform (0, 0);
        }
    }

    public void RollToDigit(int fromDigit, int toDigit)
    {
        // ✅ UI 스레드 체크 추가
        if (Application.Current?.Dispatcher.CheckAccess () == false)
        {
            Application.Current.Dispatcher.BeginInvoke (() => RollToDigit (fromDigit, toDigit));
            return;
        }

        if (_textBlock == null)
        {
            DisplayText = toDigit.ToString ();
            return;
        }

        // ✅ 증가/감소 방향 결정
        bool isIncreasing = toDigit > fromDigit || (fromDigit == 9 && toDigit == 0);

        if (isIncreasing)
        {
            // 위로 올라가는 애니메이션 (0→1, 8→9, 9→0)
            int rollDistance = CalculateRollDistance (fromDigit, toDigit);
            int animationSpeed = Math.Max (50, 300 / rollDistance);
            StartRollingUpAnimation (fromDigit, toDigit, rollDistance, animationSpeed);
        }
        else
        {
            // 아래로 내려가는 애니메이션 (9→8, 5→3)
            int rollDistance = CalculateRollDownDistance (fromDigit, toDigit);
            int animationSpeed = Math.Max (50, 300 / rollDistance);
            StartRollingDownAnimation (fromDigit, toDigit, rollDistance, animationSpeed);
        }
    }

    private int CalculateRollDistance(int from, int to)
    {
        if (to >= from)
            return to - from;
        return 10 - from + to;
    }

    private int CalculateRollDownDistance(int from, int to)
    {
        // 감소 거리 계산
        if (from >= to)
            return from - to;
        return from + (10 - to);
    }

    private async void StartRollingUpAnimation(int currentDigit, int targetDigit, int totalSteps, int stepDuration)
    {
        if (totalSteps == 0)
            return;

        for (int step = 0; step < totalSteps; step++)
        {
            int nextDigit = (currentDigit + 1) % 10;

            await Application.Current.Dispatcher.InvokeAsync (async () =>
            {
                if (_textBlock == null || ActualHeight == 0)
                    return;

                double targetY = -ActualHeight;
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = targetY,
                    Duration = TimeSpan.FromMilliseconds (stepDuration),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };

                var transform = new TranslateTransform ();
                _textBlock.RenderTransform = transform;

                animation.Completed += async (s, e) =>
                {
                    DisplayText = nextDigit.ToString ();

                    // 다시 아래에서 위로 올라오는 애니메이션
                    var returnAnimation = new DoubleAnimation
                    {
                        From = ActualHeight,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds (stepDuration),
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                    };

                    transform.BeginAnimation (TranslateTransform.YProperty, returnAnimation);
                };

                transform.BeginAnimation (TranslateTransform.YProperty, animation);
                await Task.Delay (stepDuration * 2);
            });

            currentDigit = nextDigit;
        }
    }

    private async void StartRollingDownAnimation(int currentDigit, int targetDigit, int totalSteps, int stepDuration)
    {
        if (totalSteps == 0)
            return;

        for (int step = 0; step < totalSteps; step++)
        {
            int nextDigit = currentDigit - 1;
            if (nextDigit < 0)
                nextDigit = 9;

            await Application.Current.Dispatcher.InvokeAsync (async () =>
            {
                if (_textBlock == null || ActualHeight == 0)
                    return;

                // ✅ 아래로 내려가는 애니메이션
                double targetY = ActualHeight;
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = targetY,
                    Duration = TimeSpan.FromMilliseconds (stepDuration),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };

                var transform = new TranslateTransform ();
                _textBlock.RenderTransform = transform;

                animation.Completed += async (s, e) =>
                {
                    DisplayText = nextDigit.ToString ();

                    // 다시 위에서 아래로 내려오는 애니메이션
                    var returnAnimation = new DoubleAnimation
                    {
                        From = -ActualHeight,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds (stepDuration),
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                    };

                    transform.BeginAnimation (TranslateTransform.YProperty, returnAnimation);
                };

                transform.BeginAnimation (TranslateTransform.YProperty, animation);
                await Task.Delay (stepDuration * 2);
            });

            currentDigit = nextDigit;
        }
    }
}