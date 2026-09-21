using System.Windows;
using EquipmentMonitoring.ViewModels.Pages;

namespace EquipmentMonitoring
{
    public partial class MainWindow : Window
    {
        public MainWindow(
            MainViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}