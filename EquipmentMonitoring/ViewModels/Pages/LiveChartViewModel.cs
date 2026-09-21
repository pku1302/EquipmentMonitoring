using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentMonitoring.Core.Models;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;
using EquipmentMonitoring.ViewModels.Components;
using EquipmentMonitoring.Views.Chart;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;

namespace EquipmentMonitoring.ViewModels.Pages;

public partial class LiveChartViewModel : ViewModelBase
{
    private readonly MonitoringSignalRService _signalRService;

    private const int MaxChartPoints = 60;

    private bool _isSyncingCharts;

    public KPICardViewModel ProductionCountCard { get; }
    public KPICardViewModel ProductionRateCard { get; }
    public KPICardViewModel ActiveAlarmCountCard { get; }
    public KPICardViewModel HistoricalAlarmCountCard { get; }

    // ============= CHART ================

    [ObservableProperty]
    private bool _isAutoFollow = true;

    public ObservableCollection<SensorChartPoint>
        TemperaturePoints { get; } = new();

    private static readonly TimeSpan DataRetention =
        TimeSpan.FromMinutes(5);
    private static readonly TimeSpan VisibleRange =
        TimeSpan.FromSeconds(30);

    private readonly DateTimeAxis _timeAxis;

    private readonly Queue<EquipmentData> _sensorBuffer = new();

    private readonly List<SensorChart> _charts;
    public SensorChart TemperatureChart { get; }
    public SensorChart PressureChart { get; }
    public SensorChart MotorRpmChart { get; }

    // ====================================
    public LiveChartViewModel(
        MonitoringSignalRService signalRService)
    {
        _signalRService = signalRService;


        ProductionCountCard = new KPICardViewModel
        {
            Title = "Production Count",
            Value = 0,
            Unit = "pcs"
        };

        ProductionRateCard = new KPICardViewModel
        {
            Title = "Production Rate",
            Value = 0,
            Unit = "pcs/min"
        };

        ActiveAlarmCountCard = new KPICardViewModel
        {
            Title = "Active Alarm Count",
            Value = 0,
            Unit = "건"
        };

        HistoricalAlarmCountCard = new KPICardViewModel
        {
            Title = "Historical Alarm Count",
            Value = 0,
            Unit = "건"
        };

        // ====================Chart==================
        _timeAxis =
            new DateTimeAxis(
                TimeSpan.FromSeconds(1),
                date => date.ToString("HH:mm:ss"))
            {
                Name = "Time"
            };

        TemperatureChart = new SensorChart(
            "Temperature",
            "Real-time equipment temperature",
            "°C",
            sample => sample.Temperature,
            0,
            100,
            50,
            new SKColor(220, 38, 38));

        PressureChart = new SensorChart(
            "Pressure",
            "Real-time equipment pressure",
            "bar",
            sample => sample.Pressure,
            0,
            2,
            1,
            new SKColor(37, 99, 235));

        MotorRpmChart = new SensorChart(
            "Motor RPM",
            "Real-time equipment RPM",
            "rpm",
            sample => sample.MotorRpm,
            0,
            2000,
            1000,
            new SKColor(22, 163, 74));

        _charts =
            [
                TemperatureChart,
                PressureChart,
                MotorRpmChart
            ];

        _signalRService.EquipmentUpdated +=
            OnEquipmentUpdated;
    }

    public void SyncVisibleRange(
        double minLimit,
        double maxLimit)
    {
        if (_isSyncingCharts)
            return;

        try
        {
            _isSyncingCharts = true;

            var start =
                new DateTime((long)minLimit);

            var end =
                new DateTime((long)maxLimit);

            TemperatureChart.SetXRange(start, end);
            PressureChart.SetXRange(start, end);
            MotorRpmChart.SetXRange(start, end);
        }
        finally
        {
            _isSyncingCharts = false;
        }
    }

    private void OnEquipmentUpdated(
        EquipmentData data)
    {
        if (data.EquipmentId != "EQ01")
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            AddToBuffer(data);

            if (IsAutoFollow)
            {
                AddDataToChart(data);

                MoveToLive(data.Timestamp);
            }
        });
    }

    private void AddToBuffer(
        EquipmentData data)
    {
        _sensorBuffer.Enqueue(data);

        DateTime cutoff =
            data.Timestamp - DataRetention;

        while (_sensorBuffer.Count > 0 &&
             _sensorBuffer.Peek().Timestamp < cutoff)
        {
            _sensorBuffer.Dequeue();
        }
    }

    private void AddDataToChart(
        EquipmentData data)
    {
        foreach (SensorChart chart in _charts)
        {
            chart.Add(data);

            chart.RemoveBefore(data.Timestamp - DataRetention);
        }
    }
    private void MoveToLive(DateTime recent)
    {
        DateTime start =
            recent - VisibleRange;

        _timeAxis.MinLimit =
            start.Ticks;

        _timeAxis.MaxLimit =
            recent.Ticks;

        foreach (SensorChart chart in _charts)
        {
            chart.SetXRange(start, recent);
        }
    }
}
