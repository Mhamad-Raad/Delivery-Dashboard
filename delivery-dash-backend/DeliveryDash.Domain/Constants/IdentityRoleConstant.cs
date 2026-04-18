using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryDash.Domain.Constants
{
    public static class IdentityRoleConstant
    {
        public static readonly Guid SuperAdminRoleGuid = Guid.Parse("768c88f6-9438-4714-acfe-8f9239836c98");
        public static readonly Guid AdminRoleGuid = Guid.Parse("cd56803c-f8ba-4f59-92ff-faee35d567e4");
        public static readonly Guid VendorRoleGuid = Guid.Parse("2738d25f-ebd2-4473-816a-5771b5f7c42f");
        public static readonly Guid TenantsRoleGuid = Guid.Parse("d90d48fd-655a-423c-ae20-23fe71107116");
        public static readonly Guid DriverRoleGuid = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        public static readonly Guid VendorStaffRoleGuid = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Vendor = "Vendor";
        public const string Tenant = "Tenant";
        public const string VendorStaff = "VendorStaff";
        public const string Driver = "Driver";
    }
}


