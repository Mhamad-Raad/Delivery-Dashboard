using MalDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Data
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
            if (await _context.Users.AnyAsync())
            {
                return;
            }

            foreach (var (user, roleName, password) in UserDataSeeder.GenerateUsers())
            {
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
