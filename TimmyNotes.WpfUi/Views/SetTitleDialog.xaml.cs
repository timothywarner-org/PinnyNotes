using System.Windows;

namespace TimmyNotes.WpfUi.Views;

public partial class SetTitleDialog : Window
{
    public string? NoteTitle => string.IsNullOrWhiteSpace(TitleTextBox.Text)
        ? null
        : TitleTextBox.Text.Trim();

    public SetTitleDialog(string? currentTitle)
    {
        InitializeComponent();
        TitleTextBox.Text = currentTitle ?? "";
        TitleTextBox.SelectAll();
        TitleTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
