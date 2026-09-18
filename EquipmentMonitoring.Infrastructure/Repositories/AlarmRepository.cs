using EquipmentMonitoring.Core.Enums;
using EquipmentMonitoring.Core.Interfaces;
using EquipmentMonitoring.Core.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Infrastructure.Repositories;

public class AlarmRepository : IAlarmRepository
{
    private readonly string _connectionString;

    public AlarmRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> InsertAsync(
        Alarm alarm,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Alarm
            (
                EquipmentId,
                AlarmCode,
                AlarmMessage,
                Severity,
                OccurredAt,
                ClearedAt,
                IsActive
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @EquipmentId,
                @AlarmCode,
                @AlarmMessage,
                @Severity,
                @OccurredAt,
                NULL,
                1
            )
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@EquipmentId",
            alarm.EquipmentId);

        command.Parameters.AddWithValue(
            "@AlarmCode",
            alarm.AlarmCode.ToString());

        command.Parameters.AddWithValue(
            "@AlarmMessage",
            alarm.AlarmMessage);

        command.Parameters.AddWithValue(
            "@Severity",
            alarm.Severity.ToString());

        command.Parameters.AddWithValue(
            "@OccurredAt",
            alarm.OccurredAt);

        var result =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(result);
    }

    public async Task ClearAsync(
        long alarmId,
        DateTime clearedAt,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE Alarm
            SET
                IsActive = 0,
                ClearedAt = @ClearedAt
            WHERE Id = @Id
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@Id",
            alarmId);

        command.Parameters.AddWithValue(
            "@ClearedAt",
            clearedAt);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    public async Task<List<Alarm>> GetActiveAsync(
    CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
            
                id,
                EquipmentId,
                AlarmCode,
                AlarmMessage,
                Severity,
                OccurredAt,
                ClearedAt,
                IsActive
            FROM Alarm
            WHERE IsActive = 1
            ORDER BY OccurredAt DESC
            """;

        var result = new List<Alarm>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Alarm
            {
                Id = reader.GetInt64(0),
                EquipmentId = reader.GetString(1),

                AlarmCode = Enum.Parse<AlarmCode>(
                    reader.GetString(2)),

                Severity = Enum.Parse<AlarmSeverity>(
                    reader.GetString(4)),

                OccurredAt = reader.GetDateTime(5),

                ClearedAt = reader.IsDBNull(6)
                    ? null
                    : reader.GetDateTime(6),

                IsActive = reader.GetBoolean(7)
            });
        }

        return result;
    }

    public async Task<List<Alarm>> GetHistoryAsync(
        string? equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                EquipmentId,
                AlarmCode,
                AlarmMessage,
                Severity,
                OccurredAt,
                ClearedAt,
                IsActive
            FROM Alarm
            WHERE OccurredAt >= @From
              AND OccurredAt <= @To
              AND (@EquipmentId IS NULL OR EquipmentId = @EquipmentId)
            ORDER BY OccurredAt DESC
            """;

        var result = new List<Alarm>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@From", from);
        command.Parameters.AddWithValue("@To", to);

        command.Parameters.AddWithValue(
            "@EquipmentId",
            (object?)equipmentId ?? DBNull.Value);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Alarm
            {
                Id = reader.GetInt64(0),
                EquipmentId = reader.GetString(1),

                AlarmCode = Enum.Parse<AlarmCode>(
                    reader.GetString(2)),

                AlarmMessage = reader.GetString(3),

                Severity = Enum.Parse<AlarmSeverity>(
                    reader.GetString(4)),

                OccurredAt = reader.GetDateTime(5),

                ClearedAt = reader.IsDBNull(6)
                    ? null
                    : reader.GetDateTime(6),

                IsActive = reader.GetBoolean(7)
            });
        }

        return result;
    }
}
