using Microsoft.Extensions.DependencyInjection;

using PinnyNotes.Core.Enums;
using PinnyNotes.Core.Repositories;
using PinnyNotes.WpfUi.Messages;
using PinnyNotes.WpfUi.Models;
using PinnyNotes.WpfUi.ViewModels;
using PinnyNotes.WpfUi.Views;

namespace PinnyNotes.WpfUi.Services;

public class WindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MessengerService _messengerService;
    private readonly AppMetadataService _appMetadataService;
    private readonly SettingsService _settingsService;
    private readonly NoteRepository _noteRepository;
    private readonly ThemeService _themeService;

    private readonly Dictionary<int, NoteWindow> _openNoteWindows = [];
    private SettingsWindow? _settingsWindow;
    private ManagementWindow? _managementWindow;

    public WindowService(
        IServiceProvider serviceProvider,
        MessengerService messengerService,
        AppMetadataService appMetadataService,
        SettingsService settingsService,
        NoteRepository noteRepository,
        ThemeService themeService
    )
    {
        _serviceProvider = serviceProvider;
        _messengerService = messengerService;
        _appMetadataService = appMetadataService;
        _settingsService = settingsService;
        _noteRepository = noteRepository;
        _themeService = themeService;

        _messengerService.Subscribe<ApplicationActionMessage>(OnApplicationActionMessage);

        _messengerService.Subscribe<OpenNoteWindowMessage>(OnOpenNoteWindowMessage);
        _messengerService.Subscribe<MultipleNoteWindowActionMessage>(OnMultipleNoteWindowActionMessage);
        _messengerService.Subscribe<NoteActionMessage>(OnNoteActionMessage);

        _messengerService.Subscribe<OpenSettingsWindowMessage>(OnOpenSettingsWindowMessage);

        _messengerService.Subscribe<OpenManagementWindowMessage>(OnOpenManagementWindowMessage);
    }

    private void OnApplicationActionMessage(ApplicationActionMessage message)
    {
        // TO DO: Add settings for this behaviour.
        switch (message.Action)
        {
            case ApplicationActions.Start:
                OpenManagementWindow();
                break;
            case ApplicationActions.NewInstance:
                _ = OpenNoteWindow();
                break;
        }
    }

    private void OnOpenNoteWindowMessage(OpenNoteWindowMessage message)
        => _ = OpenNoteWindow(message.NoteId, message.ParentNote);

    private void OnMultipleNoteWindowActionMessage(MultipleNoteWindowActionMessage message)
    {
        foreach (int noteId in message.NoteIds)
        {
            switch (message.Action)
            {
                case NoteWindowActions.Open:
                    _ = OpenNoteWindow(noteId);
                    break;
                case NoteWindowActions.Close:
                    CloseNoteWindow(noteId);
                    break;
            }
        }
    }

    private void OnNoteActionMessage(NoteActionMessage message)
    {
        switch (message.Action)
        {
            case NoteActions.Closed:
                _openNoteWindows.Remove(message.NoteDto.Id);
                break;
        }
    }

    private void OnOpenSettingsWindowMessage(OpenSettingsWindowMessage message)
    {
        if (_settingsWindow == null || !_settingsWindow.IsLoaded)
        {
            _settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        }

        _settingsWindow.Owner = message.Owner;

        if (!_settingsWindow.IsVisible)
            _settingsWindow.Show();

        _settingsWindow.Activate();
    }

    private void OnOpenManagementWindowMessage(OpenManagementWindowMessage message)
        => OpenManagementWindow();

    private async Task OpenNoteWindow(int? noteId = null, NoteModel? parentNote = null)
    {
        NoteWindow window;

        if (noteId is not null && _openNoteWindows.TryGetValue((int)noteId, out NoteWindow? existingWindow))
        {
            window = existingWindow;
            window.Activate();
        }
        else
        {
            NoteViewModel viewModel = new(_noteRepository, _appMetadataService, _settingsService, _messengerService, _themeService);
            await viewModel.Initialize(noteId, parentNote);
            window = new(_settingsService, _messengerService, _themeService, viewModel);

            window.Show();

            _openNoteWindows[viewModel.Note.Id] = window;
        }
    }

    private void CloseNoteWindow(int noteId)
    {
        if (_openNoteWindows.TryGetValue(noteId, out NoteWindow? window) && window.IsLoaded)
        {
            window.Close();
            _openNoteWindows.Remove(noteId);
        }
    }

    private void OpenManagementWindow()
    {
        if (_managementWindow == null || !_managementWindow.IsLoaded)
        {
            _managementWindow = _serviceProvider.GetRequiredService<ManagementWindow>();
            _managementWindow.Closed += (s, e) => _settingsWindow = null;
        }

        if (!_managementWindow.IsVisible)
            _managementWindow.Show();

        _managementWindow.Activate();
    }
}
