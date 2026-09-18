using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Commands;
using EquipmentMonitoring.Core.Enums;
using EquipmentMonitoring.Services;
using System.Windows;
using System.Windows.Input;

namespace EquipmentMonitoring.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    [ObservableProperty]
    private ConnectionState connectionState
        = ConnectionState.Disconnected;

    private readonly DashboardViewModel _dashboardViewModel;
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly AlarmViewModel _alarmViewModel;
    private readonly LogViewModel _logViewModel;

    public MainViewModel(
        DashboardViewModel dashboardViewModel,
        EquipmentViewModel equipmentViewModel,
        AlarmViewModel alarmViewModel,
        LogViewModel logViewModel,
        MonitoringSignalRService signalRService)
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

        signalRService.ConnectionStateChanged +=
            OnSignalRConnectionStateChanged;
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

    private void OnSignalRConnectionStateChanged(
        ConnectionState state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionState = state;
        });
    }
}
