using EquipmentMonitoring.ViewModels;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace EquipmentMonitoring.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void Chart_PreviewMouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            if (sender is not CartesianChart chart)
                return;

            DisableAutoFollow();

            Dispatcher.BeginInvoke(() =>
            {
                SyncCharts(chart);
            });
        }

        private void Chart_PreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DisableAutoFollow();
            }
        }

        private void Chart_PreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not CartesianChart chart)
                return;

            if (DataContext is DashboardViewModel vm)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    SyncCharts(chart);
                });
            }
        }

        private void DisableAutoFollow()
        {
            if (DataContext is DashboardViewModel viewModel)
            {
                viewModel.IsAutoFollow = false;
            }
        }

        private void SyncCharts(
            CartesianChart sourceChart)
        {
            if (DataContext is not DashboardViewModel viewModel)
                return;

            var axis = sourceChart.XAxes.First();

            if (axis.MinLimit is null ||
                axis.MaxLimit is null)
                return;

            viewModel.SyncVisibleRange(
                axis.MinLimit.Value,
                axis.MaxLimit.Value);
        }

    }
}
