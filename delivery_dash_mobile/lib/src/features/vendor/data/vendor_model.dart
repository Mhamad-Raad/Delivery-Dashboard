class Vendor {
  final int id;
  final String name;
  final String? description;
  final String? profileImageUrl;
  final String? openingTime;
  final String? closeTime;
  final String? type;

  Vendor({
    required this.id,
    required this.name,
    this.description,
    this.profileImageUrl,
    this.openingTime,
    this.closeTime,
    this.type,
  });

  factory Vendor.fromJson(Map<String, dynamic> json) {
    return Vendor(
      id: json['id'] as int,
      name: json['name'] as String,
      description: json['description'] as String?,
      profileImageUrl: json['profileImageUrl'] as String?,
      openingTime: json['openingTime'] as String?,
      closeTime: json['closeTime'] as String?,
      type: json['type'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'profileImageUrl': profileImageUrl,
      'openingTime': openingTime,
      'closeTime': closeTime,
      'type': type,
    };
  }
}
