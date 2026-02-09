using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using PinnyNotes.Core.DataTransferObjects;
using PinnyNotes.Core.Enums;
using PinnyNotes.Core.Repositories;
using PinnyNotes.WpfUi.Commands;
using PinnyNotes.WpfUi.Helpers;
using PinnyNotes.WpfUi.Interop;
using PinnyNotes.WpfUi.Interop.Constants;
using PinnyNotes.WpfUi.Messages;
using PinnyNotes.WpfUi.Models;
using PinnyNotes.WpfUi.Services;
using PinnyNotes.WpfUi.Themes;

namespace PinnyNotes.WpfUi.ViewModels;

public class NoteViewModel : BaseViewModel
{
    private readonly NoteRepository _noteRepository;
    private readonly ThemeService _themeService;

    private readonly DispatcherTimer _saveTimer;

    public RelayCommand<string> ChangeThemeColorCommand { get; }

    public NoteSettingsModel NoteSettings { get; set; }
    public EditorSettingsModel EditorSettings { get; set; }

    public NoteModel Note { get; set; } = null!;

    public NoteViewModel(
        NoteRepository noteRepository,
        AppMetadataService appMetadataService,
        SettingsService settingsService,
        MessengerService messengerService,
        ThemeService themeService
    ) : base(appMetadataService, settingsService, messengerService)
    {
        _noteRepository = noteRepository;
        _themeService = themeService;

        ChangeThemeColorCommand = new RelayCommand<string>(ChangeThemeColor);

        NoteSettings = SettingsService.NoteSettings;
        NoteSettings.PropertyChanged += OnNoteSettingsChanged;
        EditorSettings = SettingsService.EditorSettings;

        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _saveTimer.Tick += OnSaveTimerTick;
    }

    public async Task Initialize(int? noteId = null, NoteModel? parent = null)
    {
        if (noteId is null)
            await CreateNewNote(parent);
        else
            await LoadNote((int)noteId);
    }

    private async Task CreateNewNote(NoteModel? parent = null)
    {
        Note = new(NoteSettings, _themeService.GetNewNoteColorSchemeName(parent?.ThemeColorScheme));

        InitNotePosition(parent);
        UpdateBrushes();
        UpdateOpacity();

        Note.IsOpen = true;

        Note.Id = await _noteRepository.Create(Note.ToDto());

        Note.IsSaved = true;

        MessengerService.Publish<NoteActionMessage>(new(NoteActions.Created, Note.ToDto()));
    }

    private async Task LoadNote(int noteId)
    {
        Note = new(await _noteRepository.GetById((int)noteId), NoteSettings);

        UpdateBrushes();
        UpdateOpacity();

        Note.IsOpen = true;

        await SaveNote();
    }

    public void OnWindowLoaded(nint windowHandle)
    {
        Note.WindowHandle = windowHandle;
        UpdateVisibility();
        UpdateAlwaysOnTop();

        _saveTimer.Start();
    }

    public void OnWindowMoved(double left, double top)
    {
        // Reset gravity depending what position the note was moved to.
        // This does not effect the saved start up setting, only what
        // direction new child notes will go towards.
        Note.X = left;
        Note.Y = top;

        Rect screenBounds = ScreenHelper.GetCurrentScreenBounds(Note.WindowHandle);
        Note.GravityX = (left - screenBounds.X < screenBounds.Width / 2) ? 1 : -1;
        Note.GravityY = (top - screenBounds.Y < screenBounds.Height / 2) ? 1 : -1;
    }

    public void UpdateOpacity()
    {
        TransparencyModes transparentMode = NoteSettings.TransparencyMode;
        if (transparentMode == TransparencyModes.Disabled)
        {
            Note.Opacity = 1.0;
            return;
        }

        bool opaqueWhenFocused = NoteSettings.OpaqueWhenFocused;

        double opaqueOpacity = NoteSettings.OpaqueValue;
        double transparentOpacity = NoteSettings.TransparentValue;

        if ((opaqueWhenFocused && Note.IsFocused) || (transparentMode == TransparencyModes.WhenPinned && !Note.IsPinned))
            Note.Opacity = opaqueOpacity;
        else
            Note.Opacity = transparentOpacity;
    }

    public void UpdateAlwaysOnTop()
    {
        if (Note.WindowHandle == 0)
            return;

        nint hWndInsertAfter = (Note.IsFocused || Note.IsPinned) ? HWND.TOPMOST : HWND.NOTOPMOST;

        uint uFlags = SWP.NOMOVE | SWP.NOSIZE | SWP.NOACTIVATE;

        _ = User32.SetWindowPos(Note.WindowHandle, hWndInsertAfter, 0, 0, 0, 0, uFlags);
    }

    private async void OnSaveTimerTick(object? sender, EventArgs e)
    {
        await SaveNote();
    }

