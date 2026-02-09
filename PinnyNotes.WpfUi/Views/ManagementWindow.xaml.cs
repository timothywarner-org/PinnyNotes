using System.Windows;
using System.Windows.Input;

using PinnyNotes.WpfUi.ViewModels;

namespace PinnyNotes.WpfUi.Views;

public partial class ManagementWindow : Window
{
    private readonly ManagementViewModel _viewModel;

    public ManagementWindow(ManagementViewModel viewModel)
    {
        _viewModel = viewModel;

        DataContext = _viewModel;

        InitializeComponent();

        NotesListView.MouseDoubleClick += NotesListView_MouseDoubleClick;
    }

    private void NotesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (NotesListView.SelectedItems.Count == 0)
            return;

        _viewModel.OpenNotesCommand.Execute(null);
    }
}
