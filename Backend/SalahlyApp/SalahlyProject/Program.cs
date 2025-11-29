using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DAL.Repositories;
using Salahly.DAL.Services;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Orchestrator;
using Salahly.DSL.Interfaces.Payments;
using Salahly.DSL.Services;
using Salahly.DSL.Services.Orchestrator;
using Salahly.DSL.Services.Payments;
using SalahlyProject.Api.Hubs;
using SalahlyProject.Options;
using SalahlyProject.Response;
using SalahlyProject.Response.Error;
using SalahlyProject.Services;
using SalahlyProject.Services.Chat;
using SalahlyProject.Services.Interfaces;
using System.Text;

namespace SalahlyProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // ========================================
            // 1. DATABASE CONFIGURATION
            // ========================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("Salahly.DAL");

                    // Enable retry on failure
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                // Enable detailed errors in development
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // ========================================
            // 2.1 IDENTITY CONFIGURATION
            // ========================================
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Email confirmation
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 2.2 JWT settings
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            builder.Services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            if (jwtSettings == null)
                throw new Exception("JwtSettings not found in configuration.");

            // 2.3 Authentication with JWT Bearer
            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Accept JWT in the query string ONLY for SignalR WebSockets
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // ========================================
            // 3. DEPENDENCY INJECTION
            // ========================================

            // Repository Pattern
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Auth & User Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICustomerService, CustomerServicecs>();
            builder.Services.AddScoped<IAdminService, AdminService>();

            // Business Services
            builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
            builder.Services.AddScoped<ICraftService, CraftService>();
            builder.Services.AddScoped<IAreaService, AreaService>();
            builder.Services.AddScoped<ICraftsManService, CraftsManService>();
            builder.Services.AddScoped<IPortfolioService, PortfolioService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();

            // ✅ Payment Services
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();
            builder.Services.AddScoped<PaymobPaymentStrategy>();
            builder.Services.AddScoped<PaymobWalletPaymentStrategy>();
            builder.Services.AddScoped<CashPaymentStrategy>();
            builder.Services.AddScoped<IAcceptOrchestrator, OfferAcceptanceOrchestrator>();
            builder.Services.AddScoped<IFailedOrchestrator, PaymentFailureOrchestrator>();

            // ✅ HttpClient for Paymob API
            builder.Services.AddHttpClient<PaymobPaymentStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            builder.Services.AddHttpClient<PaymobWalletPaymentStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // SignalR & Notifications
            builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<INotificationHubSender, NotificationHubSender>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // File Upload Service
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddHttpContextAccessor();

            // Chatbot Services
            builder.Services.Configure<FireworksOptions>(
                configuration.GetSection(FireworksOptions.SectionName));
            builder.Services.AddSingleton<IChatContextBuilder, ChatContextBuilder>();
            builder.Services.AddHttpClient<IChatService, FireworksChatService>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FireworksOptions>>().Value;

                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    var baseUrl = options.BaseUrl.EndsWith('/')
                        ? options.BaseUrl
                        : options.BaseUrl + "/";
                    client.BaseAddress = new Uri(baseUrl);
                }

                client.Timeout = TimeSpan.FromSeconds(45);
            });

            // ========================================
            // 4. CORS CONFIGURATION
            // ========================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()    
                          .AllowAnyMethod()     
                          .AllowAnyHeader();    
                });

                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:4200",
                              "http://127.0.0.1:3000"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // ========================================
            // 5. SWAGGER CONFIGURATION
            // ========================================
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Salahly API",
                    Version = "v1",
                    Description = "A Web API for managing Craftsman, Customer, Bookings, and Payments"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT as: **Bearer {your token}**"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ========================================
            // 6. CONTROLLERS
            // ========================================
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Handle circular references
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    // Use camelCase for JSON
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // ========================================
            // 7. MAPSTER CONFIGURATION
            // ========================================
            MapsterConfiguration.RegisterMappings();

            // ========================================
            // 8. CUSTOM VALIDATION RESPONSE
            // ========================================
            builder.Services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(e => e.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    var validationErrorResponse = new ValidationErrorResponse
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });

            // ========================================
            // BUILD APP
            // ========================================
            var app = builder.Build();

            // ✅ SEED DATABASE (Commented out - uncomment when needed)
            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    try
            //    {
            //        var context = services.GetRequiredService<ApplicationDbContext>();
            //        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            //        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

            //        Console.WriteLine("🌱 Starting database seeding...");
            //        await DbSeeder.SeedAsync(context, userManager, roleManager);
            //        Console.WriteLine("✅ Database seeding completed!");
            //    }
            //    catch (Exception ex)
            //    {
            //        var logger = services.GetRequiredService<ILogger<Program>>();
            //        logger.LogError(ex, "❌ An error occurred while seeding the database.");
            //    }
            //}

            // ========================================
            // MIDDLEWARE PIPELINE
            // ========================================

            // Development environment
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // Production error handling
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // ✅ CORS - Must be before Authentication/Authorization
            app.UseCors("AllowAll");  // ⬅️ يسمح لـ Paymob webhook

            // Static files (if needed)
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controllers and hubs
            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();
        }
    }
}