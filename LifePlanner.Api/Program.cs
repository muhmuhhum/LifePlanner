using LifePlanner.Api;
using LifePlanner.Api.Store;
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
builder.Services.AddSingleton<ITelegramService, TelegramService>();
builder.Services.AddScoped<IActivityStore, ActivityStore>();
builder.Services.AddScoped<IUserStore, UserStore>();
builder.Services.AddHostedService<ActivityHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

await app.Services.GetService<ITelegramService>()!.Init();

app.MapControllers();

app.Run();