global using Microsoft.AspNetCore.SignalR;
using MAUIBroadcastServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapHub<BroadcastHub>("/BroadcastHub");

app.Run();