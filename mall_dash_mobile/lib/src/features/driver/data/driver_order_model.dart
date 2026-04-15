/// Driver order data models matching backend API structure
class DriverOrder {
  final int id;
  final String orderNumber;
  final String? customerId; // Changed to String to support UUID
  final String? customerName;
  final String? customerPhone;
  final String? customerEmail;
  final String? deliveryAddress;
  final String? buildingName;
  final String? floorNumber;
  final String? apartmentNumber;
  final int? vendorId;
  final String? vendorName;
  final int status;
  final String statusName;
  final double? subtotal;
  final double? deliveryFee;
  final double? totalAmount;
  final String? notes;
  final DateTime? createdAt;
  final DateTime? completedAt;
  final DateTime? deliveryTime;
  final List<OrderItem>? items;
  final int? assignmentId;
  final bool? isAssigned;

  DriverOrder({
    required this.id,
    required this.orderNumber,
    this.customerId,
    this.customerName,
    this.customerPhone,
    this.customerEmail,
    this.deliveryAddress,
    this.buildingName,
    this.floorNumber,
    this.apartmentNumber,
    this.vendorId,
    this.vendorName,
    required this.status,
    required this.statusName,
    this.subtotal,
    this.deliveryFee,
    this.totalAmount,
    this.notes,
    this.createdAt,
    this.completedAt,
    this.deliveryTime,
    this.items,
    this.assignmentId,
    this.isAssigned,
  });

  factory DriverOrder.fromJson(Map<String, dynamic> json) {
    print('========================================');
    print('📋 PARSING ORDER JSON');
    print('   Raw JSON keys: ${json.keys.toList()}');
    print('========================================');

    // Helper function to safely parse int from dynamic value
    int? parseIntOrNull(dynamic value) {
      if (value == null) return null;
      if (value is int) return value;
      if (value is String) return int.tryParse(value);
      return null;
    }

    // Helper function to safely parse int with default
    int parseIntOrDefault(dynamic value, int defaultValue) {
      if (value == null) return defaultValue;
      if (value is int) return value;
      if (value is String) return int.tryParse(value) ?? defaultValue;
      return defaultValue;
    }

    // Helper function to safely parse double
    double? parseDoubleOrNull(dynamic value) {
      if (value == null) return null;
      if (value is double) return value;
      if (value is int) return value.toDouble();
      if (value is String) return double.tryParse(value);
      return null;
    }

    // Parse deliveryAddress - can be a nested object or a string
    String? deliveryAddressStr;
    String? buildingName;
    String? floorNumber;
    String? apartmentNumber;

    final deliveryAddressData = json['deliveryAddress'];
    if (deliveryAddressData is Map<String, dynamic>) {
      // Nested object format: {"buildingName": "...", "floorNumber": 7, "apartmentName": "..."}
      buildingName = deliveryAddressData['buildingName']?.toString();
      floorNumber = deliveryAddressData['floorNumber']?.toString();
      apartmentNumber = deliveryAddressData['apartmentName']?.toString() ??
                        deliveryAddressData['apartmentNumber']?.toString();

      // Build a readable address string
      final parts = <String>[];
      if (apartmentNumber != null) parts.add(apartmentNumber!);
      if (floorNumber != null) parts.add('Floor $floorNumber');
      if (buildingName != null) parts.add(buildingName!);
      deliveryAddressStr = parts.isNotEmpty ? parts.join(', ') : null;

      print('   📍 Parsed nested deliveryAddress:');
      print('      Building: $buildingName');
      print('      Floor: $floorNumber');
      print('      Apartment: $apartmentNumber');
    } else if (deliveryAddressData is String) {
      deliveryAddressStr = deliveryAddressData;
    } else {
      // Fallback to flat fields
      buildingName = json['buildingName']?.toString();
      floorNumber = json['floorNumber']?.toString();
      apartmentNumber = json['apartmentNumber']?.toString() ?? json['apartmentName']?.toString();
      deliveryAddressStr = json['address']?.toString();
    }

    final status = parseIntOrDefault(json['status'], 0);

    return DriverOrder(
      id: parseIntOrDefault(json['id'] ?? json['orderId'], 0),
      orderNumber: json['orderNumber']?.toString() ?? '',
      customerId: (json['userId'] ?? json['customerId'])?.toString(),
      customerName: json['userName']?.toString() ?? json['customerName']?.toString(),
      customerPhone: json['userPhone']?.toString() ?? json['customerPhone']?.toString() ?? json['phoneNumber']?.toString(),
      customerEmail: json['userEmail']?.toString() ?? json['customerEmail']?.toString(),
      deliveryAddress: deliveryAddressStr,
      buildingName: buildingName,
      floorNumber: floorNumber,
      apartmentNumber: apartmentNumber,
      vendorId: parseIntOrNull(json['vendorId']),
      vendorName: json['vendorName']?.toString(),
      status: status,
      statusName: _getStatusName(status),
      subtotal: parseDoubleOrNull(json['subtotal']),
      deliveryFee: parseDoubleOrNull(json['deliveryFee']),
      totalAmount: parseDoubleOrNull(json['totalAmount'] ?? json['total']),
      notes: json['notes']?.toString(),
      createdAt: json['createdAt'] != null
          ? DateTime.tryParse(json['createdAt'].toString())
          : null,
      completedAt: json['completedAt'] != null
          ? DateTime.tryParse(json['completedAt'].toString())
          : null,
      deliveryTime: json['deliveryTime'] != null
          ? DateTime.tryParse(json['deliveryTime'].toString())
          : null,
      items: json['items'] != null
          ? (json['items'] as List)
                .map((item) => OrderItem.fromJson(item))
                .toList()
          : null,
      assignmentId: parseIntOrNull(json['assignmentId']),
      isAssigned: json['isAssigned'] ?? json['assigned'],
    );
  }

