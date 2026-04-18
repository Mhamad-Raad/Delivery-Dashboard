using MalDash.Domain.Constants;
using MalDash.Domain.Entities;

namespace MalDash.Infrastructure.Data
{
    public static class UserDataSeeder
    {
        public static readonly Guid AdminUserId = new("b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e");
        public static readonly Guid VendorUserId = new("c3d4e5f6-a7b8-4c5d-9e0f-2a3b4c5d6e7f");

        public static List<(User User, string RoleName, string Password)> GenerateUsers()
        {
            return new List<(User, string, string)>
            {
                (CreateUser(AdminUserId, "Hama", "Raad", "hamaraad883@gmail.com", "+9647700000001"),
                    IdentityRoleConstant.Admin, "hamagyanH1"),
                (CreateUser(VendorUserId, "Bako", "Vendor", "bako@gmail.com", "+9647700000002"),
                    IdentityRoleConstant.Vendor, "bakogyanB1"),
            };
        }

        private static User CreateUser(Guid id, string firstName, string lastName, string email, string phone)
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
                PhoneNumber = phone,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };
        }
    }
}
