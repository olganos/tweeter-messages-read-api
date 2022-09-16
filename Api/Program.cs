using Core;

using Infrastructure.Repositories;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Prometheus;

using Serilog;

using System.Text.Json;

using static System.Net.Mime.MediaTypeNames;

var logConfig = new LoggerConfiguration()
    .MinimumLevel.Warning();

if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LOGSTASH_URI")))
{
    logConfig
        .WriteTo
        .Http(Environment.GetEnvironmentVariable("LOGSTASH_URI"), null);
}
else
{
    logConfig
        .WriteTo
        .Console();
}

Log.Logger = logConfig.CreateLogger();

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
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbReplyCollectionName"),
         Environment.GetEnvironmentVariable("DB_LIKE_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbLikeCollectionName")
    ));

    builder.Services.AddSingleton<IUserRepository>(sp => new UserRepository(
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString"),
        Environment.GetEnvironmentVariable("DB_NAME")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbName"),
        Environment.GetEnvironmentVariable("DB_USER_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbUserCollectionName")
    ));

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = Environment.GetEnvironmentVariable("IDENTITY_SERVER_URI")
                ?? builder.Configuration.GetValue<string>("IdentityServer:Uri");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiScope", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "tweeter-api");
        });
    });

    builder.Services.AddSwaggerGen(c =>
    {
        c.EnableAnnotations();
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"Enter 'Bearer [space] and your token",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                },
                Scheme="oauth2",
                Name="Bearer",
                In=ParameterLocation.Header
            },
            new List<string>()
        }
        });
    });

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

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers()
        .RequireAuthorization("ApiScope");

    app.MapMetrics();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}