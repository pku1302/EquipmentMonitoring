using System.Net;
using System.Net.Sockets;
using System.Text;

var listener = new TcpListener(IPAddress.Loopback, 5000);

listener.Start();

Console.WriteLine("Equipment Simulator started");
Console.WriteLine("Listening on 127.0.0.1:5000");

while (true)
{
    using TcpClient client = await listener.AcceptTcpClientAsync();

    Console.WriteLine("Client connected");

    using NetworkStream stream = client.GetStream();

    int productionCount = 1000;
    Random random = new();

    try
    {
        while (client.Connected)
        {
            double temperature =
                30 + random.NextDouble() * 5;

            double pressure =
                1.0 + random.NextDouble() * 0.5;

            int rpm =
                random.Next(1400, 1501);

            productionCount++;

            string packet =
                $"EQ01|RUN|{temperature:F1}|{pressure:F2}|{rpm}|{productionCount}\n";

            byte[] data =
                Encoding.UTF8.GetBytes(packet);

            await stream.WriteAsync(data);

            Console.WriteLine($"TX: {packet.Trim()}");

            await Task.Delay(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Connection closed: {ex.Message}");
    }
}

