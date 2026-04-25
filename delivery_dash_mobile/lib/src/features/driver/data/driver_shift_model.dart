/// Driver shift data models matching backend API structure
class DriverShift {
  final String? id;
  final String? driverId;
  final DateTime? startTime;
  final DateTime? endTime;
  final bool isActive;
  final int? queuePosition;

  DriverShift({
    this.id,
    this.driverId,
    this.startTime,
    this.endTime,
    required this.isActive,
    this.queuePosition,
  });

  factory DriverShift.fromJson(Map<String, dynamic> json) {
    return DriverShift(
      id: json['id']?.toString() ?? json['_id']?.toString(),
      driverId: json['driverId']?.toString() ?? json['userId']?.toString(),
      startTime: json['startTime'] != null
          ? DateTime.tryParse(json['startTime'].toString())
          : null,
      endTime: json['endTime'] != null
          ? DateTime.tryParse(json['endTime'].toString())
          : null,
      isActive: json['isActive'] ?? json['active'] ?? false,
      queuePosition: json['queuePosition'] ?? json['position'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'driverId': driverId,
      'startTime': startTime?.toIso8601String(),
      'endTime': endTime?.toIso8601String(),
      'isActive': isActive,
      'queuePosition': queuePosition,
    };
  }
}

/// Queue position response model
class QueuePosition {
  final int position;
  final int totalDrivers;

  QueuePosition({required this.position, required this.totalDrivers});

  factory QueuePosition.fromJson(Map<String, dynamic> json) {
    return QueuePosition(
      position: json['position'] ?? json['queuePosition'] ?? 0,
      totalDrivers: json['totalDrivers'] ?? json['total'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {'position': position, 'totalDrivers': totalDrivers};
  }
}

/// Shift status enum
enum ShiftStatus {
  inactive,
  active,
  paused;

  static ShiftStatus fromString(String? status) {
    if (status == null) return ShiftStatus.inactive;
    switch (status.toLowerCase()) {
      case 'active':
        return ShiftStatus.active;
      case 'paused':
        return ShiftStatus.paused;
      default:
        return ShiftStatus.inactive;
    }
  }

  String get displayName {
    switch (this) {
      case ShiftStatus.active:
        return 'Active';
      case ShiftStatus.paused:
        return 'Paused';
      case ShiftStatus.inactive:
        return 'Inactive';
    }
  }
}
