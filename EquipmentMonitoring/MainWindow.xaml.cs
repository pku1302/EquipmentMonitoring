using System.Windows;
using EquipmentMonitoring.ViewModels;

namespace EquipmentMonitoring
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}