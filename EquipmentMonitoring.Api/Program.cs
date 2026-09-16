using EquipmentMonitoring.Api.BackgroundServices;
using EquipmentMonitoring.Api.Hubs;
using EquipmentMonitoring.Api.Services;
using EquipmentMonitoring.Infrastructure.Repositories;
using EquipmentMonitoring.Infrastructure.Communication;
using EquipmentMonitoring.Infrastructure.Parsers;
using EquipmentMonitoring.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

builder.Services.AddSingleton<ISensorRepository>(
    new SensorRepository(connectionString));

builder.Services.AddSingleton<IAlarmRepository>(
    new AlarmRepository(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddSingleton<
    IEquipmentCommunication, TcpCommunicationService>();

builder.Services.AddSingleton<
    EquipmentPacketParser>();

builder.Services.AddSingleton<
    EquipmentStateService>();

builder.Services.AddSingleton<
    AlarmService>();

builder.Services.AddHostedService<
    EquipmentWorker>();

builder.Services.AddHostedService<
    SensorHistoryWorker>();

builder.Services.AddSignalR();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MonitoringHub>("/monitoringHub");

app.Run();
