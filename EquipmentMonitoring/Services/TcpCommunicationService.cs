using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace EquipmentMonitoring.Services;

public class TcpCommunicationService
{
    private TcpClient? _client;
    private NetworkStream? _stream;

    private CancellationTokenSource? _receiveCts; // 수신할때 쓰는 취소 토큰
    private CancellationTokenSource? _reconnectCts; // 재연결할때 쓰는 취소 토큰

    private const int MaxReconnectAttempts = 3;
    private string _ipAddress = string.Empty;
    private int _port;
    
    private bool _manualDisconnect; // 사용자가 수동으로 연결 해제를 눌렀는가?

    public event Action<string>? DataReceived;
    public event Action? Disconnected;
    public event Action? Connected;
    public event Action<int>? Reconnecting;
    public event Action? ReconnectFailed;

    public bool IsConnected =>
        _client?.Connected == true;

    public async Task ConnectAsync(
        string ipAddress,
        int port)
    {
        _ipAddress = ipAddress;
        _port = port;

        _manualDisconnect = false;

        _reconnectCts?.Cancel();
        _reconnectCts = new CancellationTokenSource();

        await ConnectInternalAsync(
            _reconnectCts.Token);
    }

    public void DisConnect()
    {
        _manualDisconnect = true;

        _receiveCts?.Cancel();
        _reconnectCts?.Cancel();

        CleanupConnection();

        Disconnected?.Invoke();
    }
    private async Task ConnectInternalAsync(
        CancellationToken cancellationToken)
    {
        CleanupConnection();

        _client = new TcpClient();

        await _client.ConnectAsync(
            _ipAddress,
            _port,
            cancellationToken);

        _stream = _client.GetStream();

        _receiveCts?.Cancel();
        _receiveCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        Connected?.Invoke();

        _ = ReceiveLoopAsync(
            _receiveCts.Token);
    }

    private async Task ReceiveLoopAsync(
        CancellationToken cancellationToken)
    {
        if (_stream == null)
            return;

        byte[] buffer = new byte[1024];
        StringBuilder receiveBuffer = new();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead =
                    await _stream.ReadAsync(
                        buffer,
                        cancellationToken);

                if (bytesRead == 0)
                    break;

                string receivedText =
                    Encoding.UTF8.GetString(
                        buffer,
                        0,
                        bytesRead);

                receiveBuffer.Append(receivedText);

                ProcessPackets(receiveBuffer);
            }
        }
        catch (OperationCanceledException)
        {
            // 정상 disconnect
        }
        catch (IOException)
        {
            // 통신 연결 문제
        }
        catch (SocketException)
        {
            // Socket 오류
        }
        finally
        {
            CleanupConnection();

            if (!_manualDisconnect &&
                !cancellationToken.IsCancellationRequested)
            {
                Disconnected?.Invoke();

                _ = ReconnectLoopAsync();
            }
        }
    }

    private async Task ReconnectLoopAsync()
    {
        if (_reconnectCts == null)
            return;

        int attempt = 0;

        while (!_manualDisconnect &&
            !_reconnectCts.IsCancellationRequested )
        {
            attempt++;

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(3),
                    _reconnectCts.Token);

                Reconnecting?.Invoke(attempt);

                await ConnectInternalAsync(
                    _reconnectCts.Token);

                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
                when (ex is SocketException or IOException)
            {
                if (attempt >= MaxReconnectAttempts)
                {
                    ReconnectFailed?.Invoke();
                    return;
                }
            }
        }
    }

    private void ProcessPackets(
        StringBuilder receiveBuffer)
    {
        while (true)
        {
            string current =
                receiveBuffer.ToString();

            int newlineIndex =
                current.IndexOf('\n');

            if (newlineIndex < 0)
                break;

            string packet =
                current[..newlineIndex];
                
            receiveBuffer.Remove(
                0,
                newlineIndex + 1);

            if (!string.IsNullOrWhiteSpace(packet))
            {
                DataReceived?.Invoke(
                    packet.Trim());
            }
        }
    }
    private void CleanupConnection()
    {
        try
        {
            _stream?.Dispose();
        }
        catch
        {

        }
        try
        {
            _client?.Dispose();
        }
        catch
        {

        }

        _stream = null;
        _client = null;
    }
}
