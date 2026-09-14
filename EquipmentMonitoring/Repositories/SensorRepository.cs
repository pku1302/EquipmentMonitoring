using EquipmentMonitoring.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Repositories;

public class SensorRepository
{
    private readonly string _connectionString;

    public SensorRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InsertAsync(
        SensorHistory history)
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

        await using SqlConnection connection =
            new(_connectionString);

        await connection.OpenAsync();

        await using SqlCommand command =
            new(sql, connection);

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
            history.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }
}
