using RockPaperScissors.WebApi.Hubs;
using RockPaperScissors.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = configuration.GetConnectionString("Redis");
});

services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://frontend:80")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

services.AddScoped<IGameService, GameService>();

services.AddSignalR();

var app = builder.Build();

app.UseCors();

app.MapHub<GameHub>("/game");

app.Run();