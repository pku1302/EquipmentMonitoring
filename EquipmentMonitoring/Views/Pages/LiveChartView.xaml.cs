using EquipmentMonitoring.ViewModels.Pages;
using EquipmentMonitoring.Views.Chart;
using System.Windows;
using System.Windows.Controls;

namespace EquipmentMonitoring.Views.Pages;

public partial class LiveChartView : UserControl
{
    public LiveChartView()
    {
        InitializeComponent();
    }

    private void SensorChart_RangeChanged(
        object? sender,
        ChartRangeChangedEventArgs e)
    {
        if (DataContext is not LiveChartViewModel vm)
            return;

        vm.IsAutoFollow = false;

        vm.SyncVisibleRange(
            e.MinLimit,
            e.MaxLimit);
    }
}
