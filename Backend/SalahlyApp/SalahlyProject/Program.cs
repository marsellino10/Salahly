using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Interfaces;
using Salahly.DAL.Repositories;

namespace SalahlyProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            // 2. IDENTITY CONFIGURATION
            // ========================================
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Email confirmation (false for development, true for production)
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ========================================
            // 3. DEPENDENCY INJECTION
            // ========================================
            // Repository Pattern
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add your services here when you create them
            // builder.Services.AddScoped<IAuthService, AuthService>();
            // builder.Services.AddScoped<ICustomerService, CustomerService>();
            // builder.Services.AddScoped<ICraftsmanService, CraftsmanService>();

            // ========================================
            // 4. CORS CONFIGURATION
            // ========================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200") // Angular default port
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // ========================================
            // 5. CONTROLLERS
            // ========================================
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Handle circular references
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    // Use camelCase for JSON
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // ========================================
            // BUILD APP
            // ========================================
            var app = builder.Build();

            // ✅ SEED DATABASE
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    Console.WriteLine("🌱 Starting database seeding...");
                    await DbSeeder.SeedAsync(context, userManager, roleManager);
                    Console.WriteLine("✅ Database seeding completed!");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "❌ An error occurred while seeding the database.");
                }
            }

            // ========================================
            // MIDDLEWARE PIPELINE
            // ========================================

            // Development environment
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Production error handling
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // CORS - Must be before Authentication/Authorization
            app.UseCors("AllowAngular");

            // Static files (if needed)
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controllers
            app.MapControllers();

            app.Run();
        }
    }
}