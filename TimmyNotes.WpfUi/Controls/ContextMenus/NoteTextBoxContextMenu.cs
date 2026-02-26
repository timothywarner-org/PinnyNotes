using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

using PinnyNotes.Core.Enums;
using PinnyNotes.WpfUi.Commands;
using PinnyNotes.WpfUi.Tools;

namespace PinnyNotes.WpfUi.Controls.ContextMenus;

public class NoteTextBoxContextMenu : ContextMenu
{
    private readonly NoteTextBoxControl _noteTextBox;

    private readonly ITool[] _tools;

    private readonly MenuItem _copyMenuItem;
    private readonly MenuItem _cutMenuItem;
    private readonly MenuItem _pasteMenuItem;
    private readonly MenuItem _selectAllMenuItem;
    private readonly MenuItem _clearMenuItem;
    private readonly MenuItem _lockedMenuItem;
    private readonly MenuItem _formatMenuItem;
    private readonly MenuItem _countsMenuItem;
    private readonly MenuItem _lineCountMenuItem;
    private readonly MenuItem _wordCountMenuItem;
    private readonly MenuItem _charCountMenuItem;
    private readonly Separator _toolsSeparator = new();
    private readonly MenuItem _toolsMenuItem;

    private readonly List<Control> _spellingErrorMenuItems = [];
    private readonly List<Control> _toolMenuItems = [];

    public NoteTextBoxContextMenu(NoteTextBoxControl noteTextBox)
    {
        _noteTextBox = noteTextBox;

        _tools = [
            new Base64Tool(_noteTextBox),
            new BracketTool(_noteTextBox),
            new CaseTool(_noteTextBox),
            new ColourTool(_noteTextBox),
            new DateTimeTool(_noteTextBox),
            new GibberishTool(_noteTextBox),
            new GuidTool(_noteTextBox),
            new HashTool(_noteTextBox),
            new HtmlEntityTool(_noteTextBox),
            new IndentTool(_noteTextBox),
            new JoinTool(_noteTextBox),
            new JsonTool(_noteTextBox),
            new ListTool(_noteTextBox),
            new QuoteTool(_noteTextBox),
            new RemoveTool(_noteTextBox),
            new SlashTool(_noteTextBox),
            new SortTool(_noteTextBox),
            new SplitTool(_noteTextBox),
            new TrimTool(_noteTextBox),
            new UrlTool(_noteTextBox)
        ];

        _copyMenuItem = new()
        {
            Header = "Copy",
            Command = _noteTextBox.CopyCommand,
            InputGestureText = "Ctrl+C"
        };
        _cutMenuItem = new()
        {
            Header = "Cut",
            Command = _noteTextBox.CutCommand,
            InputGestureText = "Ctrl+X"
        };
        _pasteMenuItem = new()
        {
            Header = "Paste",
            Command = _noteTextBox.PasteCommand,
            InputGestureText = "Ctrl+V"
        };

        _selectAllMenuItem = new()
        {
            Header = "Select All",
            Command = new RelayCommand(_noteTextBox.SelectAll)
        };
        _clearMenuItem = new()
        {
            Header = "Clear",
            Command = _noteTextBox.ClearCommand
        };

        _lockedMenuItem = new()
        {
            Header = "Locked",
            IsCheckable = true,
            Command = _noteTextBox.SetReadOnlyCommand,
            CommandParameter = true
        };
        _lockedMenuItem.SetBinding(
            MenuItem.CommandParameterProperty,
            new Binding(nameof(MenuItem.IsChecked))
            {
                Source = _lockedMenuItem
            }
        );

        _formatMenuItem = BuildFormatMenu();

        _countsMenuItem = new()
        {
            Header = "Counts"
        };
        _lineCountMenuItem = new()
        {
            IsEnabled = false
        };
        _wordCountMenuItem = new()
        {
            IsEnabled = false
        };
        _charCountMenuItem = new()
        {
            IsEnabled = false
        };

        _toolsMenuItem = new()
        {
            Header = "Tools"
        };

        Populate();
    }

    public void Update()
    {
        bool hasText = (_noteTextBox.GetPlainText().Length > 0);

        UpdateSpellingErrorMenuItems();

        _copyMenuItem.IsEnabled = _noteTextBox.HasSelectedText;
        _cutMenuItem.IsEnabled = _noteTextBox.HasSelectedText;
        _pasteMenuItem.IsEnabled = Clipboard.ContainsText();

        _selectAllMenuItem.IsEnabled = hasText;
        _clearMenuItem.IsEnabled = hasText;

        _lineCountMenuItem.Header = $"Lines: {_noteTextBox.LineCount()}";
        _wordCountMenuItem.Header = $"Words: {_noteTextBox.WordCount()}";
        _charCountMenuItem.Header = $"Chars: {_noteTextBox.CharCount()}";

        UpdateToolContextMenus();
    }

    private void Populate()
    {
        Items.Add(_copyMenuItem);
        Items.Add(_cutMenuItem);
        Items.Add(_pasteMenuItem);

        Items.Add(new Separator());

        Items.Add(_selectAllMenuItem);
        Items.Add(_clearMenuItem);

        Items.Add(new Separator());

        Items.Add(_lockedMenuItem);

        Items.Add(new Separator());

        Items.Add(_formatMenuItem);

        Items.Add(new Separator());

        _countsMenuItem.Items.Add(_lineCountMenuItem);
        _countsMenuItem.Items.Add(_wordCountMenuItem);
        _countsMenuItem.Items.Add(_charCountMenuItem);
        Items.Add(_countsMenuItem);
    }

