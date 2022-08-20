using LifePlanner.Api;
using LifePlanner.Api.Telegram;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddEnumsWithValuesFixFilters();
});
builder.Services.AddNpgsql<DatabaseContext>(builder.Configuration["Db:LifePlanner"]);
builder.Services.AddScoped<IActivityManager, ActivityManager>();
builder.Services.AddHostedService<TelegramBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.GetService(typeof(TelegramBackgroundService));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();