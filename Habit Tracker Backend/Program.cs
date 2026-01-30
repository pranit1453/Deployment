using Habit_Tracker_Backend.Configurations;
using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.Middleware;
using Habit_Tracker_Backend.Services.Implementations;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace Habit_Tracker_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            builder.Services.Configure<JwtOptions>(
                configuration.GetSection("Jwt"));

            var jwtOptions = configuration.GetSection("Jwt")
                .Get<JwtOptions>()
                ?? throw new InvalidOperationException("JWT configuration is missing");

            if (jwtOptions.Key.Length < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 characters long");

            
            builder.Services.Configure<EmailOptions>(
                configuration.GetSection("Email"));

            
            builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler =
                        ReferenceHandler.IgnoreCycles;

                        options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;

                        options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter()
                        );
                    });
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Habit Tracker API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer"
                        },
                        Array.Empty<string>()
                    }
                });
            });


            var connectionString =
                        configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException(
                        "Database connection string 'DefaultConnection' is missing");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });


            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IHabitService, HabitService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IHabitLogService, HabitLogService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<IReminderService, ReminderService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddHostedService<ReminderBackgroundService>();


            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),

                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

           
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    if (allowedOrigins != null && allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });



            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("login-limiter", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 10;
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.AddFixedWindowLimiter("otp-limiter", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 5;
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });

            var app = builder.Build();

            app.UseRouting();

            app.UseMiddleware<GlobalExceptionMiddleware>();

            app.UseCors("AllowFrontend");

            app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/", () => Results.Ok());

            app.Run();
        }
    }
}
