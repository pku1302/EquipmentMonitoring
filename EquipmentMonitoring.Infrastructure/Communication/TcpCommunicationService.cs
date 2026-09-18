using EquipmentMonitoring.Core.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace EquipmentMonitoring.Infrastructure.Communication;
public class TcpCommunicationService : IEquipmentCommunication
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;

    private string _host = string.Empty;
    private int _port;
    public bool IsConnected =>
        _client?.Connected == true;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<int>? Reconnecting;
    public event Action? ReconnectFailed;

    public async Task ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default)
    {
        _host = host;
        _port = port;

        await ConnectInternalAsync(
            cancellationToken);
    }

    private async Task ConnectInternalAsync(
        CancellationToken cancellationToken)
    {
        _client = new TcpClient();

        await _client.ConnectAsync(
            _host,
            _port,
            cancellationToken);

        _stream = _client.GetStream();

        _reader = new StreamReader(
            _stream,
            Encoding.UTF8);

        Connected?.Invoke();
    }

    public async Task<string?> ReceiveAsync(
        CancellationToken cancellationToken = default)
    {
        if (_reader == null)
            return null;

        var line =
            await _reader.ReadLineAsync(
                cancellationToken);

        if (line == null)
        {
            Disconnected?.Invoke();
        }

        return line;
    }

    public async Task<bool> ReconnectAsync(
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;

        for (int attempt = 1;
            attempt <= maxAttempts;
            attempt++)
        {
            try
            {
                Reconnecting?.Invoke(attempt);

                await DisconnectInternalAsync();

                await ConnectInternalAsync(
                    cancellationToken);

                return true;
            }
            catch
            {
                if (attempt < maxAttempts)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(3),
                        cancellationToken);
                }
            }
        }

        ReconnectFailed?.Invoke();

        return false;
    }

    public async Task DisconnectAsync()
    {
        await DisconnectInternalAsync();

        Disconnected?.Invoke();
    }

    private Task DisconnectInternalAsync()
    {
        _reader?.Dispose();
        _stream?.Dispose();
        _client?.Dispose();

        _reader = null;
        _stream = null;
        _client = null;

        return Task.CompletedTask;
    }
}
