using BackgroundService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ServiceData>();
builder.Services.AddHostedService<BackgroundRefresh>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/messages", (ServiceData data) => data.Data.Order());

app.Run();
