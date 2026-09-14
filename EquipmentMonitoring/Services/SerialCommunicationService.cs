using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace EquipmentMonitoring.Services;

public class SerialCommunicationService
{
    private SerialPort? _serialPort;

    public event Action<string>? DataReceived;
    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<Exception>? ErrorOccurred;

    public bool IsConnected =>
        _serialPort?.IsOpen == true;

    public void Connect(
        string portName,
        int bandRate)
    {
        if (IsConnected)
            return;

        _serialPort = new SerialPort
        {
            PortName = portName,
            BaudRate = bandRate,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            NewLine = "\n",
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };

        _serialPort.DataReceived +=
            OnSerialDataReceived;

        _serialPort.Open();

        Connected?.Invoke();
    }
    public void Disconnect()
    {
        if (_serialPort == null)
            return;

        try
        {
            _serialPort.DataReceived -=
                OnSerialDataReceived;

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        finally
        {
            _serialPort.Dispose();
            _serialPort = null;

            Disconnected?.Invoke();
        }
    }

    public void Send(string message)
    {
        if (!IsConnected ||
            _serialPort == null)
        {
            throw new InvalidOperationException(
                "Serial port is not connected");
        }

        _serialPort.WriteLine(message);
    }

    private void OnSerialDataReceived(
        object sender,
        SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_serialPort == null)
                return;

            string line =
                _serialPort.ReadLine();

            DataReceived?.Invoke(
                line.Trim());
        }
        catch (TimeoutException)
        {
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }
    }
    public static string[] GetAvailablePorts()
    {
        return SerialPort
            .GetPortNames()
            .OrderBy(x => x)
            .ToArray();
    }

}
