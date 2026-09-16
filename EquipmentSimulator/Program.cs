using EquipmentSimulator;
using System.Net;
using System.Net.Sockets;
using System.Text;

var listener = new TcpListener(IPAddress.Loopback, 5000);

listener.Start();

Console.WriteLine("Equipment Simulator started");
Console.WriteLine("Listening on 127.0.0.1:5000");

var equipments = new List<SimulatedEquipment>
{
    new()
    {
        EquipmentId = "EQ01",
        Status = "RUN",
        ProductionCount = 1000
    },

    new()
    {
        EquipmentId = "EQ02",
        Status = "RUN",
        ProductionCount = 2000
    },

    new()
    {
        EquipmentId = "EQ03",
        Status = "STOP",
        ProductionCount = 500
    }
};

while (true)
{
    using TcpClient client = await listener.AcceptTcpClientAsync();

    Console.WriteLine("Client connected");

    using NetworkStream stream = client.GetStream();

    Random random = new();

    try
    {
        while (client.Connected)
        {
            foreach (var equipment in equipments)
            {
                if (equipment.Status == "STOP")
                {
                    equipment.Temperature =
                        28 + random.NextDouble() * 3;

                    equipment.Pressure =
                        1.0 + random.NextDouble() * 0.2;

                    equipment.MotorRpm = 0;
                }
                else
                {
                    if (random.Next(0, 10) == 0)
                    {
                        equipment.Temperature =
                            random.Next(80, 101);
                    }
                    else
                    {
                        equipment.Temperature =
                            30 + random.NextDouble() * 5;
                    }

                    equipment.Pressure =
                        1.0 + random.NextDouble() * 0.5;

                    equipment.MotorRpm =
                        random.Next(1400, 1501);

                    equipment.ProductionCount++;
                }

                string packet =
                    $"{equipment.EquipmentId}|" +
                    $"{equipment.Status}|" +
                    $"{equipment.Temperature:F1}|" +
                    $"{equipment.Pressure:F2}|" +
                    $"{equipment.MotorRpm}|" +
                    $"{equipment.ProductionCount}\n";

                byte[] data =
                    Encoding.UTF8.GetBytes(packet);

                await stream.WriteAsync(data);

                Console.WriteLine(
                    $"TX: {packet.Trim()}");
            }
            
            await Task.Delay(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Connection closed: {ex.Message}");
    }
}

