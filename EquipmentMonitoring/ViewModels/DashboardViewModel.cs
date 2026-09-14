using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
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

    public Equipment Equipment =>
        _equipmentStateService.CurrentEquipment;

    private readonly Queue<SensorSample> _sensorBuffer = new();
    public ObservableCollection<DateTimePoint> TemperatureValues { get; }
        = new();

    public ISeries[] TemperatureSeries { get; }

    private readonly DateTimeAxis _timeAxis;
    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }

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
        TemperatureValues.Clear();

        foreach (SensorSample sample
            in _sensorBuffer)
        {
            TemperatureValues.Add(
                new DateTimePoint(
                    sample.Timestamp,
                    sample.Temperature));
        }

        DateTime latest =
            TemperatureValues[^1].DateTime;

        MoveToLive(latest);
    }

    public DashboardViewModel(
        EquipmentStateService equipmentStateService)
    {
        _equipmentStateService =
            equipmentStateService;

        TemperatureSeries =
            [
                new LineSeries<DateTimePoint>
                {
                    Values = TemperatureValues,
                    Name = "Temperature",
                    Fill = null,

                    GeometrySize = 6,

                    LineSmoothness = 0.2
                }
            ];

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

        YAxes =
            [
                new Axis
                {
                    Name = "Temperature (℃)"
                }
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
        TemperatureValues.Add(
            new DateTimePoint(
                sample.Timestamp,
                sample.Temperature));
    }
    private void RemoveOldChartData(
        DateTime now)
    {
        DateTime cutoff =
            now - DataRetention;

        while (TemperatureValues.Count > 0 &&
            TemperatureValues[0].DateTime < cutoff)
        {
            TemperatureValues.RemoveAt(0);
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
    public void TryEnableAutoFollow()
    {
        if (_sensorBuffer.Count == 0)
            return;

        if (_timeAxis.MaxLimit == null)
            return;

        DateTime latestTime =
            _sensorBuffer.Last().Timestamp;

        double latestTicks =
            latestTime.Ticks;

        double currentMax =
            _timeAxis.MaxLimit.Value;

        double tolerance =
            TimeSpan.FromSeconds(2).Ticks;

        if (currentMax >= latestTicks - tolerance)
        {
            GoLive();
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
