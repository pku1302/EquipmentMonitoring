using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentMonitoring.Core.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;

namespace EquipmentMonitoring.ViewModels.Components;

public partial class SensorChart : ObservableObject
{
    public ObservableCollection<DateTimePoint?> LiveValues { get; }
        = new();

    public ObservableCollection<DateTimePoint?> HistoryValues { get; }
        = new();

    private readonly LineSeries<DateTimePoint?> _series;

    public ISeries[] Series { get; }
    public string Name { get; }
    public string Description { get; }
    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }
    public string Unit { get; }
    public string ColorHex { get; }
    private readonly Func<EquipmentData, double> _valueSelector;

    [ObservableProperty]
    private double _currentValue;

    public SensorChart(
        string name,
        string description,
        string yAxisName,
        Func<EquipmentData, double> valueSelector,
        double minLimit,
        double maxLimit,
        double minStep,
        SKColor color)
    {
        Name = name;
        Description = description;
        Unit = yAxisName;

        ColorHex = $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";

        _valueSelector = valueSelector;

        _series = new LineSeries<DateTimePoint?>
        {
            Values = LiveValues,
            Name = name,

            Stroke = new SolidColorPaint(
                        color,
                        3),

            Fill = new LinearGradientPaint(
                        new[]
                        {
                            new SKColor(
                                color.Red,
                                color.Green,
                                color.Blue,
                                80),

                            new SKColor(
                                color.Red,
                                color.Green,
                                color.Blue,
                                0)
                        },
                        new SKPoint(0, 0),
                        new SKPoint(0, 1)),

            GeometrySize = 4,

            GeometryStroke = new SolidColorPaint(
                        color,
                        2),

            GeometryFill =
                        new SolidColorPaint(SKColors.Wheat),

            LineSmoothness = 0.2
        };

        Series =
            [
                _series
            ];

        XAxes =
            [
                new DateTimeAxis(
                    TimeSpan.FromSeconds(1),
                    date => date.ToLocalTime().ToString("HH:mm:ss"))
            ];

        YAxes =
            [
                new Axis
                {
                    MinStep = minStep,
                    MinLimit = minLimit,
                    MaxLimit = maxLimit,
                }
            ];
    }
    public void UseLiveValues()
    {
        _series.Values = LiveValues;
    }
    public void UseHistoryValues()
    {
        _series.Values = HistoryValues;
    }

    public void AddLive(EquipmentData sample)
    {
        LiveValues.Add(
            new DateTimePoint(
                sample.Timestamp,
                _valueSelector(sample)));

        CurrentValue = _valueSelector(sample);
    }

    public void SetHistory(
        IEnumerable<EquipmentData> history)
    {
        HistoryValues.Clear();

        DateTime? previousTime = null;

        foreach (var sample in history.OrderBy(x => x.Timestamp))
        {
            if (previousTime.HasValue)
            {
                var gap =
                    sample.Timestamp - previousTime.Value;

                // DB의 현재 저장 주기는 5초
                // 15초 이상 데이터 간격이 있으면
                // Null을 넣어서 선을 끊는다
                if (gap > TimeSpan.FromSeconds(15))
                {
                    HistoryValues.Add(null);
                }
            }

            HistoryValues.Add(
                new DateTimePoint(
                    sample.Timestamp,
                    _valueSelector(sample)));

            previousTime = sample.Timestamp;
        }
    }

    public void Clear()
    {
        LiveValues.Clear();
    }

    public void RemoveBefore(DateTime cutoff)
    {
        while (LiveValues.Count > 0)
        {
            var first = LiveValues[0];

            if (first is null)
            {
                LiveValues.RemoveAt(0);
                continue;
            }

            if (first.DateTime < cutoff)
            {
                LiveValues.RemoveAt(0);
                continue;
            }

            break;
        }
    }

    public void SetXRange(
        DateTime start,
        DateTime end)
    {
        XAxes[0].MinLimit = start.Ticks;
        XAxes[0].MaxLimit = end.Ticks;
    }
}
