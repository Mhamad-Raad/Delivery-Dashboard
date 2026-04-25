/// User role constants matching backend API.
///
/// Backend `Role` enum values:
///   SuperAdmin = 0, Admin = 1, Vendor = 2, Customer = 3, Driver = 4, VendorStaff = 5.
/// The mobile app only serves Customer and Driver; other roles are web-only.
class UserRole {
  static const String customer = '3';
  static const String driver = '4';

  static const String customerName = 'Customer';
  static const String driverName = 'Driver';

  static bool isCustomer(String? role) => role == customer;

  static bool isDriver(String? role) => role == driver;

  static String getDisplayName(String? role) {
    switch (role) {
      case customer:
        return customerName;
      case driver:
        return driverName;
      default:
        return 'Unknown';
    }
  }
}
