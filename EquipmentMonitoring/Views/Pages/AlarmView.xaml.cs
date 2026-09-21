using EquipmentMonitoring.ViewModels.Pages;
using System.Windows;
using System.Windows.Controls;

namespace EquipmentMonitoring.Views.Pages;

/// <summary>
/// AlarmView.xaml에 대한 상호 작용 논리
/// </summary>
public partial class AlarmView : UserControl
{
    public AlarmView()
    {
        InitializeComponent();

        Loaded += AlarmView_Loaded;
    }

    private async void AlarmView_Loaded(
        object sender, RoutedEventArgs e)
    {
        if (DataContext is AlarmViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
