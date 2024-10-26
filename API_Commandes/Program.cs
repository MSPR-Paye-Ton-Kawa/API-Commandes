using Microsoft.EntityFrameworkCore;
using API_Commandes.Models;
using Prometheus;
using API_Commandes.Messaging; 
using Serilog;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();


builder.Host.UseSerilog();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Configuration de la base de données
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration de RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory() { HostName = "localhost" }; 
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IStockCheckPublisher, StockCheckPublisher>();
builder.Services.AddSingleton<IStockCheckResponseConsumer, StockCheckResponseConsumer>();
builder.Services.AddSingleton<ICustomerCheckPublisher, CustomerCheckPublisher>();
builder.Services.AddSingleton<ICustomerCheckResponseConsumer, CustomerCheckResponseConsumer>();

// Configuration de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Utiliser le middleware Prometheus
app.UseMetricServer();  // Ajoute un endpoint pour les métriques Prometheus
app.UseHttpMetrics();   // Collecte les métriques HTTP (requêtes, latence, etc.)

// Utiliser CORS
app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
