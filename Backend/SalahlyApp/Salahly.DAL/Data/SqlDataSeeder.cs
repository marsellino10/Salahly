using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Data
{
    public static class SqlDataSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // Run migrations
            await context.Database.MigrateAsync();

            using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                // Roles
                var roles = new[] { "Admin", "Craftsman", "Customer" };
                foreach (var r in roles)
                {
                    if (!await roleManager.RoleExistsAsync(r))
                        await roleManager.CreateAsync(new IdentityRole<int>(r));
                }

                // Crafts (15)
                if (!await context.Crafts.AnyAsync())
                {
                    var crafts = new List<Craft>
                    {
                        new Craft { Name = "Plumbing", Description = "Plumbing services, repairs and installations", IsActive = true, DisplayOrder = 1 },
                        new Craft { Name = "Electrician", Description = "Electrical works, wiring, and troubleshooting", IsActive = true, DisplayOrder = 2 },
                        new Craft { Name = "Carpentry", Description = "Wood works, furniture and finishing", IsActive = true, DisplayOrder = 3 },
                        new Craft { Name = "Painting", Description = "Interior and exterior painting services", IsActive = true, DisplayOrder = 4 },
                        new Craft { Name = "Tiling", Description = "Floor and wall tiling services", IsActive = true, DisplayOrder = 5 },
                        new Craft { Name = "HVAC", Description = "Air conditioning and ventilation services", IsActive = true, DisplayOrder = 6 },
                        new Craft { Name = "Flooring", Description = "Hardwood, laminate, and vinyl flooring", IsActive = true, DisplayOrder = 7 },
                        new Craft { Name = "Masonry", Description = "Brickwork, stone and concrete works", IsActive = true, DisplayOrder = 8 },
                        new Craft { Name = "Roofing", Description = "Roof repair and installation", IsActive = true, DisplayOrder = 9 },
                        new Craft { Name = "Welding", Description = "Metal fabrication and welding", IsActive = true, DisplayOrder = 10 },
                        new Craft { Name = "Glazing", Description = "Glass installation and repairs", IsActive = true, DisplayOrder = 11 },
                        new Craft { Name = "Appliance Repair", Description = "Repair of household appliances", IsActive = true, DisplayOrder = 12 },
                        new Craft { Name = "Locksmith", Description = "Lock and key services", IsActive = true, DisplayOrder = 13 },
                        new Craft { Name = "Pest Control", Description = "Domestic pest control services", IsActive = true, DisplayOrder = 14 },
                        new Craft { Name = "Plastering", Description = "Wall plastering and finishing", IsActive = true, DisplayOrder = 15 }
                    };

                    await context.Crafts.AddRangeAsync(crafts);
                    await context.SaveChangesAsync();
                }

                // Areas
                if (!await context.Areas.AnyAsync())
                {
                    var areas = new List<Area>
                    {
                        new Area{ Region="Cairo", City="Nasr City"},
                        new Area{ Region="Cairo", City="Maadi"},
                        new Area{ Region="Cairo", City="Heliopolis"},
                        new Area{ Region="Cairo", City="Zamalek"},
                        new Area{ Region="Giza", City="Dokki"},
                        new Area{ Region="Giza", City="Mohandessin"},
                        new Area{ Region="Giza", City="6th of October"},
                        new Area{ Region="Alexandria", City="Smouha"},
                        new Area{ Region="Alexandria", City="Sidi Gaber"},
                        new Area{ Region="Alexandria", City="Montaza"},
                        new Area{ Region="Port Said", City="Port Said"},
                        new Area{ Region="Suez", City="Suez"},
                        new Area{ Region="Ismailia", City="Ismailia"},
                        new Area{ Region="Luxor", City="Luxor"},
                        new Area{ Region="Aswan", City="Aswan"},
                        new Area{ Region="Red Sea", City="Hurghada"},
                        new Area{ Region="South Sinai", City="Sharm El Sheikh"},
                        new Area{ Region="Dakahlia", City="Mansoura"},
                        new Area{ Region="Gharbia", City="Tanta"},
                        new Area{ Region="Sharqia", City="Zagazig"},
                        new Area{ Region="Beni Suef", City="Beni Suef"},
                        new Area{ Region="Minya", City="Minya"},
                        new Area{ Region="Faiyum", City="Fayoum"},
                        new Area{ Region="Kafr El Sheikh", City="Kafr El Sheikh"},
                        new Area{ Region="Beheira", City="Damanhour"}
                    };
                    await context.Areas.AddRangeAsync(areas);
                    await context.SaveChangesAsync();
                }

                // Users: admin, 30 craftsmen, 10 customers
                var defaultPassword = "Adel123456";

                // Admin
                if (!await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
                {
                    var admin = new ApplicationUser
                    {
                        UserName = "admin@example.com",
                        Email = "admin@example.com",
                        FullName = "System Administrator",
                        EmailConfirmed = true,
                        IsActive = true,
                        UserType = UserType.Admin,
                        CreatedAt = DateTime.UtcNow,
                        IsProfileCompleted = true
                    };
                    var res = await userManager.CreateAsync(admin, defaultPassword);
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                        await context.Admins.AddAsync(new Admin { Id = admin.Id, Department = "IT", HiredAt = DateTime.UtcNow });
                        await context.SaveChangesAsync();
                    }
                }

                // Create craftsmen users and entities
                //if (!await context.Craftsmen.AnyAsync())
                //{
                //    // prepare craft ids
                //    var craftList = await context.Crafts.ToListAsync();
                //    var areaList = await context.Areas.ToListAsync();

                //    var craftNames = new[] {
                //        "Ahmed Hassan","Mohamed Ali","Youssef Ibrahim","Khaled Mahmoud","Omar Adel",
                //        "Ibrahim Mostafa","Sami Nabil","Hassan Tarek","Tamer Sherif","Nabil Fawzy",
                //        "Samir Ragab","Adel Youssef","Rami Said","Walid Hassan","Magdy Amin",
                //        "Hany Farouk","Mina Shokry","Yassin Magdy","Fady George","Rashad Mostafa",
                //        "Saber Lotfy","Nader Samy","Karim Fathy","Bassem Helmy","Ihab Sabry",
                //        "Essam Reda","Zaki Fouad","Hisham Nour","Yousef Ashraf","Moustafa Ibrahim"
                //    };

                //    int idx = 0;
                //    var craftsmenToAdd = new List<Craftsman>();
                //    foreach (var name in craftNames)
                //    {
                //        idx++;
                //        var email = $"craftsman{idx}@example.com";
                //        var appUser = new ApplicationUser
                //        {
                //            UserName = email,
                //            Email = email,
                //            FullName = name,
                //            EmailConfirmed = true,
                //            IsActive = true,
                //            UserType = UserType.Craftsman,
                //            CreatedAt = DateTime.UtcNow,
                //            IsProfileCompleted = true
                //        };
                //        var create = await userManager.CreateAsync(appUser, defaultPassword);
                //        if (!create.Succeeded) continue;
                //        await userManager.AddToRoleAsync(appUser, "Craftsman");

                //        // Decide craft assignment: first 10 use core crafts, others use newer crafts
                //        int craftId;
                //        if (idx <= 10) craftId = craftList[(idx - 1) % 3].Id; // Plumbing/Electrician/Carpentry mix
                //        else craftId = craftList[(idx - 1) % craftList.Count].Id;

                //        var craftsman = new Craftsman
                //        {
                //            Id = appUser.Id,
                //            CraftId = craftId,
                //            Bio = "Experienced professional",
                //            YearsOfExperience = 3 + (idx % 10),
                //            HourlyRate = 90 + (idx % 10) * 5,
                //            TotalCompletedBookings = idx % 5,
                //            IsAvailable = true,
                //            Balance = 0,
                //            IsVerified = idx <= 20, // first 20 verified
                //            VerificationStatus = idx <= 20 ? VerificationStatus.Verified : VerificationStatus.Pending
                //        };
                //        craftsmenToAdd.Add(craftsman);
                //    }

                //    await context.Craftsmen.AddRangeAsync(craftsmenToAdd);
                //    await context.SaveChangesAsync();

                //    // add service areas and portfolio items
                //    var allCraftsmen = await context.Craftsmen.ToListAsync();
                //    var pitems = new List<PortfolioItem>();
                //    var csareas = new List<CraftsmanServiceArea>();
                //    int order = 1;
                //    foreach (var c in allCraftsmen)
                //    {
                //        // add 2 portfolio items
                //        pitems.Add(new PortfolioItem { CraftsmanId = c.Id, Title = "Sample Work A", Description = "Description A", ImageUrl = "https://placehold.co/600x400", DisplayOrder = order++, IsActive = c.IsVerified });
                //        pitems.Add(new PortfolioItem { CraftsmanId = c.Id, Title = "Sample Work B", Description = "Description B", ImageUrl = "https://placehold.co/600x400", DisplayOrder = order++, IsActive = c.IsVerified });

                //        // add one service area
                //        var area = areaList[c.Id % areaList.Count];
                //        csareas.Add(new CraftsmanServiceArea { CraftsmanId = c.Id, AreaId = area.Id, ServiceRadiusKm = 15, IsActive = true, CreatedAt = DateTime.UtcNow });
                //    }

                //    await context.PortfolioItems.AddRangeAsync(pitems);
                //    await context.CraftsmanServiceAreas.AddRangeAsync(csareas);
                //    await context.SaveChangesAsync();
                //}

                //// Create customers
                //if (!await context.Customers.AnyAsync())
                //{
                //    var customerNames = new[] { "Aly Samir","Sara Nabil","Omar Khaled","Laila Hassan","Hossam Adel","Mona Youssef","Khaled Fathy","Noha Samy","Mahmoud Tarek","Dina Sherif" };
                //    int i = 0;
                //    foreach (var name in customerNames)
                //    {
                //        i++;
                //        var email = $"customer{i}@example.com";
                //        var appUser = new ApplicationUser
                //        {
                //            UserName = email,
                //            Email = email,
                //            FullName = name,
                //            EmailConfirmed = true,
                //            IsActive = true,
                //            UserType = UserType.Customer,
                //            CreatedAt = DateTime.UtcNow,
                //            IsProfileCompleted = true
                //        };
                //        var create = await userManager.CreateAsync(appUser, defaultPassword);
                //        if (!create.Succeeded) continue;
                //        await userManager.AddToRoleAsync(appUser, "Customer");

                //        var area = await context.Areas.FirstAsync();
                //        var customer = new Customer { Id = appUser.Id, City = area.City, Area = area.City, Address = "Address sample", PhoneNumber = "+2011000000" + i.ToString("D2"), DateOfBirth = new DateTime(1990, 1, 1) };
                //        await context.Customers.AddAsync(customer);
                //        await context.SaveChangesAsync();
                //    }
                //}

                //// Service requests (create 15 with mixed crafts and statuses)
                //if (!await context.ServiceRequests.AnyAsync())
                //{
                //    var customersList = await context.Customers.ToListAsync();
                //    var crafts = await context.Crafts.ToListAsync();
                //    var areas = await context.Areas.ToListAsync();
                //    var rnd = new Random(42);
                //    var srList = new List<ServiceRequest>();
                //    var statuses = new[] { ServiceRequestStatus.Open, ServiceRequestStatus.OfferAccepted, ServiceRequestStatus.Completed, ServiceRequestStatus.Cancelled, ServiceRequestStatus.Expired };

                //    for (int s = 1; s <= 15; s++)
                //    {
                //        var cust = customersList[(s - 1) % customersList.Count];
                //        var craft = crafts[(s - 1) % crafts.Count];
                //        var area = areas[(s - 1) % areas.Count];
                //        var status = statuses[(s - 1) % statuses.Length];

                //        srList.Add(new ServiceRequest
                //        {
                //            CustomerId = cust.Id,
                //            CraftId = craft.Id,
                //            Title = $"Sample Request {s} - {craft.Name}",
                //            Description = $"Detailed description for request {s}",
                //            Address = "Some address",
                //            AreaId = area.Id,
                //            AvailableFromDate = DateTime.UtcNow.AddDays(rnd.Next(1, 10) * (s % 3 == 0 ? -1 : 1)),
                //            AvailableToDate = DateTime.UtcNow.AddDays(rnd.Next(5, 15)),
                //            CustomerBudget = 100m * (s + 2),
                //            MaxOffers = 10,
                //            PaymentMethod = s % 2 == 0 ? "Card" : "Cash",
                //            Status = status,
                //            OffersCount = 0,
                //            ExpiresAt = DateTime.UtcNow.AddDays(30),
                //            CreatedAt = DateTime.UtcNow
                //        });
                //    }

                //    await context.ServiceRequests.AddRangeAsync(srList);
                //    await context.SaveChangesAsync();
                //}

                //// Craftsman offers: only from verified craftsmen
                //if (!await context.CraftsmanOffers.AnyAsync())
                //{
                //    var srs = await context.ServiceRequests.ToListAsync();
                //    var verified = await context.Craftsmen.Where(c => c.IsVerified).ToListAsync();
                //    var offers = new List<CraftsmanOffer>();
                //    int idCounter = 1;
                //    foreach (var sr in srs.Take(15))
                //    {
                //        var candidates = verified.Where(c => c.CraftId == sr.CraftId).Take(3).ToList();
                //        foreach (var c in candidates)
                //        {
                //            offers.Add(new CraftsmanOffer
                //            {
                //                ServiceRequestId = sr.ServiceRequestId,
                //                CraftsmanId = c.Id,
                //                OfferedPrice = sr.CustomerBudget ?? 500m,
                //                Description = "I can do this job",
                //                EstimatedDurationMinutes = 120,
                //                PreferredDate = DateTime.UtcNow.AddDays(3),
                //                PreferredTimeSlot = "Morning",
                //                Status = OfferStatus.Pending,
                //                CreatedAt = DateTime.UtcNow
                //            });
                //            idCounter++;
                //        }
                //    }

                //    if (offers.Count > 0)
                //    {
                //        await context.CraftsmanOffers.AddRangeAsync(offers);
                //        await context.SaveChangesAsync();
                //    }
                //}

                //// Create bookings + payments for some accepted offers
                //if (!await context.Bookings.AnyAsync())
                //{
                //    var acceptedOffers = await context.CraftsmanOffers.Take(6).ToListAsync();
                //    var bookings = new List<Booking>();
                //    var payments = new List<Payment>();
                //    int bId = 1;
                //    foreach (var offer in acceptedOffers)
                //    {
                //        // only if craftsman verified
                //        var craftsman = await context.Craftsmen.FindAsync(offer.CraftsmanId);
                //        if (craftsman == null || !craftsman.IsVerified) continue;

                //        var sr = await context.ServiceRequests.FindAsync(offer.ServiceRequestId);
                //        var booking = new Booking
                //        {
                //            CustomerId = sr.CustomerId,
                //            CraftsmanId = offer.CraftsmanId,
                //            CraftId = sr.CraftId,
                //            ServiceRequestId = sr.ServiceRequestId,
                //            AcceptedOfferId = offer.CraftsmanOfferId,
                //            BookingDate = DateTime.UtcNow.AddDays(2),
                //            Duration = 120,
                //            TotalAmount = offer.OfferedPrice,
                //            Status = BookingStatus.Confirmed,
                //            CreatedAt = DateTime.UtcNow
                //        };
                //        bookings.Add(booking);
                //        await context.Bookings.AddAsync(booking);
                //        await context.SaveChangesAsync();

                //        payments.Add(new Payment
                //        {
                //            BookingId = booking.BookingId,
                //            Amount = booking.TotalAmount,
                //            PaymentDate = DateTime.UtcNow,
                //            Status = PaymentStatus.Completed,
                //            TransactionId = $"TXN{booking.BookingId:D6}",
                //            PaymentMethod = "card",
                //            PaymentGateway = "paymob"
                //        });
                //        bId++;
                //    }

                //    if (payments.Count > 0)
                //    {
                //        await context.Payments.AddRangeAsync(payments);
                //        await context.SaveChangesAsync();
                //    }
                //}

                //// Reviews for some completed bookings
                //if (!await context.Reviews.AnyAsync())
                //{
                //    var completedBookings = await context.Bookings.Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed).Take(5).ToListAsync();
                //    var reviews = new List<Review>();
                //    foreach (var b in completedBookings)
                //    {
                //        reviews.Add(new Review { BookingId = b.BookingId, ReviewerUserId = b.CustomerId, TargetUserId = b.CraftsmanId, Rating = 5, Comment = "Great job!", CreatedAt = DateTime.UtcNow });
                //    }
                //    if (reviews.Count > 0)
                //    {
                //        await context.Reviews.AddRangeAsync(reviews);
                //        await context.SaveChangesAsync();
                //    }
                //}

                await tx.CommitAsync();
                Console.WriteLine("? SQL-style seed completed by C# seeder");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"? Error during seeding: {ex.Message}");
                throw;
            }
        }
    }
}
