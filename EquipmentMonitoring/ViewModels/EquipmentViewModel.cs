using EquipmentMonitoring.Commands;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Parsers;
using EquipmentMonitoring.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EquipmentMonitoring.ViewModels;

public class EquipmentViewModel : ViewModelBase
{
    private readonly TcpCommunicationService
        _tcpService;

    private readonly EquipmentPacketParser
        _packetParser;

    private Equipment _equipment;

    private string _connectionStatus =
        "DISCONNECTED";

    public Equipment Equipment
    {
        get => _equipment;

        set
        {
            _equipment = value;
            OnPropertyChanged();
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;

        set
        {
            _connectionStatus = value;
            OnPropertyChanged();
        }
    }

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }

    public EquipmentViewModel()
    {
        _equipment = new Equipment
        {
            EquipmentId = "EQ01",
            Name = "Assembly Machine",
            Status = "STOP"
        };

        _tcpService =
            new TcpCommunicationService();

        _packetParser =
            new EquipmentPacketParser();

        _tcpService.DataReceived +=
            OnDataReceived;

        _tcpService.Connected +=
            OnConnected;

        _tcpService.Disconnected +=
            OnDisconnected;

        _tcpService.Reconnecting +=
            OnReconnecting;

        ConnectCommand =
            new RelayCommand(
                async _ =>
                    await ConnectAsync());

        DisconnectCommand =
            new RelayCommand(
                _ => Disconnect());
    }

    private async Task ConnectAsync()
    {
        try
        {
            ConnectionStatus =
                "CONNECTING...";

            await _tcpService.ConnectAsync(
                "127.0.0.1",
                5000);
        }

        catch
        {
            ConnectionStatus =
                "CONNECTION FAILED";
        }
    }

    private void Disconnect()
    {
        _tcpService.DisConnect();
    }

    private void OnDataReceived(
        string packet)
    {
        if (!_packetParser.TryParse(
                packet,
                out EquipmentData? data))
        {
            return;
        }

        if (data == null)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            Equipment = new Equipment
            {
                EquipmentId = data.EquipmentId,
                Name = Equipment.Name,
                Status = data.Status,
                Temperature = data.Temperature,
                Pressure = data.Pressure,
                MotorRpm = data.MotorRpm,
                ProductionCount = data.ProductionCount
            };
        });
    }
    private void OnConnected()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionStatus = "CONNECTED";
        });
    }

    private void OnDisconnected()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionStatus = "DISCONNECTED";
            Equipment.Status = "DISCONNECTED";

            OnPropertyChanged(nameof(Equipment));
        });
    }
    private void OnReconnecting(int attempt)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionStatus =
                $"RECONNECTING... ({attempt})";
        });
    }
}
