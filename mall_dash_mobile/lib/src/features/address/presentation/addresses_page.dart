import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/loading_indicator.dart';
import '../data/address_model.dart';
import 'address_form_page.dart';
import 'address_notifier.dart';

/// Full-screen list of the user's saved addresses with CRUD actions.
class AddressesPage extends ConsumerWidget {
  const AddressesPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(addressesProvider);
    final isDark = context.isDarkMode;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      appBar: AppBar(
        title: Text('My addresses',
            style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _openForm(context),
        icon: const Icon(Icons.add_location_alt_rounded),
        label: const Text('Add address'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: async.when(
        loading: () => const LoadingIndicator(),
        error: (e, _) => ErrorState(
          message: e.toString().replaceAll('Exception: ', ''),
          onRetry: () => ref.read(addressesProvider.notifier).refresh(),
        ),
        data: (addresses) {
          if (addresses.isEmpty) return _buildEmpty(context, isDark);
          return RefreshIndicator(
            onRefresh: () => ref.read(addressesProvider.notifier).refresh(),
            child: ListView.builder(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 96),
              itemCount: addresses.length,
              itemBuilder: (context, index) {
                final address = addresses[index];
                return _AddressCard(
                  address: address,
                  isDark: isDark,
                  onTap: () => _openForm(context, existing: address),
                  onDelete: () => _confirmDelete(context, ref, address),
                  onSetDefault: address.isDefault
                      ? null
                      : () => _setDefault(context, ref, address),
                );
              },
            ),
          );
        },
      ),
    );
  }

  Widget _buildEmpty(BuildContext context, bool isDark) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 96,
              height: 96,
              decoration: BoxDecoration(
                color: AppColors.primary.withAlpha(25),
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.location_off_outlined,
                size: 44,
                color: AppColors.primary.withAlpha(180),
              ),
            ),
            const SizedBox(height: 20),
            Text('No saved addresses',
                style: GoogleFonts.inter(
                    fontSize: 18, fontWeight: FontWeight.w700)),
            const SizedBox(height: 6),
            Text(
              'Add a delivery address to place an order.',
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(
                fontSize: 13,
                color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.add_location_alt_rounded),
              label: const Text('Add your first address'),
              onPressed: () => _openForm(context),
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                shape: AppRadius.pillButtonShape,
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _openForm(BuildContext context, {Address? existing}) {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => AddressFormPage(address: existing)),
    );
  }

  Future<void> _confirmDelete(
      BuildContext context, WidgetRef ref, Address address) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete address?'),
        content: Text('Delete "${address.label ?? address.summary}"?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('Cancel')),
          TextButton(
              onPressed: () => Navigator.pop(ctx, true),
              style: TextButton.styleFrom(foregroundColor: AppColors.error),
              child: const Text('Delete')),
        ],
      ),
    );
    if (ok != true || !context.mounted) return;
    try {
      await ref.read(addressesProvider.notifier).delete(address.id);
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(e.toString().replaceAll('Exception: ', '')),
          backgroundColor: AppColors.error,
        ));
      }
    }
  }

  Future<void> _setDefault(
      BuildContext context, WidgetRef ref, Address address) async {
    try {
      await ref.read(addressesProvider.notifier).setDefault(address.id);
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(e.toString().replaceAll('Exception: ', '')),
          backgroundColor: AppColors.error,
        ));
      }
    }
  }
}

class _AddressCard extends StatelessWidget {
  final Address address;
  final bool isDark;
  final VoidCallback onTap;
  final VoidCallback onDelete;
  final VoidCallback? onSetDefault;

  const _AddressCard({
    required this.address,
    required this.isDark,
    required this.onTap,
    required this.onDelete,
    required this.onSetDefault,
  });

  IconData get _typeIcon {
    switch (address.type) {
      case AddressType.apartment:
        return Icons.apartment_rounded;
      case AddressType.house:
        return Icons.home_rounded;
      case AddressType.office:
        return Icons.business_rounded;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusLg,
        border: Border.all(
          color: address.isDefault
              ? AppColors.primary.withAlpha(120)
              : (isDark ? Colors.white : Colors.black).withAlpha(10),
          width: address.isDefault ? 1.6 : 1,
        ),
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: AppRadius.radiusLg,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(
            children: [
              Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: AppColors.primary.withAlpha(25),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Icon(_typeIcon, color: AppColors.primary),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Flexible(
                          child: Text(
                            address.label ?? address.type.displayName,
                            style: GoogleFonts.inter(
                                fontSize: 15, fontWeight: FontWeight.w700),
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        if (address.isDefault) ...[
                          const SizedBox(width: 8),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: AppColors.primary.withAlpha(25),
                              borderRadius: AppRadius.radiusPill,
                            ),
                            child: Text(
                              'DEFAULT',
                              style: GoogleFonts.inter(
                                fontSize: 10,
                                fontWeight: FontWeight.w800,
                                color: AppColors.primary,
                                letterSpacing: 0.8,
                              ),
                            ),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      address.summary,
                      style: GoogleFonts.inter(
                        fontSize: 13,
                        color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    if (address.phoneNumber.isNotEmpty) ...[
                      const SizedBox(height: 2),
                      Text(
                        address.phoneNumber,
                        style: GoogleFonts.inter(
                          fontSize: 12,
                          color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                        ),
                      ),
                    ],
                  ],
                ),
              ),
              PopupMenuButton<String>(
                icon: const Icon(Icons.more_vert_rounded),
                itemBuilder: (_) => [
                  const PopupMenuItem(
                    value: 'edit',
                    child: ListTile(
                      leading: Icon(Icons.edit_outlined),
                      title: Text('Edit'),
                      dense: true,
                    ),
                  ),
                  if (onSetDefault != null)
                    const PopupMenuItem(
                      value: 'default',
                      child: ListTile(
                        leading: Icon(Icons.star_outline_rounded),
                        title: Text('Set as default'),
                        dense: true,
                      ),
                    ),
                  const PopupMenuItem(
                    value: 'delete',
                    child: ListTile(
                      leading: Icon(Icons.delete_outline, color: AppColors.error),
                      title: Text('Delete', style: TextStyle(color: AppColors.error)),
                      dense: true,
                    ),
                  ),
                ],
                onSelected: (v) {
                  switch (v) {
                    case 'edit':
                      onTap();
                      break;
                    case 'default':
                      onSetDefault?.call();
                      break;
                    case 'delete':
                      onDelete();
                      break;
                  }
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
}
