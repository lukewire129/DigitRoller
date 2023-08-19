using CommunityToolkit.Mvvm.ComponentModel;

namespace DigitRoll.Model
{
    public partial class NumericModel : ObservableObject
    {
        public int idx { get; set; }
        [ObservableProperty]
        private string number;

        public NumericModel(int idx)
        {
            this.idx = idx;
        }

        public override string ToString()
        {
            return Number;
        }
    }
}