  static String _getStatusName(int status) {
    switch (status) {
      case 1:
        return 'Pending';
      case 2:
        return 'Confirmed';
      case 3:
        return 'Preparing';
      case 4:
        return 'Ready for Pickup';
      case 5:
        return 'Assigned to Driver';
      case 6:
        return 'Picked Up';
      case 7:
        return 'In Transit';
      case 8:
        return 'Delivered';
      case 9:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderNumber': orderNumber,
      'userId': customerId,
      'userName': customerName,
      'userPhone': customerPhone,
      'userEmail': customerEmail,
      'deliveryAddress': deliveryAddress,
      'buildingName': buildingName,
      'floorNumber': floorNumber,
      'apartmentNumber': apartmentNumber,
      'vendorId': vendorId,
      'vendorName': vendorName,
      'status': status,
      'statusName': statusName,
      'subtotal': subtotal,
      'deliveryFee': deliveryFee,
      'totalAmount': totalAmount,
      'notes': notes,
      'createdAt': createdAt?.toIso8601String(),
      'completedAt': completedAt?.toIso8601String(),
      'deliveryTime': deliveryTime?.toIso8601String(),
      'items': items?.map((item) => item.toJson()).toList(),
      'assignmentId': assignmentId,
      'isAssigned': isAssigned,
    };
  }

