class Product {
  final int id;
  final String name;
  final String? description;
  final double price;
  final double? discountPrice;
  final String? productImageUrl;
  final int? categoryId;
  final int vendorId;
  final bool inStock;
  final bool? isWeightable;

  Product({
    required this.id,
    required this.name,
    this.description,
    required this.price,
    this.discountPrice,
    this.productImageUrl,
    this.categoryId,
    required this.vendorId,
    required this.inStock,
    this.isWeightable,
  });

  // Backward compatibility getter
  String? get imageUrl => productImageUrl;
  bool get isAvailable => inStock;

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      id: json['id'] as int,
      name: json['name'] as String,
      description: json['description'] as String?,
      price: (json['price'] as num).toDouble(),
      discountPrice: json['discountPrice'] != null ? (json['discountPrice'] as num).toDouble() : null,
      productImageUrl: json['productImageUrl'] as String?,
      categoryId: json['categoryId'] as int?,
      vendorId: json['vendorId'] as int,
      inStock: json['inStock'] as bool? ?? true,
      isWeightable: json['isWeightable'] as bool?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'price': price,
      'discountPrice': discountPrice,
      'productImageUrl': productImageUrl,
      'categoryId': categoryId,
      'vendorId': vendorId,
      'inStock': inStock,
      'isWeightable': isWeightable,
    };
  }

  // Get the effective price (discount price if available, otherwise regular price)
  double get effectivePrice => discountPrice ?? price;

  // Check if product has a discount
  bool get hasDiscount => discountPrice != null && discountPrice! < price;
}
