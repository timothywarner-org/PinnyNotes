using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using PinnyNotes.WpfUi.Models;
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

        NotesListView.MouseUp += NotesListView_MouseUp;
        NotesListView.MouseDoubleClick += NotesListView_MouseDoubleClick;
    }

    private void NotesListView_MouseUp(object sender, MouseButtonEventArgs e)
    {
        HitTestResult hitTestResult = VisualTreeHelper.HitTest(NotesListView, e.GetPosition(NotesListView));
        FrameworkElement? item = hitTestResult?.VisualHit as FrameworkElement;
        if (item?.DataContext is not NotePreviewModel)
            NotesListView.UnselectAll();
    }

    private void NotesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (NotesListView.SelectedItems.Count == 0)
            return;

        _viewModel.OpenNotesCommand.Execute(null);
    }
}