    private MenuItem BuildFormatMenu()
    {
        MenuItem formatMenu = new() { Header = "Format" };

        // Font submenu
        MenuItem fontMenu = new() { Header = "Font" };
        string[] fonts = ["Segoe UI", "Arial", "Calibri", "Consolas", "Courier New", "Times New Roman"];
        foreach (string font in fonts)
        {
            fontMenu.Items.Add(new MenuItem
            {
                Header = font,
                Command = new RelayCommand(() => _noteTextBox.ApplyFontFamily(font))
            });
        }
        formatMenu.Items.Add(fontMenu);

        // Size submenu
        MenuItem sizeMenu = new() { Header = "Size" };
        double[] sizes = [8, 10, 12, 14, 16, 18, 20, 24, 28, 36];
        foreach (double size in sizes)
        {
            sizeMenu.Items.Add(new MenuItem
            {
                Header = size.ToString(),
                Command = new RelayCommand(() => _noteTextBox.ApplyFontSize(size))
            });
        }
        formatMenu.Items.Add(sizeMenu);

        // Color submenu
        MenuItem colorMenu = new() { Header = "Color" };
        (string Name, Color Color)[] colors = [
            ("Black", Colors.Black),
            ("Red", Colors.Red),
            ("Blue", Colors.Blue),
            ("Green", Colors.Green),
            ("Orange", Colors.Orange),
            ("Purple", Colors.Purple),
            ("Brown", Colors.Brown),
            ("Gray", Colors.Gray)
        ];
        foreach (var (name, color) in colors)
        {
            colorMenu.Items.Add(new MenuItem
            {
                Header = name,
                Command = new RelayCommand(() => _noteTextBox.ApplyForeground(color))
            });
        }
        formatMenu.Items.Add(colorMenu);

        formatMenu.Items.Add(new Separator());

        formatMenu.Items.Add(new MenuItem
        {
            Header = "Bold",
            InputGestureText = "Ctrl+B",
            Command = new RelayCommand(() => _noteTextBox.ToggleBold())
        });
        formatMenu.Items.Add(new MenuItem
        {
            Header = "Italic",
            InputGestureText = "Ctrl+I",
            Command = new RelayCommand(() => _noteTextBox.ToggleItalic())
        });
        formatMenu.Items.Add(new MenuItem
        {
            Header = "Underline",
            InputGestureText = "Ctrl+U",
            Command = new RelayCommand(() => _noteTextBox.ToggleUnderline())
        });

        formatMenu.Items.Add(new Separator());

        formatMenu.Items.Add(new MenuItem
        {
            Header = "Clear Formatting",
            Command = new RelayCommand(() => _noteTextBox.ClearFormatting())
        });

        return formatMenu;
    }

    private void UpdateSpellingErrorMenuItems()
    {
        foreach (Control spellingErrorMenuItem in _spellingErrorMenuItems)
            Items.Remove(spellingErrorMenuItem);

        _spellingErrorMenuItems.Clear();

        SpellingError? spellingError = _noteTextBox.GetSpellingError(_noteTextBox.CaretPosition);
        if (spellingError != null)
        {
            if (!spellingError.Suggestions.Any())
                _spellingErrorMenuItems.Add(
                    new MenuItem()
                    {
                        Header = "(no spelling suggestions)",
                        IsEnabled = false
                    }
                );
            else
                foreach (string spellingSuggestion in spellingError.Suggestions)
                {
                    _spellingErrorMenuItems.Add(
                        new MenuItem()
                        {
                            Header = spellingSuggestion,
                            FontWeight = FontWeights.Bold,
                            Command = EditingCommands.CorrectSpellingError,
                            CommandParameter = spellingSuggestion,
                            CommandTarget = _noteTextBox
                        }
                    );
                }
            _spellingErrorMenuItems.Add(new Separator());
        }

        _spellingErrorMenuItems.Reverse();
        foreach (Control spellingErrorMenuItem in _spellingErrorMenuItems)
            Items.Insert(0, spellingErrorMenuItem);
    }

    private void UpdateToolContextMenus()
    {
        foreach (Control toolMenuItem in _toolMenuItems)
            Items.Remove(toolMenuItem);
        _toolsMenuItem.Items.Clear();

        _toolMenuItems.Clear();

        IEnumerable<ITool> activeTools = _tools.Where(t => t.State != ToolState.Disabled);
        if (!activeTools.Any())
            return;

        _toolMenuItems.Add(_toolsSeparator);

        bool hasEnabledTools = false;
        foreach (ITool tool in activeTools)
        {
            switch (tool.State)
            {
                case ToolState.Favourite:
                    _toolMenuItems.Add(tool.MenuItem);
                    break;
                case ToolState.Enabled:
                    _toolsMenuItem.Items.Add(tool.MenuItem);
                    hasEnabledTools = true;
                    break;
            }
        }

        if (hasEnabledTools)
            _toolMenuItems.Add(_toolsMenuItem);

        foreach (Control toolMenuItem in _toolMenuItems)
            Items.Add(toolMenuItem);
    }
}
