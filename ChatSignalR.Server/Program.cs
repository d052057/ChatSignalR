using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ChatSignalR.Server.Dto;
using ChatSignalR.Server.Hubs;
using ChatSignalR.Server.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeDLSharp;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR(opt =>
    { 
    opt.EnableDetailedErrors = true; 
    });
builder.Services.AddSingleton<DownloadService>(); // Registers the DownloadService as a singleton

builder.Services.AddHttpContextAccessor(); // Required for accessing HTTP context, like connection IDs
builder.Services.AddCors();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(opt =>
{
//opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    opt.WithOrigins("https://localhost:50968", "https://127.0.0.1:50968")
       .AllowAnyHeader()
       .AllowAnyMethod()
       .AllowCredentials(); // Required if credentials are sent
});
app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSpa(spa => { });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DownloadHub>("/downloadHub");
app.MapFallbackToFile("/index.html");


app.Run();

