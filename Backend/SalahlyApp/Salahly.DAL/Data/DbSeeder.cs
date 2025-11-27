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

            // Seed Portfolio Items
            await SeedPortfolioItemsAsync(context);

            // Seed Bookings and Reviews
            await SeedBookingsAndReviewsAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            string[] roles = { "Admin", "Customer", "Craftsman" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
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
                        TotalCompletedBookings = 0,
                        IsAvailable = true
                    };
                    await context.Craftsmen.AddAsync(craftsman);
                    await context.SaveChangesAsync();

                    // Ensure canonical Area exists (use City as Region and Area as City in Area table)
                    var existingArea = await context.Areas.FirstOrDefaultAsync(a => a.Region == craftsmanData.City && a.City == craftsmanData.Area);
                    if (existingArea == null)
                    {
                        existingArea = new Area
                        {
                            Region = craftsmanData.City,
                            City = craftsmanData.Area
                        };

                        await context.Areas.AddAsync(existingArea);
                        await context.SaveChangesAsync();
                    }

                    // Add service area linking to canonical Area
                    var serviceArea = new CraftsmanServiceArea
                    {
                        CraftsmanId = craftsman.Id,
                        AreaId = existingArea.Id,
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

        private static async Task SeedPortfolioItemsAsync(ApplicationDbContext context)
        {
            if (await context.PortfolioItems.AnyAsync())
            {
                Console.WriteLine("⏭️ Portfolio items already exist, skipping...");
                return;
            }

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
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[0].Id,
                        Title = "LED Lighting Installation",
                        Description = "Energy-efficient LED lighting installation for residential property",
                        ImageUrl = "https://images.unsplash.com/photo-1565636192335-14f13faf58ab?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-4)
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
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[1].Id,
                        Title = "Leak Repair and Pipe Maintenance",
                        Description = "Successfully diagnosed and repaired hidden water leaks in wall pipes",
                        ImageUrl = "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3)
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
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[2].Id,
                        Title = "Wooden Bookshelf Installation",
                        Description = "Custom wooden bookshelves with integrated lighting and premium finish",
                        ImageUrl = "https://images.unsplash.com/photo-1618005182384-a83a8e3a7f1f?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2)
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
                    },
                    new PortfolioItem
                    {
                        CraftsmanId = craftsmen[3].Id,
                        Title = "Electrical Safety Inspection",
                        Description = "Comprehensive electrical safety audit and remediation for commercial building",
                        ImageUrl = "https://images.unsplash.com/photo-1621905251918-48416bd8575a?w=400&h=300&fit=crop",
                        DisplayOrder = displayOrder++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3)
                    }
                });
            }

            await context.PortfolioItems.AddRangeAsync(portfolioItems);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {portfolioItems.Count} portfolio items");
        }

        private static async Task SeedBookingsAndReviewsAsync(ApplicationDbContext context)
        {
            if (await context.Reviews.AnyAsync())
            {
                Console.WriteLine("⏭️ Reviews already exist, skipping...");
                return;
            }

            var customers = await context.Customers.Include(c => c.User).ToListAsync();
            var craftsmen = await context.Craftsmen.Include(c => c.User).ToListAsync();
            var crafts = await context.Crafts.ToListAsync();

            if (customers.Count == 0 || craftsmen.Count == 0 || crafts.Count == 0)
            {
                Console.WriteLine("❌ Insufficient data to seed bookings and reviews");
                return;
            }

            var bookings = new List<Booking>();
            var bookingDate = DateTime.UtcNow.AddMonths(-3);

            // Create completed bookings for reviews
            for (int i = 0; i < Math.Min(customers.Count, 3); i++)
            {
                var customer = customers[i];
                var craftsman = craftsmen[i % craftsmen.Count];
                var craft = crafts[0];

                var booking = new Booking
                {
                    CustomerId = customer.Id,
                    CraftsmanId = craftsman.Id,
                    CraftId = craft.Id,
                    BookingDate = bookingDate.AddDays(i * 10),
                    Duration = 2,
                    TotalAmount = 300 + (i * 50),
                    Status = BookingStatus.Completed,
                    Notes = $"Professional work completed on schedule",
                    RefundableAmount = 0,
                    CompletedAt = bookingDate.AddDays(i * 10).AddHours(2),
                    CreatedAt = bookingDate.AddDays(i * 10),
                    CompletionNotes = "Great job, everything works perfectly!"
                };

                bookings.Add(booking);
            }

            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();

            // Create reviews for the bookings
            var reviews = new List<Review>();

            for (int i = 0; i < bookings.Count; i++)
            {
                var booking = bookings[i];
                var customer = customers[i];
                var craftsman = craftsmen[i % craftsmen.Count];

                // Customer review for craftsman
                var customerReview = new Review
                {
                    ReviewerUserId = customer.User.Id,
                    TargetUserId = craftsman.User.Id,
                    BookingId = booking.BookingId,
                    Rating = 4 + (i % 2), // 4-5 stars
                    Comment = i switch
                    {
                        0 => "Excellent work! Professional and punctual. Would definitely hire again.",
                        1 => "Great service! Fixed all the issues efficiently. Highly recommended.",
                        2 => "Very skilled and friendly. Completed the job perfectly on time.",
                        _ => "Perfect service, exactly what was needed!"
                    },
                    CreatedAt = booking.CompletedAt!.Value.AddDays(1)
                };
                reviews.Add(customerReview);

                // Craftsman review for customer
                var craftsmanReview = new Review
                {
                    ReviewerUserId = craftsman.User.Id,
                    TargetUserId = customer.User.Id,
                    BookingId = booking.BookingId,
                    Rating = 4 + (i % 2), // 4-5 stars
                    Comment = i switch
                    {
                        0 => "Excellent customer! Clear requirements and easy to work with.",
                        1 => "Very cooperative and pleasant. Great communication throughout.",
                        2 => "Professional client with clear expectations. Smooth transaction.",
                        _ => "Great experience working with this customer!"
                    },
                    CreatedAt = booking.CompletedAt!.Value.AddDays(2)
                };
                reviews.Add(craftsmanReview);
            }

            await context.Reviews.AddRangeAsync(reviews);

            // Update craftsman completed bookings count
            foreach (var craftsman in craftsmen)
            {
                craftsman.TotalCompletedBookings = bookings.Count(b => b.CraftsmanId == craftsman.Id);
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {bookings.Count} completed bookings");
            Console.WriteLine($"✅ Seeded {reviews.Count} reviews");
        }
    }
}