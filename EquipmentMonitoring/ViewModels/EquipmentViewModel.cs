using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentMonitoring.Commands;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;

namespace EquipmentMonitoring.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    public EquipmentService EquipmentService { get; }

    public Equipment Equipment =>
         EquipmentService.CurrentEquipment;
    public EquipmentViewModel(
    EquipmentService equipmentService)
    {
        EquipmentService = equipmentService;
    }

    [ObservableProperty]
    public string _connectionStatus
        = "DISCONNECTED";

    [RelayCommand]
    private async Task ConnectAsync()
    {
        await EquipmentService
            .ConnectAsync();

        ConnectionStatus = "CONNECTED";
    }

    [RelayCommand]
    private async Task DisConnect()
    {
        EquipmentService
            .Disconnect();
    }


}
