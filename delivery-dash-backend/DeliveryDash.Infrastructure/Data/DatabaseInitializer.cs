using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Data
{
    public class DatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DatabaseInitializer(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            await SeedVendorCategoriesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedVendorCategoriesAsync()
        {
            if (await _context.VendorCategories.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;
            _context.VendorCategories.AddRange(
                new VendorCategory { Name = "Restaurant", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new VendorCategory { Name = "Cafe",       IsActive = true, CreatedAt = now, UpdatedAt = now },
                new VendorCategory { Name = "Market",     IsActive = true, CreatedAt = now, UpdatedAt = now },
                new VendorCategory { Name = "Bakery",     IsActive = true, CreatedAt = now, UpdatedAt = now },
                new VendorCategory { Name = "Pharmacy",   IsActive = true, CreatedAt = now, UpdatedAt = now }
            );

            await _context.SaveChangesAsync();
            Console.WriteLine("Seeded 5 VendorCategories.");
        }

        private async Task SeedUsersAsync()
        {
            foreach (var (user, roleName, password) in UserDataSeeder.GenerateUsers())
            {
                var existing = await _userManager.FindByEmailAsync(user.Email!);
                if (existing != null)
                {
                    continue;
                }

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                    Console.WriteLine($"Seeded user: {user.Email} ({roleName})");
                }
                else
                {
                    Console.WriteLine($"Failed to seed {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
