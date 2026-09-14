using EquipmentMonitoring.Commands;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace EquipmentMonitoring.ViewModels;

public partial class AlarmViewModel : ViewModelBase
{
    private const string ConnectionString =
        @"Server=localhost;
              Database=EquipmentMonitoringDb;
              Trusted_Connection=True;
              TrustServerCertificate=True;";

    private readonly AlarmRepository
        _alarmRepository;

    public ObservableCollection<Alarm>
        Alarms
    { get; }
        = new();

    public ICommand RefreshCommand { get; }

    public AlarmViewModel()
    {
        _alarmRepository =
            new AlarmRepository(
                ConnectionString);

        RefreshCommand =
            new RelayCommand(
                async _ =>
                    await LoadAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            List<Alarm> alarms =
                await _alarmRepository
                    .GetAllAsync();

            Alarms.Clear();

            foreach (Alarm alarm in alarms)
            {
                Alarms.Add(alarm);
            }
        }
        catch
        {

        }
    }
}
