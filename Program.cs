using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UserApi.DTOs.Responses;
using UserApi.Middlewares;
using UserApi.Models;
using UserApi.Services;
using UserApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// customize Validation Response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList()
            );

        var response = new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Dữ liệu đầu vào không hợp lệ",
            Content = errors
        };
        return new BadRequestObjectResult(response);
    };
}
);

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

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token theo format: bearer {your token}",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
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

// DI ProjectService
builder.Services.AddScoped<IProjectService, ProjectService>();

// DI TaskItemService\
builder.Services.AddScoped<ITaskItemService, TaskItemService>();

// DI DashboardService
builder.Services.AddScoped<IDashboardService, DashboardService>();

// DI jwt authentication/authorization
var jwtSetting = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSetting["SecretKey"]
    ?? throw new InvalidOperationException("Jwt SecretKey is missing");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSetting["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSetting["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Bạn cần đăng nhập để sử dụng chức năng này.",
                    Content = null
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Bạn không có quyền thực hiện chức năng này.",
                    Content = null
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            }
        };
    });

builder.Services.AddAuthorization();

// DI JwtTokenService
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// DI AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// configure CORS allow frontend connect
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5295")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Global Exception Handling Middleware xử lý lỗi ngoài dự kiến như database lỗi,...-> trả mess lỗi chung chung cho client 
app.UseMiddleware<GlobalExceptionMiddleware>();

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

app.UseHttpsRedirection();

app.UseCors("BlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

