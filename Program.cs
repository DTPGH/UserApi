using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApi.Services;
using UserApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// DI swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "User API",
        Version = "v1",
        Description = "A simple example ASP.NET Core Web API for managing users",
    });
}
);

// DI DbContext
builder.Services.AddDbContext<UserApi.Data.AppDbContext>(
    options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("UserApiConnection"))
);

// DI UserService
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "User API V1");
            options.RoutePrefix = "swagger"; // Set Swagger UI at the app's root
        }
    );
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

