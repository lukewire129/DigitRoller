using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DigitRoll.Views.Units;

public partial class DigitRollControl : ContentControl
{
    private Panel _digitContainer;
    private string _currentValue = "0";
    private readonly object _lockObject = new object ();

    public string Number
    {
        get { return (string)GetValue (NumberProperty); }
        set { SetValue (NumberProperty, value); }
    }

    public static readonly DependencyProperty NumberProperty =
        DependencyProperty.Register ("Number", typeof (string), typeof (DigitRollControl),
            new PropertyMetadata ("0", OnNumberChanged));

    static DigitRollControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata (typeof (DigitRollControl),
            new FrameworkPropertyMetadata (typeof (DigitRollControl)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate ();

        lock (_lockObject)
        {
            _digitContainer = GetTemplateChild ("PART_DigitContainer") as Panel;

            // Template이 적용된 후 현재 값으로 초기화
            if (_digitContainer != null && !string.IsNullOrEmpty (_currentValue))
            {
                InitializeDigits (_currentValue);
            }
        }
    }

    private static void OnNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var roller = (DigitRollControl)d;
        string newValue = e.NewValue?.ToString () ?? "0";

        // 숫자만 추출
        newValue = new string (newValue.Where (char.IsDigit).ToArray ());
        if (string.IsNullOrEmpty (newValue))
            newValue = "0";

        // UI 스레드에서 실행 보장
        if (Application.Current?.Dispatcher.CheckAccess () == false)
        {
            Application.Current.Dispatcher.BeginInvoke (() => roller.UpdateDigits (newValue));
        }
        else
        {
            roller.UpdateDigits (newValue);
        }
    }

    // ✅ 초기화 전용 메서드 (애니메이션 없음)
    private void InitializeDigits(string value)
    {
        _digitContainer.Children.Clear ();

        for (int i = 0; i < value.Length; i++)
        {
            var digitUnit = new DigitUnit ();
            _digitContainer.Children.Add (digitUnit);
            digitUnit.SetDigitImmediately (value[i] - '0');
        }

        _currentValue = value;
    }

    private void UpdateDigits(string newValue)
    {
        lock (_lockObject)
        {
            if (_digitContainer == null)
            {
                _currentValue = newValue;
                return;
            }

            string oldValue = _currentValue;

            // 첫 초기화인 경우
            if (_digitContainer.Children.Count == 0)
            {
                InitializeDigits (newValue);
                return;
            }

            _currentValue = newValue;

            int oldLength = oldValue.Length;
            int newLength = newValue.Length;

            // 자릿수가 줄어드는 경우 (10 -> 9)
            if (newLength < oldLength)
            {
                // 앞자리부터 제거
                int removeCount = oldLength - newLength;
                for (int i = 0; i < removeCount && _digitContainer.Children.Count > 0; i++)
                {
                    _digitContainer.Children.RemoveAt (0);
                }

                // 남은 자리수 업데이트
                for (int i = 0; i < newLength && i < _digitContainer.Children.Count; i++)
                {
                    if (_digitContainer.Children[i] is DigitUnit digitUnit)
                    {
                        // ✅ 수정: 올바른 인덱스 계산
                        int oldDigit = oldValue[removeCount + i] - '0';
                        int newDigit = newValue[i] - '0';

                        if (oldDigit != newDigit)
                        {
                            digitUnit.RollToDigit (oldDigit, newDigit);
                        }
                        else
                        {
                            digitUnit.SetDigitImmediately (newDigit);
                        }
                    }
                }
            }
            // 자릿수가 늘어나는 경우 (9 -> 10)
            else if (newLength > oldLength)
            {
                // ✅ 먼저 기존 자리수 업데이트 (뒤쪽부터)
                int existingDigits = Math.Min (oldLength, _digitContainer.Children.Count);
                int startIndex = newLength - existingDigits; // 새 값에서 시작 위치

                for (int i = 0; i < existingDigits; i++)
                {
                    if (_digitContainer.Children[i] is DigitUnit digitUnit)
                    {
                        int oldDigit = oldValue[i] - '0';
                        int newDigit = newValue[startIndex + i] - '0';

                        if (oldDigit != newDigit)
                        {
                            digitUnit.RollToDigit (oldDigit, newDigit);
                        }
                        else
                        {
                            digitUnit.SetDigitImmediately (newDigit);
                        }
                    }
                }

                // ✅ 앞쪽에 새 자리수 추가 (역순으로 Insert)
                int addCount = newLength - oldLength;
                for (int i = addCount - 1; i >= 0; i--)
                {
                    var digitUnit = new DigitUnit ();
                    _digitContainer.Children.Insert (0, digitUnit);
                    digitUnit.RollToDigit (0, newValue[i] - '0');
                }
            }
            // 자릿수가 같은 경우
            else
            {
                for (int i = 0; i < newLength && i < _digitContainer.Children.Count; i++)
                {
                    if (_digitContainer.Children[i] is DigitUnit digitUnit)
                    {
                        int oldDigit = oldValue[i] - '0';
                        int newDigit = newValue[i] - '0';

                        if (oldDigit != newDigit)
                        {
                            digitUnit.RollToDigit (oldDigit, newDigit);
                        }
                        else
                        {
                            digitUnit.SetDigitImmediately (newDigit);
                        }
                    }
                }
            }
        }
    }
}