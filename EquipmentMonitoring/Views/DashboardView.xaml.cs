using EquipmentMonitoring.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace EquipmentMonitoring.Views
{
    /// <summary>
    /// DashboardView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void TemperatureChart_PreviewMouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            DisableAutoFollow();

            if (e.Delta < 0)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (DataContext is DashboardViewModel vm)
                    {
                        vm.TryEnableAutoFollow();
                    }
                });
            }
        }

        private void TemperatureChart_PreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DisableAutoFollow();
            }
        }

        private void TemperatureChart_PreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                vm.TryEnableAutoFollow();
            }
        }

        private void DisableAutoFollow()
        {
            if (DataContext is DashboardViewModel viewModel)
            {
                viewModel.IsAutoFollow = false;
            }
        }

    }
}
