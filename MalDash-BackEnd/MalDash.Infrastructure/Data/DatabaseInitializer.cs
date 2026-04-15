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
            // Check if database is already seeded
            var hasUsers = await _context.Users.AnyAsync();
            var hasBuildings = await _context.Buildings.AnyAsync();

            if (hasUsers && hasBuildings)
            {
                return; // Database already seeded, skip
            }

            // Seed Buildings, Floors, and Apartments first (if not exist)
            if (!hasBuildings)
            {
                var (buildings, floors, apartments) = BuildingDataSeeder.GenerateBuildings();
                
                await _context.Buildings.AddRangeAsync(buildings);
                await _context.Floors.AddRangeAsync(floors);
                await _context.Apartments.AddRangeAsync(apartments);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Seeded {buildings.Count} buildings, {floors.Count} floors, and {apartments.Count} apartments.");
            }

            // Seed Users (if not exist)
            var createdUsers = new List<User>();
            if (!hasUsers)
            {
                var users = UserDataSeeder.GenerateUsers();

                foreach (var (user, roleName) in users)
                {
                    var result = await _userManager.CreateAsync(user, UserDataSeeder.DefaultPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                        createdUsers.Add(user);
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Seeded {createdUsers.Count} users.");
            }
            else
            {
                // If users already exist, get them from database
                createdUsers = await _context.Users.ToListAsync();
            }

            // Assign Users to Apartments (check if addresses already exist)
            var hasAddresses = await _context.Addresses.AnyAsync();
            if (!hasAddresses && createdUsers.Any())
            {
                var buildings = await _context.Buildings.ToListAsync();
                var floors = await _context.Floors.ToListAsync();
                var apartments = await _context.Apartments.ToListAsync();

                var addresses = BuildingDataSeeder.AssignUsersToApartments(
                    createdUsers, 
                    buildings, 
                    floors, 
                    apartments);

                await _context.Addresses.AddRangeAsync(addresses);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Assigned {addresses.Count} users to apartments.");
            }

            Console.WriteLine("Database seeding completed successfully.");
        }
    }
}