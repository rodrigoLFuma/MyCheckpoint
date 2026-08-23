using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Middleware;
using MyCheckpoint.Services;
using MyCheckpoint.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameListRepository, GameListRepository>();
builder.Services.AddScoped<IGameListItemRepository, GameListItemRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();
builder.Services.AddScoped<IRepositoryBaseWithId<Category>, RepositoryBaseWithId<Category>>();

builder.Services.AddScoped<IGameListService, GameListService>();

builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
