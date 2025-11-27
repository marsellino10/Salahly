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
            RoleManager<IdentityRole<int>> roleManager)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Clear existing data in correct order (respecting foreign keys)
            await ClearAllDataAsync(context);

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Areas (required by Craftsmen's service areas)
            await SeedAreasAsync(context);

            // Seed Crafts (required by Craftsmen)
            await SeedCraftsAsync(context);

            // Seed Admin
            await SeedAdminAsync(userManager, context);

            // Seed Customers
            await SeedCustomersAsync(userManager, context);

            // Seed Craftsmen with service areas
            await SeedCraftsmenAsync(userManager, context);

            // Seed Portfolio Items
            await SeedPortfolioItemsAsync(context);

            // Seed Service Requests
            await SeedServiceRequestsAsync(context);

            // Seed Craftsman Offers
            await SeedCraftsmanOffersAsync(context);

            // Seed Bookings and Payments
            await SeedBookingsAndPaymentsAsync(context);

            // Seed Reviews
            await SeedReviewsAsync(context);

            // Seed Notifications
            await SeedNotificationsAsync(context);

            Console.WriteLine("✅ Database seeding completed successfully!");
        }

        private static async Task ClearAllDataAsync(ApplicationDbContext context)
        {
            // Execute in correct order to respect foreign key constraints
            try
            {
                // Delete in reverse dependency order
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Reviews]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Notifications]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Payments]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [PortfolioItems]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Bookings]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [CraftsmanOffers]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [ServiceRequests]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [CraftsmanServiceAreas]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Craftsmen]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Customers]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Admins]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Areas]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Crafts]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [RefreshTokens]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [UserLogins]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [UserTokens]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [UserClaims]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [RoleClaims]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [UserRoles]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Users]");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Roles]");

                // Reset identity seeds if using SQL Server
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[Crafts]', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[Areas]', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('[Users]', RESEED, 0)");

                Console.WriteLine("✅ Cleared all existing data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning during data cleanup: {ex.Message}");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            string[] roles = { "Admin", "Customer", "Craftsman" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"✅ Role '{role}' created");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task SeedAreasAsync(ApplicationDbContext context)
        {
            var areas = new List<Area>
            {
                // Cairo areas
                new Area { Region = "Cairo", City = "Nasr City" },
                new Area { Region = "Cairo", City = "Maadi" },
                new Area { Region = "Cairo", City = "Heliopolis" },
                new Area { Region = "Cairo", City = "Zamalek" },
                new Area { Region = "Cairo", City = "Downtown" },
                new Area { Region = "Cairo", City = "New Cairo" },
                // Giza areas
                new Area { Region = "Giza", City = "Dokki" },
                new Area { Region = "Giza", City = "Agouza" },
                new Area { Region = "Giza", City = "6th of October" },
                // Alexandria areas
                new Area { Region = "Alexandria", City = "Downtown" },
                new Area { Region = "Alexandria", City = "Montaza" },
                // Ismailia
                new Area { Region = "Ismailia", City = "Downtown" }
            };

            await context.Areas.AddRangeAsync(areas);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {areas.Count} areas");
        }

        private static async Task SeedCraftsAsync(ApplicationDbContext context)
        {
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

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                UserType = UserType.Admin,
                CreatedAt = DateTime.UtcNow,
                IsProfileCompleted = true,
                RatingAverage = 0
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin@123");

            if (createResult.Succeeded)
            {
                // Refresh user from database to ensure Id is populated
                var createdAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (createdAdmin != null)
                {
                    // Add to role
                    var roleResult = await userManager.AddToRoleAsync(createdAdmin, "Admin");
                    if (roleResult.Succeeded)
                    {
                        // Create Admin entity
                        var admin = new Admin
                        {
                            Id = createdAdmin.Id,
                            Department = "IT",
                            HiredAt = DateTime.UtcNow
                        };

                        await context.Admins.AddAsync(admin);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Admin created: {adminEmail} / Admin@123");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to add admin to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ Failed to create admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        private static async Task SeedCustomersAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            var customers = new []
            {
                new { Email = "ahmed.customer@salahly.com", FullName = "Ahmed Mohamed", City = "Cairo", Area = "Nasr City", Phone = "+201001234567" },
                new { Email = "sara.customer@salahly.com", FullName = "Sara Ali", City = "Cairo", Area = "Maadi", Phone = "+201001234568" },
                new { Email = "fatima.customer@salahly.com", FullName = "Fatima Hassan", City = "Giza", Area = "Dokki", Phone = "+201001234569" },
                new { Email = "layla.customer@salahly.com", FullName = "Layla Mohamed", City = "Cairo", Area = "Heliopolis", Phone = "+201001234570" }
            };

            int customerCount = 0;
            foreach (var customerData in customers)
            {
                var user = new ApplicationUser
                {
                    UserName = customerData.Email,
                    Email = customerData.Email,
                    FullName = customerData.FullName,
                    EmailConfirmed = true,
                    PhoneNumber = customerData.Phone,
                    IsActive = true,
                    UserType = UserType.Customer,
                    CreatedAt = DateTime.UtcNow,
                    IsProfileCompleted = true,
                    RatingAverage = 0
                };

                var createResult = await userManager.CreateAsync(user, "Customer@123");

                if (createResult.Succeeded)
                {
                    // Refresh user from database to ensure Id is populated
                    var createdUser = await userManager.FindByEmailAsync(customerData.Email);
                    if (createdUser != null)
                    {
                        // Add to role
                        var roleResult = await userManager.AddToRoleAsync(createdUser, "Customer");
                        if (roleResult.Succeeded)
                        {
                            var customer = new Customer
                            {
                                Id = createdUser.Id,
                                City = customerData.City,
                                Area = customerData.Area,
                                Address = $"123 Main Street, {customerData.Area}, {customerData.City}",
                                PhoneNumber = customerData.Phone,
                                DateOfBirth = new DateTime(1990, 5, 15)
                            };

                            await context.Customers.AddAsync(customer);
                            await context.SaveChangesAsync();
                            customerCount++;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Failed to add {customerData.Email} to Customer role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to create customer {customerData.Email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            Console.WriteLine($"✅ Seeded {customerCount} customers");
        }

        private static async Task SeedCraftsmenAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            // Get required data
            var electricianCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Electrician");
            var plumberCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Plumber");
            var carpenterCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Carpenter");
            var painterCraft = await context.Crafts.FirstOrDefaultAsync(c => c.Name == "Painter");

            var nasrCityArea = await context.Areas.FirstOrDefaultAsync(a => a.Region == "Cairo" && a.City == "Nasr City");
            var maadiArea = await context.Areas.FirstOrDefaultAsync(a => a.Region == "Cairo" && a.City == "Maadi");
            var dokkiArea = await context.Areas.FirstOrDefaultAsync(a => a.Region == "Giza" && a.City == "Dokki");
            var heliosArea = await context.Areas.FirstOrDefaultAsync(a => a.Region == "Cairo" && a.City == "Heliopolis");

            if (electricianCraft == null || plumberCraft == null || carpenterCraft == null || painterCraft == null)
            {
                Console.WriteLine("❌ Crafts not found. Seed crafts first.");
                return;
            }

            var craftsmen = new []
            {
                new
                {
                    Email = "ali.electrician@salahly.com",
                    FullName = "Ali Mahmoud",
                    CraftId = electricianCraft.Id,
                    Bio = "Experienced electrician with 10+ years in residential and commercial work",
                    YearsOfExperience = 10,
                    HourlyRate = 150m,
                    AreaId = nasrCityArea?.Id ?? 1,
                    Phone = "+201102234567"
                },
                new
                {
                    Email = "youssef.plumber@salahly.com",
                    FullName = "Youssef Ahmed",
                    CraftId = plumberCraft.Id,
                    Bio = "Professional plumber with 24/7 emergency services",
                    YearsOfExperience = 8,
                    HourlyRate = 120m,
                    AreaId = maadiArea?.Id ?? 2,
                    Phone = "+201102234568"
                },
                new
                {
                    Email = "omar.carpenter@salahly.com",
                    FullName = "Omar Khaled",
                    CraftId = carpenterCraft.Id,
                    Bio = "Custom furniture and carpentry services with modern designs",
                    YearsOfExperience = 12,
                    HourlyRate = 180m,
                    AreaId = dokkiArea?.Id ?? 7,
                    Phone = "+201102234569"
                },
                new
                {
                    Email = "hassan.electrician@salahly.com",
                    FullName = "Hassan Ibrahim",
                    CraftId = electricianCraft.Id,
                    Bio = "Residential and commercial electrical work with certification",
                    YearsOfExperience = 6,
                    HourlyRate = 130m,
                    AreaId = heliosArea?.Id ?? 3,
                    Phone = "+201102234570"
                },
                new
                {
                    Email = "amira.painter@salahly.com",
                    FullName = "Amira Hassan",
                    CraftId = painterCraft.Id,
                    Bio = "Interior and exterior painting specialist",
                    YearsOfExperience = 5,
                    HourlyRate = 100m,
                    AreaId = nasrCityArea?.Id ?? 1,
                    Phone = "+201102234571"
                }
            };

            int craftsmanCount = 0;
            foreach (var craftsmanData in craftsmen)
            {
                var user = new ApplicationUser
                {
                    UserName = craftsmanData.Email,
                    Email = craftsmanData.Email,
                    FullName = craftsmanData.FullName,
                    EmailConfirmed = true,
                    PhoneNumber = craftsmanData.Phone,
                    IsActive = true,
                    UserType = UserType.Craftsman,
                    CreatedAt = DateTime.UtcNow,
                    IsProfileCompleted = true,
                    RatingAverage = 0
                };

                var createResult = await userManager.CreateAsync(user, "Craftsman@123");

                if (createResult.Succeeded)
                {
                    // Refresh user from database to ensure Id is populated
                    var createdUser = await userManager.FindByEmailAsync(craftsmanData.Email);
                    if (createdUser != null)
                    {
                        // Add to role
                        var roleResult = await userManager.AddToRoleAsync(createdUser, "Craftsman");
                        if (roleResult.Succeeded)
                        {
                            var craftsman = new Craftsman
                            {
                                Id = createdUser.Id,
                                CraftId = craftsmanData.CraftId,
                                Bio = craftsmanData.Bio,
                                YearsOfExperience = craftsmanData.YearsOfExperience,
                                HourlyRate = craftsmanData.HourlyRate,
                                TotalCompletedBookings = 0,
                                IsAvailable = true,
                                IsVerified = true,
                                VerificationStatus = VerificationStatus.Verified
                            };

                            await context.Craftsmen.AddAsync(craftsman);
                            await context.SaveChangesAsync();

                            // Add service area
                            var serviceArea = new CraftsmanServiceArea
                            {
                                CraftsmanId = craftsman.Id,
                                AreaId = craftsmanData.AreaId,
                                ServiceRadiusKm = 15,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            await context.CraftsmanServiceAreas.AddAsync(serviceArea);
                            await context.SaveChangesAsync();
                            craftsmanCount++;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Failed to add {craftsmanData.Email} to Craftsman role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to create craftsman {craftsmanData.Email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            Console.WriteLine($"✅ Seeded {craftsmanCount} craftsmen with service areas");
        }

        private static async Task SeedPortfolioItemsAsync(ApplicationDbContext context)
        {
            var craftsmen = await context.Craftsmen.ToListAsync();

            var portfolioItems = new List<PortfolioItem>();
            var displayOrder = 1;

            // Portfolio items for Ali Mahmoud (Electrician)
            if (craftsmen.Count > 0)
            {
                portfolioItems.AddRange(new[]
                {
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[0].Id,
                        Title = "Modern Home Wiring Installation",
                        Description = "Complete electrical rewiring of a 3-bedroom apartment with smart home integration",
                        ImageUrl = "https://images.unsplash.com/photo-1621905251918-48416bd8575a?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6)
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[0].Id,
                        Title = "Commercial Panel Upgrade",
                        Description = "Upgraded electrical panel for small office building to handle increased load",
                        ImageUrl = "https://images.unsplash.com/photo-1581092918056-0c4c3acd3789?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-5)
                    }
                });
            }

            // Portfolio items for Youssef Ahmed (Plumber)
            if (craftsmen.Count > 1)
            {
                portfolioItems.AddRange(new[]
                {
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[1].Id,
                        Title = "Bathroom Renovation Plumbing",
                        Description = "Complete plumbing installation for modern bathroom renovation including heated towel rail",
                        ImageUrl = "https://images.unsplash.com/photo-1552321554-5fefe8c9ef14?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-5)
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[1].Id,
                        Title = "Kitchen Sink Installation",
                        Description = "Modern kitchen sink and faucet installation with water filtration system",
                        ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-4)
                    }
                });
            }

            // Portfolio items for Omar Khaled (Carpenter)
            if (craftsmen.Count > 2)
            {
                portfolioItems.AddRange(new[]
                {
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[2].Id,
                        Title = "Custom Wardrobe Design",
                        Description = "Custom-designed wooden wardrobe with sliding doors and internal organization",
                        ImageUrl = "https://images.unsplash.com/photo-1592078615290-033ee584e267?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-7)
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[2].Id,
                        Title = "Kitchen Cabinet Renovation",
                        Description = "Complete kitchen cabinet renovation with custom wood finishing and hardware upgrade",
                        ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6)
                    }
                });
            }

            // Portfolio items for Hassan Ibrahim (Electrician)
            if (craftsmen.Count > 3)
            {
                portfolioItems.AddRange(new[]
                {
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[3].Id,
                        Title = "Residential Solar Panel Installation",
                        Description = "Complete solar panel installation and electrical integration for residential home",
                        ImageUrl = "https://images.unsplash.com/photo-1628617048222-4b8ebadc2402?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-4)
                    }
                });
            }

            if (portfolioItems.Count > 0)
            {
                await context.PortfolioItems.AddRangeAsync(portfolioItems);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Seeded {portfolioItems.Count} portfolio items");
            }
        }

        private static async Task SeedServiceRequestsAsync(ApplicationDbContext context)
        {
            var customers = await context.Customers.ToListAsync();
            var crafts = await context.Crafts.ToListAsync();
            var areas = await context.Areas.ToListAsync();

            if (customers.Count == 0 || crafts.Count == 0 || areas.Count == 0)
            {
                Console.WriteLine("⏭️ Insufficient data for service requests, skipping...");
                return;
            }

            var serviceRequests = new List<ServiceRequest>();
            var baseDate = DateTime.UtcNow;

            // Service Request 1: Electrical work
            serviceRequests.Add(new ServiceRequest
            {
                CustomerId = customers[0].Id,
                CraftId = crafts[0].Id, // Electrician
                Title = "Rewire Living Room",
                Description = "Need to rewire the living room with new outlets and switch installation for smart home",
                Address = "123 Main Street, Nasr City, Cairo",
                AreaId = areas[0].Id, // Nasr City
                Latitude = 30.0444,
                Longitude = 31.3571,
                AvailableFromDate = baseDate.AddDays(7),
                AvailableToDate = baseDate.AddDays(14),
                CustomerBudget = 1500m,
                PaymentMethod = "Cash",
                Status = ServiceRequestStatus.Open,
                OffersCount = 0,
                ExpiresAt = baseDate.AddDays(30),
                CreatedAt = baseDate,
                MaxOffers = 5
            });

            // Service Request 2: Plumbing work
            serviceRequests.Add(new ServiceRequest
            {
                CustomerId = customers[1].Id,
                CraftId = crafts[1].Id, // Plumber
                Title = "Fix Leaking Bathroom",
                Description = "Water leaking from bathroom pipes. Need urgent inspection and repair",
                Address = "456 Garden Road, Maadi, Cairo",
                AreaId = areas[1].Id, // Maadi
                Latitude = 29.9667,
                Longitude = 31.3667,
                AvailableFromDate = baseDate.AddDays(1),
                AvailableToDate = baseDate.AddDays(3),
                CustomerBudget = 800m,
                PaymentMethod = "Card",
                Status = ServiceRequestStatus.Open,
                OffersCount = 0,
                ExpiresAt = baseDate.AddDays(30),
                CreatedAt = baseDate,
                MaxOffers = 5
            });

            // Service Request 3: Carpentry work
            serviceRequests.Add(new ServiceRequest
            {
                CustomerId = customers[2].Id,
                CraftId = crafts[2].Id, // Carpenter
                Title = "Build Wardrobe",
                Description = "Custom wooden wardrobe for bedroom with sliding doors and shelves",
                Address = "789 Tree Lane, Dokki, Giza",
                AreaId = areas[6].Id, // Dokki
                Latitude = 30.0131,
                Longitude = 31.2089,
                AvailableFromDate = baseDate.AddDays(10),
                AvailableToDate = baseDate.AddDays(20),
                CustomerBudget = 5000m,
                PaymentMethod = "Cash",
                Status = ServiceRequestStatus.Open,
                OffersCount = 0,
                ExpiresAt = baseDate.AddDays(45),
                CreatedAt = baseDate,
                MaxOffers = 5
            });

            // Service Request 4: Painting work
            serviceRequests.Add(new ServiceRequest
            {
                CustomerId = customers[3].Id,
                CraftId = crafts[3].Id, // Painter
                Title = "Paint Entire House",
                Description = "Interior and exterior painting for 2-story house with premium paint",
                Address = "321 Paint Street, Heliopolis, Cairo",
                AreaId = areas[2].Id, // Heliopolis
                Latitude = 30.0894,
                Longitude = 31.3505,
                AvailableFromDate = baseDate.AddDays(15),
                AvailableToDate = baseDate.AddDays(25),
                CustomerBudget = 3000m,
                PaymentMethod = "Cash",
                Status = ServiceRequestStatus.Open,
                OffersCount = 0,
                ExpiresAt = baseDate.AddDays(45),
                CreatedAt = baseDate,
                MaxOffers = 5
            });

            await context.ServiceRequests.AddRangeAsync(serviceRequests);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {serviceRequests.Count} service requests");
        }

        private static async Task SeedCraftsmanOffersAsync(ApplicationDbContext context)
        {
            var serviceRequests = await context.ServiceRequests.ToListAsync();
            var craftsmen = await context.Craftsmen.ToListAsync();

            if (serviceRequests.Count == 0 || craftsmen.Count == 0)
            {
                Console.WriteLine("⏭️ Insufficient data for craftsman offers, skipping...");
                return;
            }

            var offers = new List<CraftsmanOffer>();
            var baseDate = DateTime.UtcNow;

            // Offers for Service Request 1 (Electrician)
            if (serviceRequests.Count > 0)
            {
                var sr = serviceRequests[0];
                var electricians = craftsmen.Where(c => c.CraftId == sr.CraftId).Take(2).ToList();

                foreach (var electrician in electricians)
                {
                    offers.Add(new CraftsmanOffer
                    {
                        ServiceRequestId = sr.ServiceRequestId,
                        CraftsmanId = electrician.Id,
                        OfferedPrice = 1200m + (electrician.Id % 100),
                        Description = "I can handle this rewiring project efficiently with all required certifications",
                        EstimatedDurationMinutes = 480,
                        PreferredDate = baseDate.AddDays(8),
                        PreferredTimeSlot = "Morning",
                        Status = OfferStatus.Pending,
                        CreatedAt = baseDate
                    });
                }
            }

            // Offers for Service Request 2 (Plumber)
            if (serviceRequests.Count > 1)
            {
                var sr = serviceRequests[1];
                var plumbers = craftsmen.Where(c => c.CraftId == sr.CraftId).Take(2).ToList();

                foreach (var plumber in plumbers)
                {
                    offers.Add(new CraftsmanOffer
                    {
                        ServiceRequestId = sr.ServiceRequestId,
                        CraftsmanId = plumber.Id,
                        OfferedPrice = 700m + (plumber.Id % 50),
                        Description = "Emergency plumbing repair available. Can come within 2 hours",
                        EstimatedDurationMinutes = 120,
                        PreferredDate = baseDate.AddDays(1),
                        PreferredTimeSlot = "Afternoon",
                        Status = OfferStatus.Pending,
                        CreatedAt = baseDate
                    });
                }
            }

            // Offers for Service Request 3 (Carpenter)
            if (serviceRequests.Count > 2)
            {
                var sr = serviceRequests[2];
                var carpenters = craftsmen.Where(c => c.CraftId == sr.CraftId).Take(2).ToList();

                foreach (var carpenter in carpenters)
                {
                    offers.Add(new CraftsmanOffer
                    {
                        ServiceRequestId = sr.ServiceRequestId,
                        CraftsmanId = carpenter.Id,
                        OfferedPrice = 4800m + (carpenter.Id % 200),
                        Description = "Custom wardrobe with premium materials and finishing",
                        EstimatedDurationMinutes = 1440,
                        PreferredDate = baseDate.AddDays(12),
                        PreferredTimeSlot = "Morning",
                        Status = OfferStatus.Pending,
                        CreatedAt = baseDate
                    });
                }
            }

            await context.CraftsmanOffers.AddRangeAsync(offers);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {offers.Count} craftsman offers");
        }

        private static async Task SeedBookingsAndPaymentsAsync(ApplicationDbContext context)
        {
            var customers = await context.Customers.ToListAsync();
            var craftsmen = await context.Craftsmen.ToListAsync();
            var crafts = await context.Crafts.ToListAsync();
            var serviceRequests = await context.ServiceRequests.ToListAsync();
            var offers = await context.CraftsmanOffers.ToListAsync();

            if (customers.Count == 0 || craftsmen.Count == 0 || crafts.Count == 0)
            {
                Console.WriteLine("⏭️ Insufficient data for bookings, skipping...");
                return;
            }

            var bookings = new List<Booking>();
            var payments = new List<Payment>();
            var bookingDate = DateTime.UtcNow.AddMonths(-2);

            // Create completed booking 1 (with completed payment)
            if (serviceRequests.Count > 0 && offers.Count > 0)
            {
                var sr = serviceRequests[0];
                var offer = offers.FirstOrDefault(o => o.ServiceRequestId == sr.ServiceRequestId);

                if (offer != null)
                {
                    var booking = new Booking
                    {
                        CustomerId = sr.CustomerId,
                        CraftsmanId = offer.CraftsmanId,
                        CraftId = sr.CraftId,
                        ServiceRequestId = sr.ServiceRequestId,
                        AcceptedOfferId = offer.CraftsmanOfferId,
                        BookingDate = bookingDate.AddDays(5),
                        Duration = 8,
                        TotalAmount = 1200m,
                        Status = BookingStatus.Completed,
                        Notes = "Living room rewiring with smart home features",
                        RefundableAmount = 0,
                        CompletedAt = bookingDate.AddDays(5).AddHours(8),
                        CreatedAt = bookingDate,
                        CompletionNotes = "Work completed successfully. All outlets tested and working."
                    };

                    bookings.Add(booking);
                    await context.Bookings.AddAsync(booking);
                    await context.SaveChangesAsync();

                    // Add payment for completed booking
                    payments.Add(new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = 1200m,
                        PaymentDate = booking.BookingDate,
                        Status = PaymentStatus.Completed,
                        TransactionId = $"TXN{booking.BookingId:D6}001",
                        PaymentMethod = "Cash",
                        PaymentGateway = "Manual"
                    });
                }
            }

            // Create more completed bookings (without offers)
            for (int i = 1; i < Math.Min(customers.Count, 3); i++)
            {
                var customer = customers[i];
                var craftsman = craftsmen[i % craftsmen.Count];
                var craft = crafts[i % crafts.Count];

                var booking = new Booking
                {
                    CustomerId = customer.Id,
                    CraftsmanId = craftsman.Id,
                    CraftId = craft.Id,
                    ServiceRequestId = null,
                    AcceptedOfferId = 0,
                    BookingDate = bookingDate.AddDays(i * 10),
                    Duration = 4,
                    TotalAmount = 500m + (i * 100),
                    Status = BookingStatus.Completed,
                    Notes = $"Professional {craft.Name} work",
                    RefundableAmount = 0,
                    CompletedAt = bookingDate.AddDays(i * 10).AddHours(4),
                    CreatedAt = bookingDate.AddDays(i * 10),
                    CompletionNotes = "Great work! Completed as per requirements."
                };

                bookings.Add(booking);
                await context.Bookings.AddAsync(booking);
                await context.SaveChangesAsync();

                // Add payment
                payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = booking.TotalAmount,
                    PaymentDate = booking.BookingDate,
                    Status = PaymentStatus.Completed,
                    TransactionId = $"TXN{booking.BookingId:D6}001",
                    PaymentMethod = "Cash",
                    PaymentGateway = "Manual"
                });
            }

            if (payments.Count > 0)
            {
                await context.Payments.AddRangeAsync(payments);
                await context.SaveChangesAsync();
            }
            
            Console.WriteLine($"✅ Seeded {bookings.Count} completed bookings with payments");
        }

        private static async Task SeedReviewsAsync(ApplicationDbContext context)
        {
            var bookings = await context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .Include(b => b.Customer)
                .ThenInclude(c => c.User)
                .Include(b => b.Craftsman)
                .ThenInclude(c => c.User)
                .ToListAsync();

            if (bookings.Count == 0)
            {
                Console.WriteLine("⏭️ No completed bookings for reviews, skipping...");
                return;
            }

            var reviews = new List<Review>();

            for (int i = 0; i < bookings.Count; i++)
            {
                var booking = bookings[i];

                // Ensure relationships are loaded
                if (booking.Customer?.User == null || booking.Craftsman?.User == null)
                {
                    Console.WriteLine($"⚠️ Skipping review for booking {booking.BookingId} - missing user data");
                    continue;
                }

                // Customer review for craftsman
                var customerReview = new Review
                {
                    ReviewerUserId = booking.Customer.User.Id,
                    TargetUserId = booking.Craftsman.User.Id,
                    BookingId = booking.BookingId,
                    Rating = 4 + (i % 2),
                    Comment = i switch
                    {
                        0 => "Excellent work! Professional and punctual. Highly recommended.",
                        1 => "Great service! Fixed all issues efficiently and within budget.",
                        2 => "Very skilled and friendly. Completed the job perfectly on time.",
                        _ => "Perfect service, exceeded my expectations!"
                    },
                    CreatedAt = booking.CompletedAt!.Value.AddDays(1)
                };
                reviews.Add(customerReview);

                // Craftsman review for customer
                var craftsmanReview = new Review
                {
                    ReviewerUserId = booking.Craftsman.User.Id,
                    TargetUserId = booking.Customer.User.Id,
                    BookingId = booking.BookingId,
                    Rating = 4 + (i % 2),
                    Comment = i switch
                    {
                        0 => "Excellent customer! Clear requirements and very cooperative.",
                        1 => "Very pleasant and professional. Great communication.",
                        2 => "Professional client with clear expectations. Smooth process.",
                        _ => "Great experience! Would work with again."
                    },
                    CreatedAt = booking.CompletedAt!.Value.AddDays(2)
                };
                reviews.Add(craftsmanReview);
            }

            if (reviews.Count > 0)
            {
                await context.Reviews.AddRangeAsync(reviews);

                // Update craftsman completed bookings count and ratings
                var craftsmenIds = bookings.Select(b => b.CraftsmanId).Distinct();
                foreach (var craftsmanId in craftsmenIds)
                {
                    var craftsman = await context.Craftsmen.FindAsync(craftsmanId);
                    if (craftsman != null)
                    {
                        var completedCount = bookings.Count(b => b.CraftsmanId == craftsmanId);
                        craftsman.TotalCompletedBookings = completedCount;

                        // Update craftsman user rating
                        var craftsmanUser = craftsman.User ?? await context.Users.FindAsync(craftsman.Id);
                        if (craftsmanUser != null)
                        {
                            var craftsmanReviews = reviews
                                .Where(r => r.TargetUserId == craftsmanUser.Id)
                                .Select(r => (double)r.Rating)
                                .ToList();

                            if (craftsmanReviews.Count > 0)
                            {
                                craftsmanUser.RatingAverage = craftsmanReviews.Average();
                            }
                        }
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Seeded {reviews.Count} reviews");
            }
            else
            {
                Console.WriteLine("⏭️ No reviews created - insufficient booking/user data");
            }
        }

        private static async Task SeedNotificationsAsync(ApplicationDbContext context)
        {
            var users = await context.Users.ToListAsync();
            var bookings = await context.Bookings.ToListAsync();

            if (users.Count == 0)
            {
                Console.WriteLine("⏭️ No users for notifications, skipping...");
                return;
            }

            var notifications = new List<Notification>();
            var baseDate = DateTime.UtcNow;

            // Send welcome notifications to users
            foreach (var user in users.Take(5))
            {
                notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Type = NotificationType.BookingConfirmed,
                    Title = "Welcome to Salahly!",
                    Message = $"Welcome {user.FullName}! Your account has been successfully created.",
                    ActionUrl = "/dashboard",
                    IsRead = false,
                    CreatedAt = baseDate
                });
            }

            // Add booking notifications for completed bookings
            foreach (var booking in bookings.Where(b => b.Status == BookingStatus.Completed).Take(3))
            {
                notifications.Add(new Notification
                {
                    UserId = booking.CustomerId,
                    Type = NotificationType.BookingCompleted,
                    Title = "Booking Completed",
                    Message = "Your booking has been completed successfully!",
                    ActionUrl = $"/bookings/{booking.BookingId}",
                    BookingId = booking.BookingId,
                    IsRead = true,
                    CreatedAt = baseDate.AddDays(-1)
                });
            }

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {notifications.Count} notifications");
        }
    }
}