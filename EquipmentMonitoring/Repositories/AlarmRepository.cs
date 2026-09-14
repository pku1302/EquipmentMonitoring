using EquipmentMonitoring.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Repositories;

public class AlarmRepository
{
    private readonly string _connectionString;

    public AlarmRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InsertAsync(Alarm alarm)
    {
        const string sql = """
            INSERT INTO AlarmHistory
            (
                EquipmentId,
                AlarmCode,
                AlarmMessage,
                OccurredAt,
                ClearedAt
            )
            VALUES
            (
                @EquipmentId,
                @AlarmCode,
                @AlarmMessage,
                @OccurredAt,
                @ClearedAT
            )
            """;

        await using SqlConnection connection =
            new(_connectionString);

        await connection.OpenAsync();

        await using SqlCommand command =
            new(sql, connection);

        command.Parameters.AddWithValue(
            "@EquipmentId",
            alarm.EquipmentId);

        command.Parameters.AddWithValue(
            "@AlarmCode",
            alarm.AlarmCode);

        command.Parameters.AddWithValue(
            "@AlarmMessage",
            alarm.AlarmMessage);

        command.Parameters.AddWithValue(
            "@OccurredAt",
            alarm.OccurredAt);

        command.Parameters.AddWithValue(
            "@ClearedAt",
            (object?)alarm.ClearedAt
            ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Alarm>> GetAllAsync()
    {
        const string sql = """
            SELECT
                Id,
                EquipmentId,
                AlarmCode,
                AlarmMessage,
                OccurredAt,
                ClearedAt
            FROM AlarmHistory
            ORDER BY OccurredAt DESC
            """;

        List<Alarm> alarms = new();

        await using SqlConnection connection =
            new(_connectionString);

        await connection.OpenAsync();

        await using SqlCommand command =
            new(sql, connection);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            alarms.Add(new Alarm
            {
                Id = reader.GetInt32(0),
                EquipmentId = reader.GetString(1),
                AlarmCode = reader.GetString(2),
                AlarmMessage = reader.GetString(3),
                OccurredAt = reader.GetDateTime(4),
                ClearedAt =
                    reader.IsDBNull(5)
                        ? null
                        : reader.GetDateTime(5)
            });
        }

        return alarms;
    }
}
