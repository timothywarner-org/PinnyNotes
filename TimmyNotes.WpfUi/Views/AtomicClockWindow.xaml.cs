using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

using TimmyNotes.WpfUi.ViewModels;

namespace TimmyNotes.WpfUi.Views;

public partial class AtomicClockWindow : Window
{
    private readonly AtomicClockViewModel _viewModel;

    public AtomicClockWindow(AtomicClockViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;

        InitializeComponent();

        DrawTickMarks();

        Closed += AtomicClockWindow_Closed;
    }

    private void AtomicClockWindow_Closed(object? sender, EventArgs e)
    {
        _viewModel.StopTimers();
    }

    private void DrawTickMarks()
    {
        double centerX = 170;
        double centerY = 170;
        double outerRadius = 155;
        SolidColorBrush tickBrush = new(Color.FromRgb(0x00, 0xAC, 0xC1));

        for (int i = 0; i < 60; i++)
        {
            double angle = i * 6.0 * Math.PI / 180.0;
            bool isHourMark = i % 5 == 0;
            double innerRadius = isHourMark ? 140 : 148;
            double thickness = isHourMark ? 2.5 : 1.0;

            double x1 = centerX + innerRadius * Math.Sin(angle);
            double y1 = centerY - innerRadius * Math.Cos(angle);
            double x2 = centerX + outerRadius * Math.Sin(angle);
            double y2 = centerY - outerRadius * Math.Cos(angle);

            Line tick = new()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = tickBrush,
                StrokeThickness = thickness
            };

            TickCanvas.Children.Add(tick);
        }
    }
}
