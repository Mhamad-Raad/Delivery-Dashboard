using MalDash.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MalDash.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
        {
            builder.HasData(new List<IdentityRole<Guid>>
            {
                new()
                {
                    Id = IdentityRoleConstant.SuperAdminRoleGuid,
                    Name = IdentityRoleConstant.SuperAdmin,
                    NormalizedName = IdentityRoleConstant.SuperAdmin.ToUpper(),
                    ConcurrencyStamp = "c1672889-7933-4585-8684-9a899fc55687"
                },
                new()
                {
                    Id = IdentityRoleConstant.AdminRoleGuid,
                    Name = IdentityRoleConstant.Admin,
                    NormalizedName = IdentityRoleConstant.Admin.ToUpper(),
                    ConcurrencyStamp = "368749f9-79cd-46c6-bbc1-72fe6d3365d5"
                },
                new()
                {
                    Id = IdentityRoleConstant.VendorRoleGuid,
                    Name = IdentityRoleConstant.Vendor,
                    NormalizedName = IdentityRoleConstant.Vendor.ToUpper(),
                    ConcurrencyStamp = "8554d40c-ee93-4b5e-b3a1-e072b71877c7"
                },
                new()
                {
                    Id = IdentityRoleConstant.TenantsRoleGuid,
                    Name = IdentityRoleConstant.Tenant,
                    NormalizedName = IdentityRoleConstant.Tenant.ToUpper(),
                    ConcurrencyStamp = "6dc9dea8-7375-4a4d-b35b-8b18670eb407"
                },
                new()
                {
                    Id = IdentityRoleConstant.VendorStaffRoleGuid,
                    Name = IdentityRoleConstant.VendorStaff,
                    NormalizedName = IdentityRoleConstant.VendorStaff.ToUpper(),
                    ConcurrencyStamp = "f1a2b3c4-d5e6-7890-abcd-ef1234567890"
                },
                new()
                {
                    Id = IdentityRoleConstant.DriverRoleGuid,
                    Name = IdentityRoleConstant.Driver,
                    NormalizedName = IdentityRoleConstant.Driver.ToUpper(),
                    ConcurrencyStamp = "a2b3c4d5-e6f7-8901-bcde-f23456789012"
                }
            });

            builder.HasIndex(r => r.NormalizedName)
                .IsUnique()
                .HasDatabaseName("IX_Roles_NormalizedName");
        }
    }

    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
        {
            builder.HasIndex(ur => ur.UserId)
                .HasDatabaseName("IX_UserRoles_UserId");

            builder.HasIndex(ur => ur.RoleId)
                .HasDatabaseName("IX_UserRoles_RoleId");
        }
    }
}