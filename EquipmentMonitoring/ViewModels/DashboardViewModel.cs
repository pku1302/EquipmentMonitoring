using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;

namespace EquipmentMonitoring.ViewModels;

public enum ChartStatus
{
    Disconnected,
    Live,
    History,
}

public partial class DashboardViewModel : ViewModelBase
{
    private static readonly TimeSpan DataRetention =
        TimeSpan.FromMinutes(5);
    private static readonly TimeSpan VisibleRange =
        TimeSpan.FromSeconds(30);

    private readonly EquipmentStateService
        _equipmentStateService;

    [ObservableProperty]
    private bool _isAutoFollow = true;
    [ObservableProperty]
    private ChartStatus _chartStatus = ChartStatus.Disconnected;
    [ObservableProperty]
    private bool _isConnected = false;

    private readonly List<SensorChart> _charts;

    public SensorChart TemperatureChart { get; }
    public SensorChart PressureChart { get; }
    public SensorChart MotorRpmChart { get; }

    public Equipment Equipment =>
        _equipmentStateService.CurrentEquipment;


    private readonly Queue<SensorSample> _sensorBuffer = new();

    private readonly DateTimeAxis _timeAxis;
    public Axis[] XAxes { get; }

    partial void OnIsAutoFollowChanged(
        bool oldValue,
        bool newValue)
    {
        if (!IsConnected)
        {
            ChartStatus = ChartStatus.Disconnected;
            return;
        }

        if (!oldValue && newValue)
        {
            ChartStatus = ChartStatus.Live;
        }

        if (oldValue && !newValue)
        {
            ChartStatus = ChartStatus.History;
        }
    }

    partial void OnIsConnectedChanged(
        bool oldValue,
        bool newValue)
    {
        if (!oldValue && newValue)
        {
            ChartStatus = ChartStatus.Live;
        }

        if (oldValue && !newValue)
        {
            ChartStatus = ChartStatus.Disconnected;
        }
    }

    [RelayCommand]
    private void GoLive()
    {
        if (ChartStatus == ChartStatus.Disconnected)
            return;

        if (_sensorBuffer.Count == 0)
            return;

        IsAutoFollow = true;

        foreach (SensorChart chart in _charts)
        {
            chart.Clear();
        }

        foreach (SensorSample sample
            in _sensorBuffer)
        {
            foreach (SensorChart chart in _charts)
            {
                chart.Add(sample);
            }
        }

        DateTime latest =
            _sensorBuffer.Last().Timestamp;

        MoveToLive(latest);
    }

    public DashboardViewModel(
        EquipmentStateService equipmentStateService)
    {
        _equipmentStateService =
            equipmentStateService;

        _timeAxis =
            new DateTimeAxis(
                TimeSpan.FromSeconds(1),
                date => date.ToString("HH:mm:ss"))
            {
                Name = "Time"
            };

        XAxes =
            [
                _timeAxis
            ];

        TemperatureChart = new SensorChart(
            "Temperature",
            "Temperature (℃)",
            sample => sample.Temperature,
            0,
            100,
            50,
            new SKColor(220, 38, 38));

        PressureChart = new SensorChart(
            "Pressure",
            "Pressure",
            sample => sample.Pressure,
            0,
            2,
            1,
            new SKColor(37, 99, 235));

        MotorRpmChart = new SensorChart(
            "Motor RPM",
            "RPM",
            sample => sample.MotorRpm,
            0,
            3000,
            1500,
            new SKColor(22, 163, 74));

        _charts =
            [
                TemperatureChart,
                PressureChart,
                MotorRpmChart
            ];

        _equipmentStateService.EquipmentUpdated +=
            OnEquipmentUpdated;

        _equipmentStateService.Connected +=
            OnConnected;

        _equipmentStateService.Disconnected +=
            OnDisconnected;
    }
    private void MoveToLive(DateTime now)
    {
        DateTime start =
            now - VisibleRange;

        _timeAxis.MinLimit =
            start.Ticks;

        _timeAxis.MaxLimit =
            now.Ticks;

        foreach (SensorChart chart in _charts)
        {
            chart.SetXRange(start, now);
        }
    }

    private void OnEquipmentUpdated(
        Equipment equipment)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            DateTime now = DateTime.Now;

            SensorSample sample = new()
            {
                Timestamp = now,
                Temperature = equipment.Temperature,
                Pressure = equipment.Pressure,
                MotorRpm = equipment.MotorRpm,
                ProductionCount = equipment.ProductionCount
            };

            AddToBuffer(sample);

            if (IsAutoFollow)
            {
                AddSampleToChart(sample);

                RemoveOldChartData(now);

                MoveToLive(now);
            }
        });
    }

    private void AddSampleToChart(
        SensorSample sample)
    {
        foreach (SensorChart chart in _charts)
        {
            chart.Add(sample);
        }
    }
    private void RemoveOldChartData(
        DateTime now)
    {
        DateTime cutoff =
            now - DataRetention;

        foreach (SensorChart chart in _charts)
        {
            chart.RemoveBefore(cutoff);
        }
    }

    private void AddToBuffer(
        SensorSample sample)
    {
        _sensorBuffer.Enqueue(sample);

        DateTime cutoff =
            sample.Timestamp - DataRetention;

        while (_sensorBuffer.Count > 0 &&
            _sensorBuffer.Peek().Timestamp < cutoff)
        {
            _sensorBuffer.Dequeue();
        }
    }

    public void SyncVisibleRange(
        double minLimit,
        double maxLimit)
    {
        foreach (SensorChart chart in _charts)
        {
            chart.XAxes[0].MinLimit = minLimit;
            chart.XAxes[0].MaxLimit = maxLimit;
        }
    }

    private void OnConnected()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsConnected = true;
            IsAutoFollow = true;
        });
    }

    private void OnDisconnected()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsConnected = false;
            IsAutoFollow = false;
        });
    }
}
