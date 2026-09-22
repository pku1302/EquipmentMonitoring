using EquipmentMonitoring.Core.Interfaces;
using EquipmentMonitoring.Core.Models;
using Microsoft.Data.SqlClient;

namespace EquipmentMonitoring.Infrastructure.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly string _connectionString;

    public SensorRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InsertAsync(
        EquipmentData history,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO SensorHistory
            (
                EquipmentId,
                Temperature,
                Pressure,
                MotorRpm,
                ProductionCount,
                CreatedAt
            )
            VALUES
            (
                @EquipmentId,
                @Temperature,
                @Pressure,
                @MotorRpm,
                @ProductionCount,
                @CreatedAt
            )
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@EquipmentId",
            history.EquipmentId);

        command.Parameters.AddWithValue(
            "@Temperature",
            history.Temperature);

        command.Parameters.AddWithValue(
            "@Pressure",
            history.Pressure);

        command.Parameters.AddWithValue(
            "@MotorRpm",
            history.MotorRpm);

        command.Parameters.AddWithValue(
            "@ProductionCount",
            history.ProductionCount);

        command.Parameters.AddWithValue(
            "@CreatedAt",
            history.Timestamp);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    public async Task<List<EquipmentData>> GetHistoriesAsync(
        string equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {

        const string sql = """
            SELECT
                id,
                EquipmentId,
                Temperature,
                Pressure,
                MotorRpm,
                ProductionCount,
                CreatedAt
            FROM SensorHistory
            WHERE EquipmentId = @EquipmentId
              AND CreatedAt >= @From
              AND CreatedAt <= @To
            ORDER BY CreatedAt
            """;

        var result = new List<EquipmentData>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@EquipmentId",
            equipmentId);

        command.Parameters.AddWithValue(
            "@From",
            from);

        command.Parameters.AddWithValue(
            "@To",
            to);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new EquipmentData
            {
                EquipmentId = reader.GetString(1),
                Temperature = reader.GetDouble(2),
                Pressure = reader.GetDouble(3),
                MotorRpm = reader.GetInt32(4),
                ProductionCount = reader.GetInt32(5),
                Timestamp = reader.GetDateTime(6)
            });
        }

        return result;
    }
}

