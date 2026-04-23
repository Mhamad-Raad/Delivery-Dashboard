class Order {
  final int id;
  final String orderNumber;
  final String userId;
  final String? userName;
  final int vendorId;
  final String? vendorName;
  final int? addressId;
  final double subtotal;
  final double deliveryFee;
  final double totalAmount;
  final String? notes;
  final int status;
  final String statusName;
  final DateTime createdAt;
  final DateTime? confirmedAt;
  final DateTime? preparingAt;
  final DateTime? outForDeliveryAt;
  final DateTime? deliveredAt;
  final DateTime? cancelledAt;
  final DateTime? completedAt;
  final int itemCount;
  final List<OrderItem> items;

  Order({
    required this.id,
    required this.orderNumber,
    required this.userId,
    this.userName,
    required this.vendorId,
    this.vendorName,
    this.addressId,
    required this.subtotal,
    required this.deliveryFee,
    required this.totalAmount,
    this.notes,
    required this.status,
    required this.statusName,
    required this.createdAt,
    this.confirmedAt,
    this.preparingAt,
    this.outForDeliveryAt,
    this.deliveredAt,
    this.cancelledAt,
    this.completedAt,
    required this.itemCount,
    this.items = const [],
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    DateTime? parseDate(dynamic v) =>
        v == null ? null : DateTime.tryParse(v.toString());

    return Order(
      id: json['id'] as int? ?? 0,
      orderNumber: json['orderNumber'] as String? ?? '',
      userId: json['userId'] as String? ?? '',
      userName: json['userName'] as String?,
      vendorId: json['vendorId'] as int? ?? 0,
      vendorName: json['vendorName'] as String?,
      addressId: json['addressId'] as int?,
      subtotal: (json['subtotal'] as num?)?.toDouble() ?? 0.0,
      deliveryFee: (json['deliveryFee'] as num?)?.toDouble() ?? 0.0,
      totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0.0,
      notes: json['notes'] as String?,
      status: json['status'] as int? ?? 0,
      statusName: json['statusName'] as String? ?? _getStatusName(json['status'] as int? ?? 0),
      createdAt: parseDate(json['createdAt']) ?? DateTime.now(),
      confirmedAt: parseDate(json['confirmedAt']),
      preparingAt: parseDate(json['preparingAt']),
      outForDeliveryAt: parseDate(json['outForDeliveryAt']),
      deliveredAt: parseDate(json['deliveredAt']),
      cancelledAt: parseDate(json['cancelledAt']),
      completedAt: parseDate(json['completedAt']),
      itemCount: json['itemCount'] as int? ?? 0,
      items: (json['items'] as List<dynamic>?)
              ?.map((item) => OrderItem.fromJson(item as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderNumber': orderNumber,
      'userId': userId,
      'userName': userName,
      'vendorId': vendorId,
      'vendorName': vendorName,
      'addressId': addressId,
      'subtotal': subtotal,
      'deliveryFee': deliveryFee,
      'totalAmount': totalAmount,
      'notes': notes,
      'status': status,
      'statusName': statusName,
      'createdAt': createdAt.toIso8601String(),
      'confirmedAt': confirmedAt?.toIso8601String(),
      'preparingAt': preparingAt?.toIso8601String(),
      'outForDeliveryAt': outForDeliveryAt?.toIso8601String(),
      'deliveredAt': deliveredAt?.toIso8601String(),
      'cancelledAt': cancelledAt?.toIso8601String(),
      'completedAt': completedAt?.toIso8601String(),
      'itemCount': itemCount,
      'items': items.map((item) => item.toJson()).toList(),
    };
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
        return 'OutForDelivery';
      case 5:
        return 'Delivered';
      case 6:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }
}

class OrderItem {
  final int id;
  final int productId;
  final String productName;
  final String? productImageUrl;
  final int quantity;
  final double price;
  final double totalPrice;

  OrderItem({
    required this.id,
    required this.productId,
    required this.productName,
    this.productImageUrl,
    required this.quantity,
    required this.price,
    required this.totalPrice,
  });

  factory OrderItem.fromJson(Map<String, dynamic> json) {
    // Backend uses 'unitPrice' field, not 'price'
    final price = json['unitPrice'] ?? json['price'];

    return OrderItem(
      id: json['id'] as int? ?? 0,
      productId: json['productId'] as int? ?? 0,
      productName: json['productName'] as String? ?? '',
      productImageUrl: json['productImageUrl'] as String?,
      quantity: json['quantity'] as int? ?? 1,
      price: (price as num?)?.toDouble() ?? 0.0,
      totalPrice: (json['totalPrice'] as num?)?.toDouble() ?? 0.0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'productId': productId,
      'productName': productName,
      'productImageUrl': productImageUrl,
      'quantity': quantity,
      'price': price,
      'totalPrice': totalPrice,
    };
  }
}

class CreateOrderRequest {
  final int vendorId;
  final int? addressId;
  final double deliveryFee;
  final String? notes;
  final List<CreateOrderItem> items;

  CreateOrderRequest({
    required this.vendorId,
    this.addressId,
    required this.deliveryFee,
    this.notes,
    required this.items,
  });

  Map<String, dynamic> toJson() {
    return {
      'vendorId': vendorId,
      'addressId': addressId,
      'deliveryFee': deliveryFee,
      'notes': notes,
      'items': items.map((item) => item.toJson()).toList(),
    };
  }
}

class CreateOrderItem {
  final int productId;
  final int quantity;

  CreateOrderItem({
    required this.productId,
    required this.quantity,
  });

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'quantity': quantity,
    };
  }
}
