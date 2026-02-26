using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

using TimmyTools.WpfUi.Services;

namespace TimmyTools.WpfUi.ViewModels;

public class AtomicClockViewModel : INotifyPropertyChanged
{
    private readonly NtpService _ntpService;
    private readonly DispatcherTimer _displayTimer;
    private readonly DispatcherTimer _syncTimer;

    private TimeSpan _ntpOffset = TimeSpan.Zero;

    private string _dateText = "";
    private string _timeText = "";
    private double _hourAngle;
    private double _minuteAngle;
    private double _secondAngle;
    private bool _isSynced;

    public AtomicClockViewModel(NtpService ntpService)
    {
        _ntpService = ntpService;

        _displayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _displayTimer.Tick += DisplayTimer_Tick;

        _syncTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(10)
        };
        _syncTimer.Tick += async (s, e) => await SyncNtpAsync();

        UpdateDisplay();
        _displayTimer.Start();
        _syncTimer.Start();

        _ = SyncNtpAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DateText
    {
        get => _dateText;
        private set => SetProperty(ref _dateText, value);
    }

    public string TimeText
    {
        get => _timeText;
        private set => SetProperty(ref _timeText, value);
    }

    public double HourAngle
    {
        get => _hourAngle;
        private set => SetProperty(ref _hourAngle, value);
    }

    public double MinuteAngle
    {
        get => _minuteAngle;
        private set => SetProperty(ref _minuteAngle, value);
    }

    public double SecondAngle
    {
        get => _secondAngle;
        private set => SetProperty(ref _secondAngle, value);
    }

    public bool IsSynced
    {
        get => _isSynced;
        private set => SetProperty(ref _isSynced, value);
    }

    public void StopTimers()
    {
        _displayTimer.Stop();
        _syncTimer.Stop();
    }

    private DateTime CurrentTime => DateTime.Now + _ntpOffset;

    private void DisplayTimer_Tick(object? sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        DateTime now = CurrentTime;

        DateText = now.ToString("ddd, M, dd, yyyy");
        TimeText = now.ToString("h:mm:ss tt");

        double hours = now.Hour % 12;
        double minutes = now.Minute;
        double seconds = now.Second + (now.Millisecond / 1000.0);

        HourAngle = (hours * 30.0) + (minutes * 0.5);
        MinuteAngle = (minutes * 6.0) + (seconds * 0.1);
        SecondAngle = seconds * 6.0;
    }

    private async Task SyncNtpAsync()
    {
        try
        {
            NtpResult result = await _ntpService.GetNetworkTimeAsync();
            if (result.Success)
            {
                _ntpOffset = result.Offset;
                IsSynced = true;
            }
            else
            {
                IsSynced = false;
            }
        }
        catch
        {
            IsSynced = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
