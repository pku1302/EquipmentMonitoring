using EquipmentMonitoring.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace EquipmentMonitoring.Services;

public class LogService
{
    private readonly string _logDirectory;

    public ObservableCollection<CommunicationLog>
        Logs { get; } = new();

    public LogService()
    {
        _logDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Logs");

        Directory.CreateDirectory(
            _logDirectory);
    }

    public async Task AddAsync(
        string level,
        string source,
        string message)
    {
        CommunicationLog log =
            new()
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message
            };

        await Application.Current.Dispatcher
            .InvokeAsync(() =>
            {
                Logs.Insert(0, log);
            });

        await WriteToFileAsync(log);
    }

    private async Task WriteToFileAsync(
        CommunicationLog log)
    {
        try
        {
            string fileName =
                $"Log_{DateTime.Now:yyyyMMdd}.txt";

            string filePath =
                Path.Combine(
                    _logDirectory,
                    fileName);

            string line =
                $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] " +
                $"[{log.Level}] " +
                $"[{log.Source}] " +
                $"{log.Message}";

            await File.AppendAllTextAsync(
                filePath,
                line + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Debug.WriteLine(
                $"Log file write failed: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine(
                $"Log directory access denied: {ex.Message}");
        }

    }
}
