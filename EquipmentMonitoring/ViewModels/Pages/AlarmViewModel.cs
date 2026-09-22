using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Core.Models;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;
using EquipmentMonitoring.ViewModels.Pages;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace EquipmentMonitoring.ViewModels.Pages;

public partial class AlarmViewModel : ViewModelBase
{
    private readonly MonitoringSignalRService _signalRService;
    private readonly AlarmApiService _alarmApiService;
    private bool _loaded = false;

    public ObservableCollection<AlarmItemViewModel> ActiveAlarms { get; }
        = new();

    public ObservableCollection<AlarmItemViewModel> AlarmHistory { get; }
        = new();

    [ObservableProperty]
    private string? selectedEquipmentId;

    [ObservableProperty]
    private DateTime from = 
        DateTime.Today.ToUniversalTime();

    [ObservableProperty]
    private DateTime to = 
        DateTime.UtcNow;

    [RelayCommand]
    private async Task SearchHistoryAsync()
    {
        var alarms =
            await _alarmApiService.GetHistoryAsync(
                SelectedEquipmentId,
                From,
                To);

        AlarmHistory.Clear();

        foreach (var alarm in alarms)
        {
            AlarmHistory.Add(
                new AlarmItemViewModel(alarm));
        }
    }

    public AlarmViewModel(
        MonitoringSignalRService signalRService,
        AlarmApiService alarmApiService)
    {
        _signalRService = signalRService;
        _alarmApiService = alarmApiService;

        _signalRService.AlarmRaised += OnAlarmRaised;
        _signalRService.AlarmCleared += OnAlarmCleared;
    }

    public async Task LoadAsync()
    {
        if (_loaded)
            return;

        _loaded = true;

        try
        {
            var alarms =
                await _alarmApiService.GetActiveAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ActiveAlarms.Clear();

                foreach (var alarm in alarms)
                {
                    ActiveAlarms.Add(
                        new AlarmItemViewModel(alarm));
                }
            });

            _loaded = true;
        }
        catch
        {
            _loaded = false;
            throw;
        }
    }

    private void OnAlarmRaised(Alarm alarm)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ActiveAlarms.Add(
                new AlarmItemViewModel(alarm));
        });
    }

    private void OnAlarmCleared(Alarm alarm)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing =
                ActiveAlarms.FirstOrDefault(
                    x => x.Id == alarm.Id);

            if (existing != null)
            {
                ActiveAlarms.Remove(existing);
            }
        });
    }

}
