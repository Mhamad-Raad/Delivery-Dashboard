class DriverLocation {
  final int orderId;
  final double lat;
  final double lng;
  final double? heading;
  final DateTime recordedAt;

  const DriverLocation({
    required this.orderId,
    required this.lat,
    required this.lng,
    this.heading,
    required this.recordedAt,
  });

  factory DriverLocation.fromJson(Map<String, dynamic> json) {
    return DriverLocation(
      orderId: (json['orderId'] as num?)?.toInt() ?? 0,
      lat: (json['lat'] as num?)?.toDouble() ?? 0,
      lng: (json['lng'] as num?)?.toDouble() ?? 0,
      heading: (json['heading'] as num?)?.toDouble(),
      recordedAt: DateTime.tryParse(json['recordedAt']?.toString() ?? '') ??
          DateTime.now(),
    );
  }
}
