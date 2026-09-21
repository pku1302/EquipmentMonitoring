using LiveChartsCore.SkiaSharpView;
using System.Windows.Controls;
using System.Windows.Input;

namespace EquipmentMonitoring.Views.Chart;

public sealed class ChartRangeChangedEventArgs : EventArgs
{
    public double MinLimit { get; }
    public double MaxLimit { get; }

    public ChartRangeChangedEventArgs(
        double minLimit,
        double maxLimit)
    {
        MinLimit = minLimit;
        MaxLimit = maxLimit;
    }
}

public partial class SensorChartView : UserControl
{
    public event EventHandler<ChartRangeChangedEventArgs>? RangeChanged;
    private bool _isDragging;

    public SensorChartView()
    {
        InitializeComponent();
    }
    private void Chart_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        _isDragging = true;
    }

    private void Chart_PreviewMouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        if (!_isDragging)
            return;

        _isDragging = false;

        Dispatcher.BeginInvoke(() =>
        {
            NotifyRangeChanged();
        });
    }
    private void NotifyRangeChanged()
    {
        if (Chart.XAxes.FirstOrDefault() is not Axis axis)
            return;

        if (axis.MinLimit is null ||
            axis.MaxLimit is null)
            return;

        RangeChanged?.Invoke(
            this,
            new ChartRangeChangedEventArgs(
                axis.MinLimit.Value,
                axis.MaxLimit.Value));
    }

}
