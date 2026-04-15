import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../product/data/product_model.dart';

class CartItem {
  final Product product;
  final int quantity;

  CartItem({
    required this.product,
    required this.quantity,
  });

  CartItem copyWith({int? quantity}) {
    return CartItem(
      product: product,
      quantity: quantity ?? this.quantity,
    );
  }

  double get totalPrice => product.effectivePrice * quantity;
}

class Cart {
  final Map<int, CartItem> items;
  final int? vendorId;

  Cart({
    Map<int, CartItem>? items,
    this.vendorId,
  }) : items = items ?? {};

  Cart copyWith({
    Map<int, CartItem>? items,
    int? vendorId,
  }) {
    return Cart(
      items: items ?? this.items,
      vendorId: vendorId ?? this.vendorId,
    );
  }

  double get subtotal => items.values.fold(0, (sum, item) => sum + item.totalPrice);
  
  int get totalItems => items.values.fold(0, (sum, item) => sum + item.quantity);

  bool get isEmpty => items.isEmpty;

  bool get isNotEmpty => items.isNotEmpty;
}

class CartNotifier extends Notifier<Cart> {
  @override
  Cart build() {
    return Cart();
  }

  void addItem(Product product) {
    // Check if adding from different vendor
    if (state.vendorId != null && state.vendorId != product.vendorId && state.isNotEmpty) {
      throw Exception('Cannot add items from different vendors. Please clear cart first.');
    }

    final currentItem = state.items[product.id];
    final newItems = Map<int, CartItem>.from(state.items);

    if (currentItem == null) {
      newItems[product.id] = CartItem(product: product, quantity: 1);
    } else {
      newItems[product.id] = currentItem.copyWith(quantity: currentItem.quantity + 1);
    }

    state = state.copyWith(
      items: newItems,
      vendorId: product.vendorId,
    );
  }

  void removeItem(int productId) {
    final newItems = Map<int, CartItem>.from(state.items);
    newItems.remove(productId);

    state = state.copyWith(
      items: newItems,
      vendorId: newItems.isEmpty ? null : state.vendorId,
    );
  }

  void updateQuantity(int productId, int quantity) {
    if (quantity <= 0) {
      removeItem(productId);
      return;
    }

    final currentItem = state.items[productId];
    if (currentItem == null) return;

    final newItems = Map<int, CartItem>.from(state.items);
    newItems[productId] = currentItem.copyWith(quantity: quantity);

    state = state.copyWith(items: newItems);
  }

  void incrementQuantity(int productId) {
    final currentItem = state.items[productId];
    if (currentItem == null) return;

    updateQuantity(productId, currentItem.quantity + 1);
  }

  void decrementQuantity(int productId) {
    final currentItem = state.items[productId];
    if (currentItem == null) return;

    updateQuantity(productId, currentItem.quantity - 1);
  }

  void clear() {
    state = Cart();
  }

  int getQuantity(int productId) {
    return state.items[productId]?.quantity ?? 0;
  }
}

final cartProvider = NotifierProvider<CartNotifier, Cart>(() {
  return CartNotifier();
});
