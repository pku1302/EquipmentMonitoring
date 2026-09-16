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

    public Task<List<Alarm>> GetActiveAsync(
    CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Alarm>> GetHistoryAsync(
        string? equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
