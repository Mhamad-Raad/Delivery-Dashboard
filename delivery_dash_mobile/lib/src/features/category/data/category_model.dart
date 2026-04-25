class Category {
  final int id;
  final int vendorId;
  final String name;
  final String? description;
  final int? sortOrder;
  final int productsCount;

  const Category({
    required this.id,
    required this.vendorId,
    required this.name,
    this.description,
    this.sortOrder,
    this.productsCount = 0,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'] as int,
      vendorId: json['vendorId'] as int,
      name: json['name'] as String,
      description: json['description'] as String?,
      sortOrder: json['sortOrder'] as int?,
      productsCount: (json['productsCount'] as int?) ?? 0,
    );
  }
}
