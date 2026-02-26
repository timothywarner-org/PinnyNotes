# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimmyNotes is a WPF sticky notes application for Windows, forked from [63BeetleSmurf/TimmyNotes](https://github.com/63BeetleSmurf/TimmyNotes). The goal is to replicate and extend functionality inspired by Zhorn Software's Stickies.

## Build & Run

```bash
# Build the solution
dotnet build TimmyNotes.sln

# Run the app
dotnet run --project TimmyNotes.WpfUi

# Build release
dotnet build TimmyNotes.sln -c Release
```

Target: .NET 10.0 (WPF, Windows-only). The solution has three projects but only two build via `dotnet`: **TimmyNotes.Core** (class library) and **TimmyNotes.WpfUi** (WPF exe). TimmyNotes.Setup is a Visual Studio Installer project (.vdproj) and does not build from CLI.

## Architecture

**MVVM with Pub/Sub Messaging** — Views bind to ViewModels, ViewModels communicate via `MessengerService` (publish/subscribe with typed messages), all services registered through Microsoft.Extensions.DependencyInjection in `App.xaml.cs`.

### Project Layout

- **TimmyNotes.Core** — Data layer. SQLite database via Microsoft.Data.Sqlite, repository pattern, DTOs (record types), enums, and schema migrations.
- **TimmyNotes.WpfUi** — UI layer. Views (WPF windows/controls), ViewModels, Models (INotifyPropertyChanged), Services, Tools, Commands, Interop (Win32 API calls).

### Key Architectural Flows

**Note Lifecycle:** `WindowService` creates `NoteWindow` → `NoteViewModel.Initialize()` creates/loads `NoteModel` → auto-save via `DispatcherTimer` (5s interval) → `CloseNote()` saves or deletes if empty. State changes publish `NoteActionMessage`.

**Tools System:** 20 text transformation tools inherit from `BaseTool` (Template Method pattern). Each tool builds its own `MenuItem` tree. Tools are instantiated in `NoteTextBoxContextMenu` and filtered by `ToolState` (Disabled/Enabled/Favourite) from settings. To add a new tool: create a class extending `BaseTool`, implement menu items, and register it in the `NoteTextBoxContextMenu` constructor's tools array.

**Database Migrations:** `DatabaseInitializer` runs sequential migrations (`SchemaMigration` subclasses) from current version to target. Current schema version: 5. Migrations live in `TimmyNotes.Core/Migrations/`.

**Settings:** Four model objects (`ApplicationSettingsModel`, `NoteSettingsModel`, `EditorSettingsModel`, `ToolSettingsModel`) loaded from DB via `SettingsService` on startup, saved on exit.

**Window Management:** `WindowService` tracks open windows by NoteId, prevents duplicates, manages singleton settings/management windows. Inter-component communication uses `MessengerService` message types (records in `Messages/`).

### Service Registration (App.xaml.cs)

- **Singletons:** DatabaseConfiguration, all Repositories, AppMetadataService, SettingsService, MessengerService, WindowService, ThemeService
- **Transients:** NotifyIconService, SettingsWindow/ViewModel, ManagementWindow/ViewModel

## Coding Conventions

- **No `var`** — explicit types enforced via .editorconfig (`csharp_style_var_*: false:warning`)
- **British English spelling** — `Colour`, `Initialise`, etc. (spelling_languages = en-gb in .editorconfig)
- **File-scoped namespaces** preferred
- **Allman brace style** (braces on new lines)
- 4-space indentation, CRLF line endings
- PascalCase for types/members, `I` prefix for interfaces
- DTOs use C# `record` types for immutability
- Models use `BaseModel.SetProperty<T>()` which auto-sets `IsSaved = false`
- Commands use `RelayCommand`/`RelayCommand<T>`

## Database

SQLite stored at `%APPDATA%/Pinny Notes/pinny_notes.sqlite` (production) or exe directory (debug/portable mode, triggered by `portable.txt` marker file). Connection management opens/closes per operation.

## Single Instance

App uses a named Mutex (different GUIDs for Debug vs Release) and EventWaitHandle for inter-process communication. A second instance signals the first and exits.

## Win32 Interop

`Interop/` folder contains P/Invoke wrappers (User32) and `ScreenHelper` for multi-monitor support, always-on-top behavior, and taskbar/task-switcher visibility control.
