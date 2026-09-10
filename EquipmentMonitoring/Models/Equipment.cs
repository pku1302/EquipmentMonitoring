using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace EquipmentMonitoring.Models;

public class Equipment : INotifyPropertyChanged
{
    private string _equipmentId = string.Empty;
    private string _name = string.Empty;
    private string _status = string.Empty;

    private double _temperature;
    private double _pressure;

    private int _motorRpm;
    private int _productionCount;

    public string EquipmentId
    {
        get => _equipmentId;

        set
        {
            _equipmentId = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;

        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;

        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public double Temperature
    {
        get => _temperature;

        set
        {
            _temperature = value;
            OnPropertyChanged();
        }
    }

    public double Pressure
    {
        get => _pressure;

        set
        {
            _pressure = value;
            OnPropertyChanged();
        }
    }

    public int MotorRpm
    {
        get => _motorRpm;

        set
        {
            _motorRpm = value;
            OnPropertyChanged();
        }
    }

    public int ProductionCount
    {
        get => _productionCount;

        set
        {
            _productionCount = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName]
            string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}
