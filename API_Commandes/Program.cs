using Microsoft.EntityFrameworkCore;
using System;
using API_Commandes.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Ajouter le service CORS avec des options spécifiques
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.
//builder.Services.AddControllers().AddJsonOptions(options =>
//{
//    // �viter les boucles infinies avec la gestion des r�f�rences
//    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
//    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
//});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Simplifier la gestion des références circulaires
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

    // Ignorer les propriétés nulles
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Add DbContext and SQLite configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Utiliser le middleware Prometheus
app.UseMetricServer();  // Ajoute un endpoint pour les métriques Prometheus
app.UseHttpMetrics();   // Collecte les métriques HTTP (requêtes, latence, etc.)

// Utiliser CORS
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
