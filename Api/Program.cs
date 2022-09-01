using Core;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Prometheus;
using Serilog;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Http(
        Environment.GetEnvironmentVariable("LOGSTASH_URI")
            ?? throw new ArgumentNullException("Logstash URI is needed"),
        null)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddSingleton<IMessageRepository>(sp => new MessageRepository(
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString"),
        Environment.GetEnvironmentVariable("DB_NAME")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbName"),
        Environment.GetEnvironmentVariable("DB_TWEET_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbTweetCollectionName"),
        Environment.GetEnvironmentVariable("DB_REPLY_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbReplyCollectionName")
    ));

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddAutoMapper(typeof(Program));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                context.Response.ContentType = Application.Json;

                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                Log.Error(exceptionHandlerPathFeature?.Error, "Error happened");

                if (exceptionHandlerPathFeature == null)
                {
                    return;
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    new
                    {
                        Message = app.Environment.IsDevelopment()
                            ? JsonSerializer.Serialize(exceptionHandlerPathFeature.Error)
                            : "Something went wrong"
                    })
                );
            });
        });

    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseHttpMetrics();

    app.UseAuthorization();

    app.MapControllers();

    app.MapMetrics();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}