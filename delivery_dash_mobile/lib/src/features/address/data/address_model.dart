/// Address types matching the backend `AddressType` enum.
enum AddressType {
  apartment(0, 'Apartment'),
  house(1, 'House'),
  office(2, 'Office');

  final int value;
  final String displayName;
  const AddressType(this.value, this.displayName);

  static AddressType fromValue(int value) {
    return AddressType.values.firstWhere(
      (t) => t.value == value,
      orElse: () => AddressType.apartment,
    );
  }
}

/// Customer delivery address matching backend `AddressResponse`.
class Address {
  final int id;
  final String userId;
  final AddressType type;
  final double latitude;
  final double longitude;
  final String phoneNumber;
  final String street;

  // Apartment / Office
  final String? buildingName;
  // Apartment / Office / House
  final String? floor;
  // Apartment only
  final String? apartmentNumber;
  // House only
  final String? houseName;
  final String? houseNumber;
  // Office only
  final String? companyName;

  final String? additionalDirections;
  final String? label;
  final bool isDefault;
  final DateTime? createdAt;
  final DateTime? lastModifiedAt;

  Address({
    required this.id,
    required this.userId,
    required this.type,
    required this.latitude,
    required this.longitude,
    required this.phoneNumber,
    required this.street,
    this.buildingName,
    this.floor,
    this.apartmentNumber,
    this.houseName,
    this.houseNumber,
    this.companyName,
    this.additionalDirections,
    this.label,
    this.isDefault = false,
    this.createdAt,
    this.lastModifiedAt,
  });

  factory Address.fromJson(Map<String, dynamic> json) {
    double parseDouble(dynamic v) {
      if (v == null) return 0;
      if (v is double) return v;
      if (v is int) return v.toDouble();
      if (v is String) return double.tryParse(v) ?? 0;
      return 0;
    }

    int? parseIntOrNull(dynamic v) {
      if (v == null) return null;
      if (v is int) return v;
      if (v is String) return int.tryParse(v);
      return null;
    }

    DateTime? parseDate(dynamic v) =>
        v == null ? null : DateTime.tryParse(v.toString());

    return Address(
      id: parseIntOrNull(json['id'] ?? json['Id']) ?? 0,
      userId: (json['userId'] ?? json['UserId'] ?? '').toString(),
      type: AddressType.fromValue(
        parseIntOrNull(json['type'] ?? json['Type']) ?? 0,
      ),
      latitude: parseDouble(json['latitude'] ?? json['Latitude']),
      longitude: parseDouble(json['longitude'] ?? json['Longitude']),
      phoneNumber: (json['phoneNumber'] ?? json['PhoneNumber'] ?? '').toString(),
      street: (json['street'] ?? json['Street'] ?? '').toString(),
      buildingName: (json['buildingName'] ?? json['BuildingName'])?.toString(),
      floor: (json['floor'] ?? json['Floor'])?.toString(),
      apartmentNumber: (json['apartmentNumber'] ?? json['ApartmentNumber'])?.toString(),
      houseName: (json['houseName'] ?? json['HouseName'])?.toString(),
      houseNumber: (json['houseNumber'] ?? json['HouseNumber'])?.toString(),
      companyName: (json['companyName'] ?? json['CompanyName'])?.toString(),
      additionalDirections: (json['additionalDirections'] ?? json['AdditionalDirections'])?.toString(),
      label: (json['label'] ?? json['Label'])?.toString(),
      isDefault: (json['isDefault'] ?? json['IsDefault']) == true,
      createdAt: parseDate(json['createdAt'] ?? json['CreatedAt']),
      lastModifiedAt: parseDate(json['lastModifiedAt'] ?? json['LastModifiedAt']),
    );
  }

  /// Short one-line summary for list + cart display.
  String get summary {
    final parts = <String>[];
    switch (type) {
      case AddressType.apartment:
        if (apartmentNumber != null) parts.add('Apt $apartmentNumber');
        if (floor != null) parts.add('Floor $floor');
        if (buildingName != null) parts.add(buildingName!);
        break;
      case AddressType.house:
        if (houseName != null) parts.add(houseName!);
        if (houseNumber != null) parts.add('No. $houseNumber');
        break;
      case AddressType.office:
        if (companyName != null) parts.add(companyName!);
        if (floor != null) parts.add('Floor $floor');
        if (buildingName != null) parts.add(buildingName!);
        break;
    }
    if (street.isNotEmpty) parts.add(street);
    return parts.isEmpty ? type.displayName : parts.join(', ');
  }
}

/// Payload for `POST /Address` and `PUT /Address/{id}` — identical shape.
class AddressRequest {
  final AddressType type;
  final double latitude;
  final double longitude;
  final String phoneNumber;
  final String street;
  final String? buildingName;
  final String? floor;
  final String? apartmentNumber;
  final String? houseName;
  final String? houseNumber;
  final String? companyName;
  final String? additionalDirections;
  final String? label;
  final bool isDefault;

  AddressRequest({
    required this.type,
    required this.latitude,
    required this.longitude,
    required this.phoneNumber,
    required this.street,
    this.buildingName,
    this.floor,
    this.apartmentNumber,
    this.houseName,
    this.houseNumber,
    this.companyName,
    this.additionalDirections,
    this.label,
    this.isDefault = false,
  });

  Map<String, dynamic> toJson() => {
        'type': type.value,
        'latitude': latitude,
        'longitude': longitude,
        'phoneNumber': phoneNumber,
        'street': street,
        if (buildingName != null) 'buildingName': buildingName,
        if (floor != null) 'floor': floor,
        if (apartmentNumber != null) 'apartmentNumber': apartmentNumber,
        if (houseName != null) 'houseName': houseName,
        if (houseNumber != null) 'houseNumber': houseNumber,
        if (companyName != null) 'companyName': companyName,
        if (additionalDirections != null) 'additionalDirections': additionalDirections,
        if (label != null) 'label': label,
        'isDefault': isDefault,
      };
}
