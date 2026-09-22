using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Core.Models;
using EquipmentMonitoring.Services;
using EquipmentMonitoring.ViewModels.Components;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System.Windows;

namespace EquipmentMonitoring.ViewModels.Pages;

public partial class LiveChartViewModel : ViewModelBase
{
    private readonly MonitoringSignalRService _signalRService;
    private readonly SensorApiService _sensorApiService;
    private bool _isSyncingCharts;
    public KPICardViewModel ProductionCountCard { get; }
    public KPICardViewModel ProductionRateCard { get; }
    public KPICardViewModel ActiveAlarmCountCard { get; }
    public KPICardViewModel HistoricalAlarmCountCard { get; }

    // ============= CHART ================

    [ObservableProperty]
    private bool _isAutoFollow = true;

    private static readonly TimeSpan DataRetention =
        TimeSpan.FromHours(24);
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
        MonitoringSignalRService signalRService,
        SensorApiService sensorApiService)
    {
        _signalRService = signalRService;
        _sensorApiService = sensorApiService;

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

    [RelayCommand]
    private void GoLive()
    {
        if (IsAutoFollow)
            return;

        IsAutoFollow = true;

        foreach (SensorChart chart in _charts)
        {
            chart.Clear();
            chart.UseLiveValues();
        }

        foreach (EquipmentData data in _sensorBuffer)
        {
            foreach (SensorChart chart in _charts)
            {
                chart.AddLive(data);
            }
        }

        DateTime latest =
            _sensorBuffer.Last().Timestamp;

        MoveToLive(latest);
    }

    [RelayCommand]
    private async Task SetChartRangeAsync(string range)
    {
        var duration = range switch
        {
            "24H" => TimeSpan.FromHours(24),
            "12H" => TimeSpan.FromHours(12),
            "1H"  => TimeSpan.FromHours(1),
            _     => TimeSpan.FromHours(1)
        };

        var end = DateTime.UtcNow;
        var start = end - duration;

        IsAutoFollow = false;

        // 과거 데이터 조회
        List<EquipmentData> history =
            await _sensorApiService.GetHistoryAsync(
                "EQ01",
                start,
                end);

        // 각 차트의 HistoryValues 구성
        foreach (SensorChart chart in _charts)
        {
            chart.SetHistory(history);
            chart.UseHistoryValues();
        }

        // 세 차트의 X축 범위를 동일하게 맞춤
        SyncVisibleRange(
            start.Ticks,
            end.Ticks);
    }

    // 세 차트의 X축 범위를 동일하게 맞추는 메서드
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

    // SignalR로부터 데이터가 들어올 때마다 실행하는 메서드
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
                // 세 차트에 데이터를 넣고,
                // 시간 제한 범위 밖에 데이터는 버림
                AddDataToChart(data);

                // 차트를 현재 시간으로 당김
                MoveToLive(data.Timestamp);
            }
        });
    }

    // 현재 시각 - DataRetention 까지의 데이터만 버퍼에 담는다
    // 차트는 멈출지언정 버퍼에 데이터 담는 건 멈추지 않는다
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

    // 데이터를 넣고, 넘치는 데이터는 삭제
    private void AddDataToChart(
        EquipmentData data)
    {
        foreach (SensorChart chart in _charts)
        {
            chart.AddLive(data);

            chart.RemoveBefore(data.Timestamp - DataRetention);
        }
    }

    // 현재 시각으로 이동
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
            // 한 눈에 보이는 영역을 VisibleRange로 줄인다
            chart.SetXRange(start, recent);
        }
    }
}