  /// Create a copy of this order with updated fields
  DriverOrder copyWith({
    int? id,
    String? orderNumber,
    String? customerId,
    String? customerName,
    String? customerPhone,
    String? customerEmail,
    String? deliveryAddress,
    String? buildingName,
    String? floorNumber,
    String? apartmentNumber,
    int? vendorId,
    String? vendorName,
    int? status,
    String? statusName,
    double? subtotal,
    double? deliveryFee,
    double? totalAmount,
    String? notes,
    DateTime? createdAt,
    DateTime? completedAt,
    DateTime? deliveryTime,
    List<OrderItem>? items,
    int? assignmentId,
    bool? isAssigned,
  }) {
    return DriverOrder(
      id: id ?? this.id,
      orderNumber: orderNumber ?? this.orderNumber,
      customerId: customerId ?? this.customerId,
      customerName: customerName ?? this.customerName,
      customerPhone: customerPhone ?? this.customerPhone,
      customerEmail: customerEmail ?? this.customerEmail,
      deliveryAddress: deliveryAddress ?? this.deliveryAddress,
      buildingName: buildingName ?? this.buildingName,
      floorNumber: floorNumber ?? this.floorNumber,
      apartmentNumber: apartmentNumber ?? this.apartmentNumber,
      vendorId: vendorId ?? this.vendorId,
      vendorName: vendorName ?? this.vendorName,
      status: status ?? this.status,
      statusName: statusName ?? this.statusName,
      subtotal: subtotal ?? this.subtotal,
      deliveryFee: deliveryFee ?? this.deliveryFee,
      totalAmount: totalAmount ?? this.totalAmount,
      notes: notes ?? this.notes,
      createdAt: createdAt ?? this.createdAt,
      completedAt: completedAt ?? this.completedAt,
      deliveryTime: deliveryTime ?? this.deliveryTime,
      items: items ?? this.items,
      assignmentId: assignmentId ?? this.assignmentId,
      isAssigned: isAssigned ?? this.isAssigned,
    );
  }
}

/// Order item model
class OrderItem {
  final int id;
  final String productName;
  final int quantity;
  final double price;
  final String? imageUrl;

  OrderItem({
    required this.id,
    required this.productName,
    required this.quantity,
    required this.price,
    this.imageUrl,
  });

  factory OrderItem.fromJson(Map<String, dynamic> json) {
    return OrderItem(
      id: json['id'] ?? json['productId'] ?? 0,
      productName: json['productName'] ?? json['name'] ?? '',
      quantity: json['quantity'] ?? 1,
      price: (json['price'] ?? json['unitPrice'] ?? 0).toDouble(),
      imageUrl: json['imageUrl'] ?? json['image'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'productName': productName,
      'quantity': quantity,
      'price': price,
      'imageUrl': imageUrl,
    };
  }
}

/// Order assignment request model
class OrderAssignmentRequest {
  final int assignmentId;

  OrderAssignmentRequest({required this.assignmentId});

  Map<String, dynamic> toJson() {
    return {'assignmentId': assignmentId};
  }
}

/// Update order status request model
class UpdateOrderStatusRequest {
  final int status;

  UpdateOrderStatusRequest({required this.status});

  Map<String, dynamic> toJson() {
    return {'status': status};
  }
}

/// Delivery status enum matching backend values
enum DeliveryStatus {
  pending(1),
  confirmed(2),
  preparing(3),
  readyForPickup(4),
  assignedToDriver(5),
  pickedUp(6),
  inTransit(7),
  delivered(8),
  cancelled(9);

  final int value;
  const DeliveryStatus(this.value);

  static DeliveryStatus fromValue(int value) {
    return DeliveryStatus.values.firstWhere(
      (status) => status.value == value,
      orElse: () => DeliveryStatus.pending,
    );
  }

  String get displayName {
    switch (this) {
      case DeliveryStatus.pending:
        return 'Pending';
      case DeliveryStatus.confirmed:
        return 'Confirmed';
      case DeliveryStatus.preparing:
        return 'Preparing';
      case DeliveryStatus.readyForPickup:
        return 'Ready for Pickup';
      case DeliveryStatus.assignedToDriver:
        return 'Assigned to Driver';
      case DeliveryStatus.pickedUp:
        return 'Picked Up';
      case DeliveryStatus.inTransit:
        return 'In Transit';
      case DeliveryStatus.delivered:
        return 'Delivered';
      case DeliveryStatus.cancelled:
        return 'Cancelled';
    }
  }
}
