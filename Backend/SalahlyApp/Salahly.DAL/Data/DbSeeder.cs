using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Crafts
            await SeedCraftsAsync(context);

            // Seed Admin
            await SeedAdminAsync(userManager, context);

            // Seed Customers
            await SeedCustomersAsync(userManager, context);

            // Seed Craftsmen
            await SeedCraftsmenAsync(userManager, context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Customer", "Craftsman" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✅ Role '{role}' created");
                }
            }
        }

        private static async Task SeedCraftsAsync(ApplicationDbContext context)
        {
            if (await context.Crafts.AnyAsync())
            {
                Console.WriteLine("⏭️ Crafts already exist, skipping...");
                return;
            }

            var crafts = new List<Craft>
            {
                new Craft
                {
                    Name = "Electrician",
                    Description = "Electrical repairs, installations, and maintenance",
                    IconUrl = "/icons/electrician.svg",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "Plumber",
                    Description = "Plumbing services, pipe repairs, and installations",
                    IconUrl = "/icons/plumber.svg",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "Carpenter",
                    Description = "Carpentry and woodwork services",
                    IconUrl = "/icons/carpenter.svg",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "Painter",
                    Description = "Interior and exterior painting services",
                    IconUrl = "/icons/painter.svg",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "AC Technician",
                    Description = "Air conditioning repair and maintenance",
                    IconUrl = "/icons/ac-technician.svg",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "Handyman",
                    Description = "General home repair and maintenance services",
                    IconUrl = "/icons/handyman.svg",
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "Cleaner",
                    Description = "Professional cleaning services",
                    IconUrl = "/icons/cleaner.svg",
                    IsActive = true,
                    DisplayOrder = 7,
                    CreatedAt = DateTime.UtcNow
                },
                new Craft
                {
                    Name = "Gardener",
                    Description = "Garden maintenance and landscaping",
                    IconUrl = "/icons/gardener.svg",
                    IsActive = true,
                    DisplayOrder = 8,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Crafts.AddRangeAsync(crafts);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {crafts.Count} crafts");
        }

        private static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            var adminEmail = "admin@salahly.com";

            if (await userManager.FindByEmailAsync(adminEmail) != null)
            {
                Console.WriteLine("⏭️ Admin already exists, skipping...");
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                UserType = UserType.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");

                var admin = new Admin
                {
                    Id = adminUser.Id,
                    Department = "IT",
                    HiredAt = DateTime.UtcNow
                };

                await context.Admins.AddAsync(admin);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Admin created: {adminEmail} / Admin@123");
            }
        }

        private static async Task SeedCustomersAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            var customers = new[]
            {
                new { Email = "ahmed.customer@salahly.com", FullName = "Ahmed Mohamed", City = "Cairo", Area = "Nasr City" },
                new { Email = "sara.customer@salahly.com", FullName = "Sara Ali", City = "Cairo", Area = "Maadi" },
                new { Email = "mohamed.customer@salahly.com", FullName = "Mohamed Hassan", City = "Giza", Area = "Dokki" }
            };

            foreach (var customerData in customers)
            {
                if (await userManager.FindByEmailAsync(customerData.Email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = customerData.Email,
                    Email = customerData.Email,
                    FullName = customerData.FullName,
                    EmailConfirmed = true,
                    IsActive = true,
                    UserType = UserType.Customer,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Customer@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");

                    var customer = new Customer
                    {
                        Id = user.Id,
                        City = customerData.City,
                        Area = customerData.Area,
                        Address = $"123 Main Street, {customerData.Area}",
                        PhoneNumber = "+201234567890"
                    };

                    await context.Customers.AddAsync(customer);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {customers.Length} customers");
        }

        private static async Task SeedCraftsmenAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            // Get crafts
            var electricianCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Electrician");
            var plumberCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Plumber");
            var carpenterCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Carpenter");

            if (electricianCraft == null || plumberCraft == null || carpenterCraft == null)
            {
                Console.WriteLine("❌ Crafts not found. Seed crafts first.");
                return;
            }

            var craftsmen = new[]
            {
                new
                {
                    Email = "ali.electrician@salahly.com",
                    FullName = "Ali Mahmoud",
                    CraftId = electricianCraft.Id,
                    Bio = "Experienced electrician with 10+ years",
                    YearsOfExperience = 10,
                    HourlyRate = 150m,
                    City = "Cairo",
                    Area = "Nasr City"
                },
                new
                {
                    Email = "youssef.plumber@salahly.com",
                    FullName = "Youssef Ahmed",
                    CraftId = plumberCraft.Id,
                    Bio = "Professional plumber, available 24/7",
                    YearsOfExperience = 8,
                    HourlyRate = 120m,
                    City = "Cairo",
                    Area = "Maadi"
                },
                new
                {
                    Email = "omar.carpenter@salahly.com",
                    FullName = "Omar Khaled",
                    CraftId = carpenterCraft.Id,
                    Bio = "Custom furniture and carpentry services",
                    YearsOfExperience = 12,
                    HourlyRate = 180m,
                    City = "Giza",
                    Area = "Dokki"
                },
                new
                {
                    Email = "hassan.electrician@salahly.com",
                    FullName = "Hassan Ibrahim",
                    CraftId = electricianCraft.Id,
                    Bio = "Residential and commercial electrical work",
                    YearsOfExperience = 6,
                    HourlyRate = 130m,
                    City = "Cairo",
                    Area = "Heliopolis"
                }
            };

            foreach (var craftsmanData in craftsmen)
            {
                if (await userManager.FindByEmailAsync(craftsmanData.Email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = craftsmanData.Email,
                    Email = craftsmanData.Email,
                    FullName = craftsmanData.FullName,
                    EmailConfirmed = true,
                    IsActive = true,
                    UserType = UserType.Craftsman,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Craftsman@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Craftsman");

                    var craftsman = new Craftsman
                    {
                        Id = user.Id,
                        CraftId = craftsmanData.CraftId,
                        Bio = craftsmanData.Bio,
                        YearsOfExperience = craftsmanData.YearsOfExperience,
                        HourlyRate = craftsmanData.HourlyRate,
                        RatingAverage = 0,
                        TotalCompletedBookings = 0,
                        IsAvailable = true
                    };

                    await context.Craftsmen.AddAsync(craftsman);
                    await context.SaveChangesAsync();

                    // Add service area
                    var serviceArea = new CraftsmanServiceArea
                    {
                        CraftsmanId = craftsman.Id,
                        City = craftsmanData.City,
                        Area = craftsmanData.Area,
                        ServiceRadiusKm = 10,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.CraftsmanServiceAreas.AddAsync(serviceArea);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {craftsmen.Length} craftsmen with service areas");
        }
    }
}