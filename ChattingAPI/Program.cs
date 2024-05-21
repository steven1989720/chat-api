using ChattingAPI;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day) // File sink
.CreateLogger();

builder.Services.AddSingleton<ChatServer>();

// Add services to the container.
builder.Services.AddSingleton<ChatDbContext>();

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseAuthorization();
app.MapControllers();

app.UseStaticFiles();
app.MapRazorPages();
app.UseRouting();

var chatServer = app.Services.GetService<ChatServer>();
if (chatServer != null)
    chatServer.Start("http://127.0.0.1:8081/");

app.Run();
