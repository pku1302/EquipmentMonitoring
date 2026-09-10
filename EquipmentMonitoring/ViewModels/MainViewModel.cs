using EquipmentMonitoring.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace EquipmentMonitoring.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel
        = new DashboardViewModel();
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;

        set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowDashboardCommand { get; }
    public ICommand ShowEquipmentCommand { get; }
    public ICommand ShowAlarmCommand { get; }
    public ICommand ShowLogCommand { get; }

    public MainViewModel()
    {
        ShowDashboardCommand = new RelayCommand(
            _ => CurrentViewModel = new DashboardViewModel());

        ShowEquipmentCommand = new RelayCommand(
            _ => CurrentViewModel = new EquipmentViewModel());

        ShowAlarmCommand = new RelayCommand(
            _ => CurrentViewModel = new AlarmViewModel());

        ShowLogCommand = new RelayCommand(
            _ => CurrentViewModel = new LogViewModel());
    }

}
