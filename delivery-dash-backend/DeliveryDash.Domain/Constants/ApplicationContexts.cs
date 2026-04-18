namespace DeliveryDash.Domain.Constants
{
    public static class ApplicationContexts
    {
        public const string SuperAdminDashboard = "superadmin_dashboard";
        public const string AdminDashboard = "admin_dashboard";
        public const string VendorPortal = "vendor_portal";
        public const string TenantPortal = "tenant_portal";
        public const string DriverPortal = "driver_portal";

        public static readonly Dictionary<string, string[]> ContextRoleMap = new()
        {
            { SuperAdminDashboard, [IdentityRoleConstant.SuperAdmin] },
            { AdminDashboard, [IdentityRoleConstant.SuperAdmin, IdentityRoleConstant.Admin] },
            { VendorPortal, [IdentityRoleConstant.Vendor, IdentityRoleConstant.VendorStaff] },
            { TenantPortal, [IdentityRoleConstant.Tenant] },
            { DriverPortal, [IdentityRoleConstant.Driver] }
        };

        public static bool IsValidContext(string context, IList<string> userRoles)
        {
            if (!ContextRoleMap.TryGetValue(context, out var requiredRoles))
                return false;

            return userRoles.Any(role => requiredRoles.Contains(role));
        }
    }
}