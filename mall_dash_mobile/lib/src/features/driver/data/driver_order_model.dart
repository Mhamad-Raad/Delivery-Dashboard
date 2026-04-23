/// Driver order data models matching backend API structure.
///
/// Backend `OrderStatus`: Pending=1, Confirmed=2, Preparing=3, OutForDelivery=4, Delivered=5, Cancelled=6.
class DriverOrder {
  final int id;
  final String orderNumber;
  final String? customerId;
  final String? customerName;
  final String? customerPhone;
  final String? customerEmail;

  // Delivery-address fields (new flat address shape from backend).
  final String? deliveryAddress; // pre-formatted human-readable string for list views
  final double? latitude;
  final double? longitude;
  final String? street;
  final String? addressPhoneNumber;
  final String? buildingName;
  final String? floorNumber;
  final String? apartmentNumber;
  final String? additionalDirections;
  final String? addressLabel;

  final int? vendorId;
  final String? vendorName;
  final int status;
  final String statusName;
  final double? subtotal;
  final double? deliveryFee;
  final double? totalAmount;
  final String? notes;
  final DateTime? createdAt;
  final DateTime? confirmedAt;
  final DateTime? preparingAt;
  final DateTime? outForDeliveryAt;
  final DateTime? deliveredAt;
  final DateTime? cancelledAt;
  final DateTime? completedAt;
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
    this.latitude,
    this.longitude,
    this.street,
    this.addressPhoneNumber,
    this.buildingName,
    this.floorNumber,
    this.apartmentNumber,
    this.additionalDirections,
    this.addressLabel,
    this.vendorId,
    this.vendorName,
    required this.status,
    required this.statusName,
    this.subtotal,
    this.deliveryFee,
    this.totalAmount,
    this.notes,
    this.createdAt,
    this.confirmedAt,
    this.preparingAt,
    this.outForDeliveryAt,
    this.deliveredAt,
    this.cancelledAt,
    this.completedAt,
    this.items,
    this.assignmentId,
    this.isAssigned,
  });

  factory DriverOrder.fromJson(Map<String, dynamic> json) {
    int? parseIntOrNull(dynamic value) {
      if (value == null) return null;
      if (value is int) return value;
      if (value is String) return int.tryParse(value);
      return null;
    }

    int parseIntOrDefault(dynamic value, int defaultValue) {
      if (value == null) return defaultValue;
      if (value is int) return value;
      if (value is String) return int.tryParse(value) ?? defaultValue;
      return defaultValue;
    }

    double? parseDoubleOrNull(dynamic value) {
      if (value == null) return null;
      if (value is double) return value;
      if (value is int) return value.toDouble();
      if (value is String) return double.tryParse(value);
      return null;
    }

    DateTime? parseDate(dynamic value) =>
        value == null ? null : DateTime.tryParse(value.toString());

    // Parse deliveryAddress — backend now sends a flat object with lat/lng + type-specific fields.
    // Falls back to the old Building/Floor/Apartment shape or a plain string for tolerance.
    String? deliveryAddressStr;
    double? latitude;
    double? longitude;
    String? street;
    String? addressPhone;
    String? buildingName;
    String? floorNumber;
    String? apartmentNumber;
    String? additionalDirections;
    String? addressLabel;

    final deliveryAddressData = json['deliveryAddress'];
    if (deliveryAddressData is Map<String, dynamic>) {
      latitude = parseDoubleOrNull(deliveryAddressData['latitude']);
      longitude = parseDoubleOrNull(deliveryAddressData['longitude']);
      street = deliveryAddressData['street']?.toString();
      addressPhone = deliveryAddressData['phoneNumber']?.toString();
      buildingName = deliveryAddressData['buildingName']?.toString();
      floorNumber = (deliveryAddressData['floor'] ?? deliveryAddressData['floorNumber'])?.toString();
      apartmentNumber = (deliveryAddressData['apartmentNumber'] ?? deliveryAddressData['apartmentName'])?.toString();
      additionalDirections = deliveryAddressData['additionalDirections']?.toString();
      addressLabel = deliveryAddressData['label']?.toString();

      // Build a readable one-liner for list views.
      final parts = <String>[];
      if (apartmentNumber != null) parts.add('Apt $apartmentNumber');
      if (floorNumber != null) parts.add('Floor $floorNumber');
      if (buildingName != null) parts.add(buildingName);
      if (street != null) parts.add(street);
      deliveryAddressStr = parts.isNotEmpty ? parts.join(', ') : null;
    } else if (deliveryAddressData is String) {
      deliveryAddressStr = deliveryAddressData;
    } else {
      // Fallback to flat fields at the root (legacy shape).
      buildingName = json['buildingName']?.toString();
      floorNumber = (json['floor'] ?? json['floorNumber'])?.toString();
      apartmentNumber = (json['apartmentNumber'] ?? json['apartmentName'])?.toString();
      deliveryAddressStr = json['address']?.toString();
    }

    final status = parseIntOrDefault(json['status'], 0);

    return DriverOrder(
      id: parseIntOrDefault(json['id'] ?? json['orderId'], 0),
      orderNumber: json['orderNumber']?.toString() ?? '',
      customerId: (json['userId'] ?? json['customerId'])?.toString(),
      customerName: json['userName']?.toString() ?? json['customerName']?.toString(),
      customerPhone: json['userPhone']?.toString() ??
          json['customerPhone']?.toString() ??
          json['phoneNumber']?.toString(),
      customerEmail: json['userEmail']?.toString() ?? json['customerEmail']?.toString(),
      deliveryAddress: deliveryAddressStr,
      latitude: latitude,
      longitude: longitude,
      street: street,
      addressPhoneNumber: addressPhone,
      buildingName: buildingName,
      floorNumber: floorNumber,
      apartmentNumber: apartmentNumber,
      additionalDirections: additionalDirections,
      addressLabel: addressLabel,
      vendorId: parseIntOrNull(json['vendorId']),
      vendorName: json['vendorName']?.toString(),
      status: status,
      statusName: _getStatusName(status),
      subtotal: parseDoubleOrNull(json['subtotal']),
      deliveryFee: parseDoubleOrNull(json['deliveryFee']),
      totalAmount: parseDoubleOrNull(json['totalAmount'] ?? json['total']),
      notes: json['notes']?.toString(),
      createdAt: parseDate(json['createdAt']),
      confirmedAt: parseDate(json['confirmedAt']),
      preparingAt: parseDate(json['preparingAt']),
      outForDeliveryAt: parseDate(json['outForDeliveryAt']),
      deliveredAt: parseDate(json['deliveredAt']),
      cancelledAt: parseDate(json['cancelledAt']),
      completedAt: parseDate(json['completedAt']),
      items: json['items'] != null
          ? (json['items'] as List).map((item) => OrderItem.fromJson(item)).toList()
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
        return 'Out for Delivery';
      case 5:
        return 'Delivered';
      case 6:
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
      'latitude': latitude,
      'longitude': longitude,
      'street': street,
      'addressPhoneNumber': addressPhoneNumber,
      'buildingName': buildingName,
      'floorNumber': floorNumber,
      'apartmentNumber': apartmentNumber,
      'additionalDirections': additionalDirections,
      'addressLabel': addressLabel,
      'vendorId': vendorId,
      'vendorName': vendorName,
      'status': status,
      'statusName': statusName,
      'subtotal': subtotal,
      'deliveryFee': deliveryFee,
      'totalAmount': totalAmount,
      'notes': notes,
      'createdAt': createdAt?.toIso8601String(),
      'confirmedAt': confirmedAt?.toIso8601String(),
      'preparingAt': preparingAt?.toIso8601String(),
      'outForDeliveryAt': outForDeliveryAt?.toIso8601String(),
      'deliveredAt': deliveredAt?.toIso8601String(),
      'cancelledAt': cancelledAt?.toIso8601String(),
      'completedAt': completedAt?.toIso8601String(),
      'items': items?.map((item) => item.toJson()).toList(),
      'assignmentId': assignmentId,
      'isAssigned': isAssigned,
    };
  }

  DriverOrder copyWith({
    int? id,
    String? orderNumber,
    String? customerId,
    String? customerName,
    String? customerPhone,
    String? customerEmail,
    String? deliveryAddress,
    double? latitude,
    double? longitude,
    String? street,
    String? addressPhoneNumber,
    String? buildingName,
    String? floorNumber,
    String? apartmentNumber,
    String? additionalDirections,
    String? addressLabel,
    int? vendorId,
    String? vendorName,
    int? status,
    String? statusName,
    double? subtotal,
    double? deliveryFee,
    double? totalAmount,
    String? notes,
    DateTime? createdAt,
    DateTime? confirmedAt,
    DateTime? preparingAt,
    DateTime? outForDeliveryAt,
    DateTime? deliveredAt,
    DateTime? cancelledAt,
    DateTime? completedAt,
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
      latitude: latitude ?? this.latitude,
      longitude: longitude ?? this.longitude,
      street: street ?? this.street,
      addressPhoneNumber: addressPhoneNumber ?? this.addressPhoneNumber,
      buildingName: buildingName ?? this.buildingName,
      floorNumber: floorNumber ?? this.floorNumber,
      apartmentNumber: apartmentNumber ?? this.apartmentNumber,
      additionalDirections: additionalDirections ?? this.additionalDirections,
      addressLabel: addressLabel ?? this.addressLabel,
      vendorId: vendorId ?? this.vendorId,
      vendorName: vendorName ?? this.vendorName,
      status: status ?? this.status,
      statusName: statusName ?? this.statusName,
      subtotal: subtotal ?? this.subtotal,
      deliveryFee: deliveryFee ?? this.deliveryFee,
      totalAmount: totalAmount ?? this.totalAmount,
      notes: notes ?? this.notes,
      createdAt: createdAt ?? this.createdAt,
      confirmedAt: confirmedAt ?? this.confirmedAt,
      preparingAt: preparingAt ?? this.preparingAt,
      outForDeliveryAt: outForDeliveryAt ?? this.outForDeliveryAt,
      deliveredAt: deliveredAt ?? this.deliveredAt,
      cancelledAt: cancelledAt ?? this.cancelledAt,
      completedAt: completedAt ?? this.completedAt,
      items: items ?? this.items,
      assignmentId: assignmentId ?? this.assignmentId,
      isAssigned: isAssigned ?? this.isAssigned,
    );
  }
}

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

class OrderAssignmentRequest {
  final int assignmentId;

  OrderAssignmentRequest({required this.assignmentId});

  Map<String, dynamic> toJson() => {'assignmentId': assignmentId};
}

class UpdateOrderStatusRequest {
  final int status;

  UpdateOrderStatusRequest({required this.status});

  Map<String, dynamic> toJson() => {'status': status};
}

/// Order status enum matching backend OrderStatus values.
enum DeliveryStatus {
  pending(1),
  confirmed(2),
  preparing(3),
  outForDelivery(4),
  delivered(5),
  cancelled(6);

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
      case DeliveryStatus.outForDelivery:
        return 'Out for Delivery';
      case DeliveryStatus.delivered:
        return 'Delivered';
      case DeliveryStatus.cancelled:
        return 'Cancelled';
    }
  }
}
