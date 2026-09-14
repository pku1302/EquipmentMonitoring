using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace EquipmentMonitoring.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private const int MaxChartPoints = 30;

    private readonly EquipmentStateService
        _equipmentStateService;

    public Equipment Equipment =>
        _equipmentStateService.CurrentEquipment;

    public ObservableCollection<double>
        TemperatureValues { get; }
        = new();

    public ISeries[] TemperatureSeries { get; }

    public DashboardViewModel(
        EquipmentStateService equipmentStateService)
    {
        _equipmentStateService =
            equipmentStateService;

        TemperatureSeries =
            [
                new LineSeries<double>
                {
                    Values = TemperatureValues,
                    Name = "Temperature",
                    Fill = null
                }
            ];

        _equipmentStateService.EquipmentUpdated +=
            OnEquipmentUpdated;
    }
    private void OnEquipmentUpdated(
        Equipment equipment)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            OnPropertyChanged(nameof(Equipment));

            TemperatureValues.Add(
                equipment.Temperature);

            if (TemperatureValues.Count >
                    MaxChartPoints)
            {
                TemperatureValues.RemoveAt(0);
            }
        });
    }
}
