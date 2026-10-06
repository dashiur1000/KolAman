//using AlertsAPI.Exceptions;
//using AlertsAPI.Repositories;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using MongoDB.Driver;

//var builder = WebApplication.CreateBuilder(args);

//var mongoConnectionString = "mongodb://localhost:27017";
//var databaseName = "AlertsDB";
//builder.Services.AddSingleton(sp => new MongoClient(mongoConnectionString));
//builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

//builder.Services.AddScoped<IAlertRepository, AlertRepository>();

//builder.Services.AddProblemDetails();

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//app.UseGlobalExceptionMiddleware();
//app.UseExceptionHandler();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();

//app.Run();

using AlertsAPI.Exceptions;
using AlertsAPI.Models;
using AlertsAPI.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Alerts"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    if (string.IsNullOrWhiteSpace(options?.ConnectionString))
        throw new ArgumentException("Missing ConnectionString in configuration.");

    return new MongoClient(options.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();

    if (string.IsNullOrWhiteSpace(options?.DatabaseName))
        throw new ArgumentException("Missing DatabaseName in 'Alerts' configuration.");

    return client.GetDatabase(options.DatabaseName);
});

builder.Services.AddScoped<IMongoCollection<AlertModel>>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    var database = sp.GetRequiredService<IMongoDatabase>();

    var collectionName = string.IsNullOrWhiteSpace(options.CollectionName) ? "Alerts" : options.CollectionName;
    return database.GetCollection<AlertModel>(collectionName);
});

var app = builder.Build();

app.UseGlobalExceptionMiddleware();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
