using System.Windows;
using System.Windows.Controls;

namespace DigitRoll.Views.Units
{
    public class DigitUnitList : ListBox
    {
        static DigitUnitList()
        {
            DefaultStyleKeyProperty.OverrideMetadata (typeof (DigitUnitList), new FrameworkPropertyMetadata (typeof (DigitUnitList)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DigitUnit ();
        }
    }
}
