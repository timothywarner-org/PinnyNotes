using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

using TimmyTools.WpfUi.Commands;

namespace TimmyTools.WpfUi.ViewModels;

public class BreakTimerViewModel : INotifyPropertyChanged
{
    private enum TimerState
    {
        Idle,
        Running,
        Paused,
        Completed
    }

    private readonly DispatcherTimer _timer;

    private TimerState _state = TimerState.Idle;
    private DateTime _endTime;
    private TimeSpan _totalDuration;
    private TimeSpan _remainingOnPause;

    private string _timeRemainingText = "00:00";
    private string _verboseTimeText = "";
    private double _progressFraction;
    private int _customMinutes = 5;

    public BreakTimerViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _timer.Tick += Timer_Tick;

        StartPresetCommand = new RelayCommand<int>(StartPreset);
        StartCustomCommand = new RelayCommand(StartCustom, () => CustomMinutes > 0);
        PauseCommand = new RelayCommand(Pause);
        ResumeCommand = new RelayCommand(Resume);
        ResetCommand = new RelayCommand(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string TimeRemainingText
    {
        get => _timeRemainingText;
        private set => SetProperty(ref _timeRemainingText, value);
    }

    public string VerboseTimeText
    {
        get => _verboseTimeText;
        private set => SetProperty(ref _verboseTimeText, value);
    }

    public double ProgressFraction
    {
        get => _progressFraction;
        private set => SetProperty(ref _progressFraction, value);
    }

    public int CustomMinutes
    {
        get => _customMinutes;
        set
        {
            if (SetProperty(ref _customMinutes, value))
                ((RelayCommand)StartCustomCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsIdle
    {
        get => _state == TimerState.Idle;
        private set => OnPropertyChanged();
    }

    public bool IsRunning
    {
        get => _state == TimerState.Running;
        private set => OnPropertyChanged();
    }

    public bool IsPaused
    {
        get => _state == TimerState.Paused;
        private set => OnPropertyChanged();
    }

    public bool IsCompleted
    {
        get => _state == TimerState.Completed;
        private set => OnPropertyChanged();
    }

    public bool IsActive
    {
        get => _state != TimerState.Idle;
        private set => OnPropertyChanged();
    }

    public ICommand StartPresetCommand { get; }
    public ICommand StartCustomCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand ResetCommand { get; }

    private void StartPreset(int minutes)
    {
        StartTimer(TimeSpan.FromMinutes(minutes));
    }

    private void StartCustom()
    {
        if (CustomMinutes > 0)
            StartTimer(TimeSpan.FromMinutes(CustomMinutes));
    }

    private void StartTimer(TimeSpan duration)
    {
        _totalDuration = duration;
        _endTime = DateTime.Now + duration;

        UpdateDisplay(duration);
        SetState(TimerState.Running);

        _timer.Start();
    }

    private void Pause()
    {
        _timer.Stop();
        _remainingOnPause = _endTime - DateTime.Now;
        if (_remainingOnPause < TimeSpan.Zero)
            _remainingOnPause = TimeSpan.Zero;

        SetState(TimerState.Paused);
    }

    private void Resume()
    {
        _endTime = DateTime.Now + _remainingOnPause;
        SetState(TimerState.Running);
        _timer.Start();
    }

    private void Reset()
    {
        _timer.Stop();
        _timeRemainingText = "00:00";
        _verboseTimeText = "";
        _progressFraction = 0;

        SetState(TimerState.Idle);

        OnPropertyChanged(nameof(TimeRemainingText));
        OnPropertyChanged(nameof(VerboseTimeText));
        OnPropertyChanged(nameof(ProgressFraction));
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        TimeSpan remaining = _endTime - DateTime.Now;

        if (remaining <= TimeSpan.Zero)
        {
            _timer.Stop();
            UpdateDisplay(TimeSpan.Zero);
            ProgressFraction = 1.0;
            SetState(TimerState.Completed);
            return;
        }

        UpdateDisplay(remaining);

        double elapsed = _totalDuration.TotalSeconds - remaining.TotalSeconds;
        ProgressFraction = Math.Min(1.0, elapsed / _totalDuration.TotalSeconds);
    }

    private void UpdateDisplay(TimeSpan remaining)
    {
        if (remaining.TotalHours >= 1)
            TimeRemainingText = $"{(int)remaining.TotalHours}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        else
            TimeRemainingText = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";

        int displayMinutes = remaining.Minutes + ((int)remaining.TotalHours * 60);
        int displaySeconds = remaining.Seconds;

        if (remaining.TotalHours >= 1)
        {
            int hours = (int)remaining.TotalHours;
            int minutes = remaining.Minutes;
            VerboseTimeText = $"{hours} {(hours == 1 ? "hour" : "hours")} {minutes} {(minutes == 1 ? "minute" : "minutes")} {displaySeconds} {(displaySeconds == 1 ? "second" : "seconds")}";
        }
        else
        {
            VerboseTimeText = $"{displayMinutes} {(displayMinutes == 1 ? "minute" : "minutes")} {displaySeconds} {(displaySeconds == 1 ? "second" : "seconds")}";
        }
    }

    private void SetState(TimerState state)
    {
        _state = state;
        OnPropertyChanged(nameof(IsIdle));
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsPaused));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsActive));
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
