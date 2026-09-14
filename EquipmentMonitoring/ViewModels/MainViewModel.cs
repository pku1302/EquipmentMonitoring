using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Commands;
using EquipmentMonitoring.Services;
using System.Windows.Input;

namespace EquipmentMonitoring.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    private readonly DashboardViewModel _dashboardViewModel;
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly AlarmViewModel _alarmViewModel;
    private readonly LogViewModel _logViewModel;

    public MainViewModel(
        DashboardViewModel dashboardViewModel,
        EquipmentViewModel equipmentViewModel,
        AlarmViewModel alarmViewModel,
        LogViewModel logViewModel)
    {
        _dashboardViewModel =
            dashboardViewModel;

        _equipmentViewModel =
            equipmentViewModel;

        _alarmViewModel =
            alarmViewModel;

        _logViewModel =
            logViewModel;

        _currentViewModel =
            _dashboardViewModel;
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        CurrentViewModel = _dashboardViewModel;
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private void ShowAlarm()
    {
        CurrentViewModel = _alarmViewModel;
    }

    [RelayCommand]
    private void ShowLog()
    {
        CurrentViewModel = _logViewModel;
    }
}
