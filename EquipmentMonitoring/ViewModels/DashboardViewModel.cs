using EquipmentMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private Equipment _equipment;

    public Equipment Equipment
    {
        get => _equipment;
        set
        {
            _equipment = value;
            OnPropertyChanged();
        }
    }
    public DashboardViewModel()
    {
        _equipment = new Equipment
        {
            EquipmentId = "EQ-01",
            Name = "Assembly Machine",
            Status = "RUN",
            Temperature = 32.4,
            Pressure = 1.24
        };
    }
}
