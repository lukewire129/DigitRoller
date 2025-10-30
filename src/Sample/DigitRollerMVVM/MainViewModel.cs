using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DigitRollerMVVM;

public partial class MainViewModel : ObservableObject
{
    int _iNumber = 0;
    [ObservableProperty] string number;

    private Func<int, int> _counterFunc;
    public MainViewModel()
    {
        _counterFunc = (n) => n;

        new Thread (() =>
        {
            while(true)
            {
                _iNumber = _counterFunc (_iNumber);
                Number = _iNumber.ToString ();
                Thread.Sleep (1000);
            }
        }).Start();
    }

    [RelayCommand]
    private void Up()
    {
        _counterFunc = (n) => n + 1;
    }

    [RelayCommand]
    private void Down()
    {
        _counterFunc = (n) => n - 1;
    }

    [RelayCommand]
    private void Clear()
    {
        // 카운팅 중단 + 0으로 초기화
        _counterFunc = (n) => n; // 변화 없음
        _iNumber = 0;
        Number = "0";
    }
}
