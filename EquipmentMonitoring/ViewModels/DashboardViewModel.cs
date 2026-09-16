using EquipmentMonitoring.Core.Models;
using EquipmentMonitoring.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace EquipmentMonitoring.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly MonitoringSignalRService _signalRService;

    public ObservableCollection<EquipmentData> Equipments { get; }
        = new();

    public DashboardViewModel(
        MonitoringSignalRService signalRService)
    {
        _signalRService = signalRService;

        _signalRService.EquipmentUpdated += OnEquipmentUpdated;
    }
    private void OnEquipmentUpdated(EquipmentData data)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = Equipments
                .FirstOrDefault(x =>
                    x.EquipmentId == data.EquipmentId);

            if (existing == null)
            {
                Equipments.Add(data);
                return;
            }

            var index = Equipments.IndexOf(existing);

            Equipments[index] = data;
        });
    }
}
