class UserProfile {
  final String id;
  final String firstName;
  final String lastName;
  final String email;
  final String phoneNumber;
  final String? profileImageUrl;
  final String? role;
  final String? buildingName;
  final String? apartmentNumber;

  UserProfile({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.phoneNumber,
    this.profileImageUrl,
    this.role,
    this.buildingName,
    this.apartmentNumber,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    // Backend uses different field names and types
    // GET: {user: {_id, firstName, ...}} PUT: {_id, firstName, ...}
    
    // Handle role as int or String (backend returns int)
    String? roleStr;
    final roleValue = json['role'] ?? json['Role'];
    if (roleValue != null) {
      roleStr = roleValue.toString();
    }
    
    return UserProfile(
      id: (json['_id'] ?? json['id'] ?? json['Id']) as String? ?? '',
      firstName: (json['firstName'] ?? json['FirstName']) as String? ?? '',
      lastName: (json['lastName'] ?? json['LastName']) as String? ?? '',
      email: (json['email'] ?? json['Email']) as String? ?? '',
      phoneNumber: (json['phoneNumber'] ?? json['PhoneNumber']) as String? ?? '',
      profileImageUrl: (json['profileImageUrl'] ?? json['ProfileImageUrl']) as String?,
      role: roleStr,
      buildingName: (json['buildingName'] ?? json['BuildingName']) as String?,
      apartmentNumber: (json['apartmentNumber'] ?? json['ApartmentNumber'] ?? json['apartmentName']) as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'firstName': firstName,
      'lastName': lastName,
      'email': email,
      'phoneNumber': phoneNumber,
      'profileImageUrl': profileImageUrl,
      'role': role,
      'buildingName': buildingName,
      'apartmentNumber': apartmentNumber,
    };
  }

  String get fullName => '$firstName $lastName';
}

class UpdateProfileRequest {
  final String firstName;
  final String lastName;
  final String email;
  final String phoneNumber;

  UpdateProfileRequest({
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.phoneNumber,
  });

  Map<String, dynamic> toJson() {
    return {
      'firstName': firstName,
      'lastName': lastName,
      'email': email,
      'phoneNumber': phoneNumber,
    };
  }
}
