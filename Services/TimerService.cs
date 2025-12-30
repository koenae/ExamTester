namespace ExamTester.Services;

public class TimerService : IDisposable
{
    private System.Timers.Timer? _timer;
    private int _remainingSeconds;
    private int _totalSeconds;
    private bool _isRunning;

    public int RemainingSeconds => _remainingSeconds;
    public int TotalSeconds => _totalSeconds;
    public int ElapsedSeconds => _totalSeconds - _remainingSeconds;
    public bool IsRunning => _isRunning;
    public bool IsWarning => _remainingSeconds <= 300 && _remainingSeconds > 60;
    public bool IsCritical => _remainingSeconds <= 60;

    public string FormattedTime
    {
        get
        {
            var span = TimeSpan.FromSeconds(_remainingSeconds);
            return span.Hours > 0
                ? $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}"
                : $"{span.Minutes:D2}:{span.Seconds:D2}";
        }
    }

    public double ProgressPercentage => _totalSeconds > 0
        ? (double)(_totalSeconds - _remainingSeconds) / _totalSeconds * 100
        : 0;

    public event Action? OnTick;
    public event Action? OnTimeUp;

    public void Start(int minutes)
    {
        _totalSeconds = minutes * 60;
        _remainingSeconds = _totalSeconds;
        _isRunning = true;

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (sender, args) =>
        {
            if (_remainingSeconds > 0)
            {
                _remainingSeconds--;
                OnTick?.Invoke();

                if (_remainingSeconds == 0)
                {
                    Stop();
                    OnTimeUp?.Invoke();
                }
            }
        };
        _timer.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    public void Pause()
    {
        _isRunning = false;
        _timer?.Stop();
    }

    public void Resume()
    {
        if (_timer != null && _remainingSeconds > 0)
        {
            _isRunning = true;
            _timer.Start();
        }
    }

    public void Reset()
    {
        Stop();
        _remainingSeconds = 0;
        _totalSeconds = 0;
    }

    public void Dispose()
    {
        Stop();
    }
}
