using EquipmentMonitoring.Models;
using EquipmentMonitoring.Services;
using System.Collections.ObjectModel;

namespace EquipmentMonitoring.ViewModels.Pages;

public partial class LogViewModel : ViewModelBase
{
    private readonly LogService
        _logService;

    public ObservableCollection<CommunicationLog>
        Logs => _logService.Logs;

    public LogViewModel(
        LogService logService)
    {
        _logService = logService;
    }
}
