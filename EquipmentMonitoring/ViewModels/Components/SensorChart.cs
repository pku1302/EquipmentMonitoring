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
    public ObservableCollection<DateTimePoint> Values { get; }
        = new();
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

        Series =
            [
                new LineSeries<DateTimePoint>
                {
                    Values = Values,
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
                }
            ];

        XAxes =
            [
                new DateTimeAxis(
                    TimeSpan.FromSeconds(1),
                    date => date.ToString("HH:mm:ss"))
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
    public void Add(EquipmentData sample)
    {
        Values.Add(
            new DateTimePoint(
                sample.Timestamp,
                _valueSelector(sample)));

        CurrentValue = _valueSelector(sample);
    }
    public void Clear()
    {
        Values.Clear();
    }

    public void RemoveBefore(DateTime cutoff)
    {
        while (Values.Count > 0 &&
            Values[0].DateTime < cutoff)
        {
            Values.RemoveAt(0);
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
