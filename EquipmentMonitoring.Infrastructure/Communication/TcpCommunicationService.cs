using EquipmentMonitoring.Core.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace EquipmentMonitoring.Infrastructure.Communication;
public class TcpCommunicationService : IEquipmentCommunication
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;

    public async Task ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default)
    {
        _client = new TcpClient();

        await _client.ConnectAsync(
            host,
            port,
            cancellationToken);

        _stream = _client.GetStream();

        _reader = new StreamReader(
            _stream,
            Encoding.UTF8);
    }

    public async Task<string?> ReceiveAsync(
        CancellationToken cancellationToken = default)
    {
        if (_reader == null)
            return null;

        return await _reader.ReadLineAsync(
            cancellationToken);
    }
}
