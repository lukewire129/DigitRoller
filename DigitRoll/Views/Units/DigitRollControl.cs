using CommunityToolkit.Mvvm.ComponentModel;
using DigitRoll.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitRoll.Views.Units
{
    [INotifyPropertyChanged]
    public partial class DigitRollControl : ContentControl
    {
        #region DependencyProperty
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumericProperty =
            DependencyProperty.Register ("Numeric", typeof (object), typeof (DigitRollControl), new PropertyMetadata (null, NumbericChanged));


        // Using a DependencyProperty as the backing store for TextColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register ("TextColor", typeof (Brush), typeof (DigitRollControl), new PropertyMetadata (new SolidColorBrush (Colors.Black)));

        public object Numeric
        {
            get { return (object)GetValue (NumericProperty); }
            set { SetValue (NumericProperty, value); }
        }


        public Brush TextColor
        {
            get { return (Brush)GetValue (TextColorProperty); }
            set { SetValue (TextColorProperty, value); }
        }
        #endregion

        [ObservableProperty]
        private ObservableCollection<NumericModel> numerics;

        private DigitUnitList unitList;
        static DigitRollControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata (typeof (DigitRollControl), new FrameworkPropertyMetadata (typeof (DigitRollControl)));
        }
        public DigitRollControl()
        {
            this.Numerics = new ObservableCollection<NumericModel> ();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate ();
            unitList = GetTemplateChild ("PART_DATA") as DigitUnitList;
            unitList.ItemsSource = this.Numerics;
        }
        private static void NumbericChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            if (e.NewValue.GetType () == typeof (Int32) || e.NewValue.GetType () == typeof (Int64) || e.NewValue.GetType () == typeof (Int16) || e.NewValue.GetType () == typeof (string))
            {
                DigitRollControl rollControl = (DigitRollControl)d;

                List<string> chars = new List<string> ();
                chars.AddRange (e.NewValue.ToString ().Select (c => c.ToString ()));

                bool NumericalCount = rollControl.IsNumericalDigit (e.OldValue, e.NewValue);
                if (NumericalCount == true)
                {
                    for(int i =0;i< rollControl.Numerics.Count; i++)
                    {
                        rollControl.Numerics[i].Number = chars[i];
                    }
                }
                else
                {
                    Task.Run (() =>
                    {
                        Application.Current.Dispatcher.Invoke (async() =>
                        {
                            int cnt = chars.Count - rollControl.Numerics.Count;
                            if (cnt > 0)
                            {
                                int lastidx = rollControl.Numerics.Count == 0 ? 0 : rollControl.Numerics.Last ().idx + 1;
                                for (int i = 0; i < cnt; i++)
                                {
                                    rollControl.Numerics.Add (new NumericModel (lastidx + 1));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Math.Abs(cnt); i++)
                                {
                                    rollControl.Numerics.RemoveAt (rollControl.Numerics.Count () - 1);
                                }
                            }
                            for (int i = 0; i < rollControl.Numerics.Count; i++)
                            {
                                rollControl.Numerics[i].Number = "0";
                            }

                            await Task.Delay (1000);

                            for(int i=0; i< chars.Count; i++)
                            {
                                rollControl.Numerics[i].Number = chars[i];
                            }
                        });                        
                    });
                }
            }
            else
                return;
        }


        public bool IsNumericalDigit(object oldNum, object newNum)
        {
            if (oldNum == null)
                return false;
            int _oldNumCnt = oldNum.ToString ().Length;
            int _newNumCnt = newNum.ToString ().Length;

            return _oldNumCnt == _newNumCnt;
        }
    }
}
