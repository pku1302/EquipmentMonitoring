using CommunityToolkit.Mvvm.ComponentModel;

namespace EquipmentMonitoring.ViewModels.Components;

public partial class KPICardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private double _value;

    [ObservableProperty]
    private string _unit = string.Empty;

    public KPICardViewModel()
    {
    }
}
