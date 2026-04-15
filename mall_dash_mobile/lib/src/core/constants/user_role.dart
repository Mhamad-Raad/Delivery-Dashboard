/// User role constants matching backend API
/// These values are returned from the backend as integers in the role field
class UserRole {
  // Role values from backend
  static const String tenant = '3';  // Tenant/Customer role
  static const String driver = '4';  // Driver role
  
  // Role names for display
  static const String tenantName = 'Tenant';
  static const String driverName = 'Driver';
  
  /// Check if role is tenant
  static bool isTenant(String? role) {
    return role == tenant;
  }
  
  /// Check if role is driver
  static bool isDriver(String? role) {
    return role == driver;
  }
  
  /// Get display name for role
  static String getDisplayName(String? role) {
    switch (role) {
      case tenant:
        return tenantName;
      case driver:
        return driverName;
      default:
        return 'Unknown';
    }
  }
}
