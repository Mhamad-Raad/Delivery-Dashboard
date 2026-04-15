using MalDash.Domain.Constants;
using MalDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Data
{
    public static class UserDataSeeder
    {
        // Pre-defined user IDs for consistency
        public static readonly Guid SuperAdminUserId = new("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");
        public static readonly Guid AdminUserId = new("b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e");
        public static readonly Guid VendorUserId = new("c3d4e5f6-a7b8-4c5d-9e0f-2a3b4c5d6e7f");
        public static readonly Guid TenantUserId = new("d4e5f6a7-b8c9-4d5e-0f1a-3b4c5d6e7f8a");

        // Configuration for bulk user generation
        private const int NumberOfAdmins = 50;
        private const int NumberOfVendors = 100;
        private const int NumberOfTenants = 350;

        // Single password for all users
        public const string DefaultPassword = "Test123-";

        // Sample data for realistic user generation
        private static readonly string[] FirstNames = 
        [
            "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
            "William", "Barbara", "David", "Elizabeth", "Richard", "Susan", "Joseph", "Jessica",
            "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa",
            "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley",
            "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua", "Michelle",
            "Kenneth", "Dorothy", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa",
            "Edward", "Deborah", "Ronald", "Stephanie", "Timothy", "Rebecca", "Jason", "Sharon",
            "Jeffrey", "Laura", "Ryan", "Cynthia", "Jacob", "Kathleen", "Gary", "Amy",
            "Nicholas", "Shirley", "Eric", "Angela", "Jonathan", "Helen", "Stephen", "Anna",
            "Larry", "Brenda", "Justin", "Pamela", "Scott", "Nicole", "Brandon", "Emma",
            "Benjamin", "Samantha", "Samuel", "Katherine", "Raymond", "Christine", "Gregory", "Debra",
            "Alexander", "Rachel", "Patrick", "Catherine", "Frank", "Carolyn", "Jack", "Janet",
            "Dennis", "Ruth", "Jerry", "Maria", "Tyler", "Heather", "Aaron", "Diane"
        ];

        private static readonly string[] LastNames = 
        [
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
            "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
            "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
            "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
            "Green", "AdAMS", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell",
            "Carter", "Roberts", "Gomez", "Phillips", "EvANS", "Turner", "Diaz", "Parker",
            "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy",
            "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey",
            "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson",
            "Watson", "Brooks", "Chavez", "Wood", "James", "Bennett", "Gray", "Mendoza",
            "Ruiz", "Hughes", "Price", "Alvarez", "Castillo", "Sanders", "Patel", "Myers",
            "Long", "Ross", "Foster", "Jimenez", "Powell", "Jenkins", "Perry", "Russell"
        ];

        // For migration seeding (OnModelCreating)
        public static void SeedUsers(ModelBuilder builder)
        {
            var passwordHasher = new PasswordHasher<User>();
            var users = new List<User>();
            var userRoles = new List<IdentityUserRole<Guid>>();

            var usersWithRoles = GenerateUsersForMigration(passwordHasher);
            
            foreach (var (user, roleId) in usersWithRoles)
            {
                users.Add(user);
                userRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleId });
            }

            builder.Entity<User>().HasData(users);
            builder.Entity<IdentityUserRole<Guid>>().HasData(userRoles);
        }

        // For runtime seeding (DatabaseInitializer)
        public static List<(User User, string RoleName)> GenerateUsers()
        {
            var result = new List<(User, string)>();
            var random = new Random(42);

            // Main users - keep original emails
            result.Add((CreateUserWithoutPassword(SuperAdminUserId, "Super", "Admin", "superadmin@maldash.com", "+1234567890"), IdentityRoleConstant.SuperAdmin));
            result.Add((CreateUserWithoutPassword(AdminUserId, "John", "Doe", "admin@maldash.com", "+1234567891"), IdentityRoleConstant.Admin));
            result.Add((CreateUserWithoutPassword(VendorUserId, "Jane", "Smith", "vendor@maldash.com", "+1234567892"), IdentityRoleConstant.Vendor));
            result.Add((CreateUserWithoutPassword(TenantUserId, "Michael", "Johnson", "tenant@maldash.com", "+1234567893"), IdentityRoleConstant.Tenant));

            // Generate Admins
            for (int i = 1; i <= NumberOfAdmins; i++)
            {
                var userId = Guid.NewGuid();
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var email = $"admin{firstName}123@maldash.com";
                result.Add((
                    CreateUserWithoutPassword(userId, firstName, lastName, email, $"+1{random.Next(200, 999)}{random.Next(1000000, 9999999)}"),
                    IdentityRoleConstant.Admin
                ));
            }

            // Generate Vendors
            for (int i = 1; i <= NumberOfVendors; i++)
            {
                var userId = Guid.NewGuid();
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var email = $"vendor{firstName}123@maldash.com";
                result.Add((
                    CreateUserWithoutPassword(userId, firstName, lastName, email, $"+1{random.Next(200, 999)}{random.Next(1000000, 9999999)}"),
                    IdentityRoleConstant.Vendor
                ));
            }

            // Generate Tenants
            for (int i = 1; i <= NumberOfTenants; i++)
            {
                var userId = Guid.NewGuid();
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var email = $"tenant{firstName}123@maldash.com";
                result.Add((
                    CreateUserWithoutPassword(userId, firstName, lastName, email, $"+1{random.Next(200, 999)}{random.Next(1000000, 9999999)}"),
                    IdentityRoleConstant.Tenant
                ));
            }

            return result;
        }

        private static List<(User User, Guid RoleId)> GenerateUsersForMigration(PasswordHasher<User> passwordHasher)
        {
            var result = new List<(User, Guid)>();
            var random = new Random(42);

            // Main users - keep original emails
            result.Add((CreateUser(SuperAdminUserId, "Super", "Admin", "superadmin@maldash.com", "+1234567890", passwordHasher, DefaultPassword), IdentityRoleConstant.SuperAdminRoleGuid));
            result.Add((CreateUser(AdminUserId, "John", "Doe", "admin@maldash.com", "+1234567891", passwordHasher, DefaultPassword), IdentityRoleConstant.AdminRoleGuid));
            result.Add((CreateUser(VendorUserId, "Jane", "Smith", "vendor@maldash.com", "+1234567892", passwordHasher, DefaultPassword), IdentityRoleConstant.VendorRoleGuid));
            result.Add((CreateUser(TenantUserId, "Michael", "Johnson", "tenant@maldash.com", "+1234567893", passwordHasher, DefaultPassword), IdentityRoleConstant.TenantsRoleGuid));

            // Generate Admins
            for (int i = 1; i <= NumberOfAdmins; i++)
            {
                var userId = Guid.NewGuid();
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var email = $"admin{firstName}123@maldash.com";
                result.Add((
                    CreateUser(userId, firstName, lastName, email, $"+1{random.Next(200, 999)}{random.Next(1000000, 9999999)}", passwordHasher, DefaultPassword),
                    IdentityRoleConstant.AdminRoleGuid
                ));
            }

            // Generate Vendors
            for (int i = 1; i <= NumberOfVendors; i++)
            {
                var userId = Guid.NewGuid();
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var email = $"vendor{firstName}123@maldash.com";
                result.Add((
                    CreateUser(userId, firstName, lastName, email, $"+1{random.Next(200, 999)}{random.Next(1000000, 9999999)}", passwordHasher, DefaultPassword),
                    IdentityRoleConstant.VendorRoleGuid
                ));
            }

            // Generate Tenants
            for (int i = 1; i <= NumberOfTenants; i++)
            {
                var userId = Guid.NewGuid();
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var email = $"tenant{firstName}123@maldash.com";
                result.Add((
                    CreateUser(userId, firstName, lastName, email, $"+1{random.Next(200, 999)}{random.Next(1000000, 9999999)}", passwordHasher, DefaultPassword),
                    IdentityRoleConstant.TenantsRoleGuid
                ));
            }

            return result;
        }

        private static User CreateUser(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            PasswordHasher<User> passwordHasher,
            string password)
        {
            var user = new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                NormalizedEmail = email.ToUpper(),
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            return user;
        }

        private static User CreateUserWithoutPassword(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string phoneNumber)
        {
            return new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                NormalizedEmail = email.ToUpper(),
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };
        }
    }
}