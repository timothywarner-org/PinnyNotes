using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;

using TimmyNotes.Core;
using TimmyNotes.Core.Configurations;
using TimmyNotes.Core.Enums;
using TimmyNotes.Core.Repositories;
using TimmyNotes.WpfUi.Helpers;
using TimmyNotes.WpfUi.Messages;
using TimmyNotes.WpfUi.Models;
using TimmyNotes.WpfUi.Services;
using TimmyNotes.WpfUi.ViewModels;
using TimmyNotes.WpfUi.Views;

namespace TimmyNotes.WpfUi;

public partial class App : Application
{
#if DEBUG
    public const bool IsDebugMode = true;
#else
    public const bool IsDebugMode = false;
#endif

    private const string UniqueEventName = (IsDebugMode) ? "176fc692-28c2-4ed0-ba64-60fbd7165018" : "b1bc1a95-e142-4031-a239-dd0e14568a3c";
    private const string UniqueMutexName = (IsDebugMode) ? "e21c6456-5a11-4f37-a08d-83661b642abe" : "a46c6290-525a-40d8-9880-c95d35a49057";

    private Mutex _mutex = null!;

    private AppMetadataService _appMetadataService = null!;
    private SettingsService _settingsService = null!;
    private NotifyIconService _notifyIconService = null!;
    private DatabaseBackupService _databaseBackupService = null!;

    private EventWaitHandle _eventWaitHandle = null!;

    private ApplicationSettingsModel _applicationSettings = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // _mutex is required to keep it in memory, using _ = new Mutex() will not work as garbage collector will dispose of it eventually.
        _mutex = new Mutex(true, UniqueMutexName, out bool createdNew);
        _eventWaitHandle = new(false, EventResetMode.AutoReset, UniqueEventName);

        if (!createdNew)
        {
            _eventWaitHandle.Set();
            Shutdown();
            return;
        }

        try
        {
            base.OnStartup(e);

            ServiceCollection services = new();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            DatabaseConfiguration databaseConfiguration = Services.GetRequiredService<DatabaseConfiguration>();
            await DatabaseInitialiser.Initialise(databaseConfiguration.ConnectionString);

            _databaseBackupService = Services.GetRequiredService<DatabaseBackupService>();
            _databaseBackupService.Start();

            _settingsService = Services.GetRequiredService<SettingsService>();
            await _settingsService.Load();
            _applicationSettings = _settingsService.ApplicationSettings;
            _applicationSettings.PropertyChanged += OnApplicationSettingsChanged;

            _appMetadataService = Services.GetRequiredService<AppMetadataService>();
            await _appMetadataService.Load();
            _ = Services.GetRequiredService<WindowService>();
            _notifyIconService = Services.GetRequiredService<NotifyIconService>();

            MessengerService messengerService = Services.GetRequiredService<MessengerService>();
            messengerService.Subscribe<ApplicationActionMessage>(OnApplicationActionMessage);

            // Spawn a thread which will be waiting for our event
            Thread thread = new(
                () => {
                    while (_eventWaitHandle.WaitOne())
                        Current.Dispatcher.BeginInvoke(
                            () => messengerService.Publish(new ApplicationActionMessage(ApplicationAction.NewInstance))
                        );
                }
            )
            {
                // It is important mark it as background otherwise it will prevent app from exiting.
                IsBackground = true
            };

            thread.Start();

            messengerService.Publish(new ApplicationActionMessage(ApplicationAction.Start));

            ShutdownMode = (_applicationSettings.ShowNotifyIcon) ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnLastWindowClose;

            if (_settingsService.ApplicationSettings.CheckForUpdates)
            {
                DateTimeOffset date = DateTimeOffset.UtcNow;
                if (await VersionHelper.CheckForNewRelease(_appMetadataService.Metadata.LastUpdateCheck, date))
                    _appMetadataService.Metadata.LastUpdateCheck = date.ToUnixTimeSeconds();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Timmy Notes failed to start:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Shutdown();
        }
    }

    public static IServiceProvider Services { get; private set; } = null!;

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DatabaseConfiguration>();

        services.AddSingleton<SettingsRepository>();
        services.AddSingleton<AppMetadataRepository>();
        services.AddSingleton<NoteRepository>();

        services.AddSingleton<AppMetadataService>();
        services.AddSingleton<SettingsService>();

        services.AddSingleton<MessengerService>();
        services.AddSingleton<WindowService>();
        services.AddTransient<NotifyIconService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<DatabaseBackupService>();

        services.AddTransient<SettingsWindow>();
        services.AddTransient<SettingsViewModel>();

        services.AddTransient<ManagementWindow>();
        services.AddTransient<ManagementViewModel>();

        services.AddTransient<BreakTimerWindow>();
        services.AddTransient<BreakTimerViewModel>();

        services.AddSingleton<NtpService>();
        services.AddTransient<AtomicClockWindow>();
        services.AddTransient<AtomicClockViewModel>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            WindowService windowService = Services.GetRequiredService<WindowService>();
            await windowService.SaveAllOpenNotes();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save notes on exit: {ex.Message}");
        }

        _databaseBackupService?.Stop();
        if (_appMetadataService != null) await _appMetadataService.Save();
        if (_settingsService != null) await _settingsService.Save();
        _notifyIconService?.Dispose();

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        _eventWaitHandle?.Dispose();

        base.OnExit(e);
    }

    private void OnApplicationActionMessage(ApplicationActionMessage message)
    {
        if (message.Action == ApplicationAction.Close)
            Shutdown();
    }

    private void OnApplicationSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApplicationSettingsModel.ShowNotifyIcon))
            ShutdownMode = (_applicationSettings.ShowNotifyIcon) ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnLastWindowClose;
    }
}