    private void InitNotePosition(NoteModel? parent = null)
    {
        int noteMargin = 45;

        Point position = new(0, 0);
        Rect screenBounds;

        if (parent != null)
        {
            screenBounds = ScreenHelper.GetCurrentScreenBounds(parent.WindowHandle);

            Note.GravityX = parent.GravityX;
            Note.GravityY = parent.GravityY;

            position.X = parent.X + (noteMargin * Note.GravityX);
            position.Y = parent.Y + (noteMargin * Note.GravityY);
        }
        else
        {
            int screenMargin = 78;
            screenBounds = ScreenHelper.GetPrimaryScreenBounds();

            switch (NoteSettings.StartupPosition)
            {
                case StartupPositions.TopLeft:
                case StartupPositions.MiddleLeft:
                case StartupPositions.BottomLeft:
                    position.X = screenMargin;
                    Note.GravityX = 1;
                    break;
                case StartupPositions.TopCenter:
                case StartupPositions.MiddleCenter:
                case StartupPositions.BottomCenter:
                    position.X = screenBounds.Width / 2 - Note.Width / 2;
                    Note.GravityX = 1;
                    break;
                case StartupPositions.TopRight:
                case StartupPositions.MiddleRight:
                case StartupPositions.BottomRight:
                    position.X = (screenBounds.Width - screenMargin) - Note.Width;
                    Note.GravityX = -1;
                    break;
            }

            switch (NoteSettings.StartupPosition)
            {
                case StartupPositions.TopLeft:
                case StartupPositions.TopCenter:
                case StartupPositions.TopRight:
                    position.Y = screenMargin;
                    Note.GravityY = 1;
                    break;
                case StartupPositions.MiddleLeft:
                case StartupPositions.MiddleCenter:
                case StartupPositions.MiddleRight:
                    position.Y = screenBounds.Height / 2 - Note.Height / 2;
                    Note.GravityY = -1;
                    break;
                case StartupPositions.BottomLeft:
                case StartupPositions.BottomCenter:
                case StartupPositions.BottomRight:
                    position.Y = (screenBounds.Height - screenMargin) - Note.Height;
                    Note.GravityY = -1;
                    break;
            }
        }

        // Apply noteMargin if another note is already in that position
        if (Application.Current.Windows.Count > 1)
        {
            Window[] otherWindows = new Window[Application.Current.Windows.Count];
            Application.Current.Windows.CopyTo(otherWindows, 0);
            while (otherWindows.Any(w => w.Left == position.X && w.Top == position.Y))
            {
                double newX = position.X + (noteMargin * Note.GravityX);
                if (newX < screenBounds.Left)
                    newX = screenBounds.Left;
                else if (newX + Note.Width > screenBounds.Right)
                    newX = screenBounds.Right - Note.Width;

                double newY = position.Y + (noteMargin * Note.GravityY);
                if (newY < screenBounds.Top)
                    newY = screenBounds.Top;
                else if (newY + Note.Height > screenBounds.Bottom)
                    newY = screenBounds.Bottom - Note.Height;

                if (position.X == newX && position.Y == newY)
                    break;

                position.X = newX;
                position.Y = newY;
            }
        }

        Note.X = position.X;
        Note.Y = position.Y;
    }

    private void UpdateBrushes()
    {
        AppMetadataService.Metadata.ColorScheme = Note.ThemeColorScheme;

        Palette palette = _themeService.GetPalette(Note.ThemeColorScheme, NoteSettings.ColorMode);
        Note.UpdateBrushes(palette);
    }

    private void ChangeThemeColor(string colorScheme)
    {
        Note.ThemeColorScheme = colorScheme;
        UpdateBrushes();
    }

    private void OnNoteSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(NoteSettingsModel.TransparencyMode):
            case nameof(NoteSettingsModel.OpaqueWhenFocused):
            case nameof(NoteSettingsModel.OpaqueValue):
            case nameof(NoteSettingsModel.TransparentValue):
                UpdateOpacity();
                break;
            case nameof(NoteSettingsModel.ColorMode):
                UpdateBrushes();
                break;
            case nameof(NoteSettingsModel.VisibilityMode):
                UpdateVisibility();
                break;
        }
    }

    private void UpdateVisibility()
    {
        if (Note.WindowHandle == 0)
            return;

        nint exStyle = User32.GetWindowLongPtrW(Note.WindowHandle, GWL.EXSTYLE);

        switch (NoteSettings.VisibilityMode)
        {
            default:
            case VisibilityModes.ShowInTaskbar:
                exStyle &= ~WS_EX.TOOLWINDOW;
                Note.ShowInTaskbar = true;
                break;
            case VisibilityModes.HideInTaskbar:
                exStyle &= ~WS_EX.TOOLWINDOW;
                Note.ShowInTaskbar = false;
                break;
            case VisibilityModes.HideInTaskbarAndTaskSwitcher:
                exStyle |= WS_EX.TOOLWINDOW;
                Note.ShowInTaskbar = false;
                break;
        }

        _ = User32.SetWindowLongPtrW(Note.WindowHandle, GWL.EXSTYLE, exStyle);
    }

    public async Task SaveNote()
    {
        if (Note.IsSaved)
            return;

        await _noteRepository.Update(
            Note.ToDto()
        );

        Note.IsSaved = true;

        MessengerService.Publish<NoteActionMessage>(new(NoteActions.Updated, Note.ToDto()));
    }

    public async Task<bool> CloseNote()
    {
        _saveTimer.Stop();

        MessengerService.Publish<NoteActionMessage>(new(NoteActions.Closed, Note.ToDto()));

        if (string.IsNullOrEmpty(Note.Content))
        {
            // Delete note if empty, TO DO: Add setting for this behaviour
            await _noteRepository.Delete(Note.Id);
            MessengerService.Publish<NoteActionMessage>(new(NoteActions.Deleted, Note.ToDto()));
            return false;
        }

        // IsOpen is updated when users closes note via close button,
        // it's left true if windows is closed by exiting app so it will re-open with app.
        await SaveNote();

        return false;
    }
}
