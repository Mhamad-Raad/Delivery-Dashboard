import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/error_state.dart';
import 'address_form_page.dart';
import 'address_notifier.dart';

/// Bottom-sheet used from the cart to pick a delivery address.
///
/// Call [show] to open it; it resolves to the picked address id (or null if the
/// user dismissed). The provider state is also updated in place.
class AddressSelectorSheet extends ConsumerWidget {
  const AddressSelectorSheet({super.key});

  static Future<int?> show(BuildContext context) {
    return showModalBottomSheet<int>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => const AddressSelectorSheet(),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(addressesProvider);
    final selectedId = ref.watch(selectedAddressIdProvider);
    final isDark = context.isDarkMode;

    return DraggableScrollableSheet(
      initialChildSize: 0.65,
      minChildSize: 0.4,
      maxChildSize: 0.95,
      expand: false,
      builder: (context, scrollController) {
        return Container(
          decoration: BoxDecoration(
            color: isDark ? AppColors.backgroundDark : AppColors.background,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: Column(
            children: [
              Container(
                margin: const EdgeInsets.only(top: 12),
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: (isDark ? Colors.white : Colors.black).withAlpha(40),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Padding(
                padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
                child: Row(
                  children: [
                    Expanded(
                      child: Text(
                        'Deliver to',
                        style: GoogleFonts.inter(
                            fontSize: 18, fontWeight: FontWeight.w700),
                      ),
                    ),
                    TextButton.icon(
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(builder: (_) => const AddressFormPage()),
                        );
                      },
                      icon: const Icon(Icons.add_rounded, size: 18),
                      label: const Text('Add new'),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: async.when(
                  loading: () => const Center(child: CircularProgressIndicator()),
                  error: (e, _) => ErrorState(
                    message: e.toString().replaceAll('Exception: ', ''),
                    onRetry: () => ref.read(addressesProvider.notifier).refresh(),
                  ),
                  data: (addresses) {
                    if (addresses.isEmpty) {
                      return Center(
                        child: Padding(
                          padding: const EdgeInsets.all(24),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(
                                Icons.location_off_outlined,
                                size: 48,
                                color: (isDark ? Colors.white : Colors.black).withAlpha(90),
                              ),
                              const SizedBox(height: 12),
                              Text('No saved addresses yet',
                                  style: GoogleFonts.inter(
                                      fontSize: 15, fontWeight: FontWeight.w600)),
                              const SizedBox(height: 4),
                              Text(
                                'Tap "Add new" above to create one.',
                                style: GoogleFonts.inter(
                                  fontSize: 13,
                                  color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                                ),
                              ),
                            ],
                          ),
                        ),
                      );
                    }
                    return ListView.builder(
                      controller: scrollController,
                      padding: const EdgeInsets.fromLTRB(16, 4, 16, 24),
                      itemCount: addresses.length,
                      itemBuilder: (context, index) {
                        final a = addresses[index];
                        final isSelected = a.id == selectedId;
                        return Container(
                          margin: const EdgeInsets.only(bottom: 10),
                          decoration: BoxDecoration(
                            color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                            borderRadius: AppRadius.radiusLg,
                            border: Border.all(
                              color: isSelected
                                  ? AppColors.primary
                                  : (isDark ? Colors.white : Colors.black).withAlpha(10),
                              width: isSelected ? 1.8 : 1,
                            ),
                          ),
                          child: InkWell(
                            borderRadius: AppRadius.radiusLg,
                            onTap: () {
                              ref.read(selectedAddressIdProvider.notifier).select(a.id);
                              Navigator.pop(context, a.id);
                            },
                            child: Padding(
                              padding: const EdgeInsets.all(14),
                              child: Row(
                                children: [
                                  Icon(
                                    isSelected
                                        ? Icons.radio_button_checked_rounded
                                        : Icons.radio_button_unchecked_rounded,
                                    color: isSelected
                                        ? AppColors.primary
                                        : (isDark ? Colors.white : Colors.black).withAlpha(120),
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
                                                a.label ?? a.type.displayName,
                                                style: GoogleFonts.inter(
                                                    fontSize: 14,
                                                    fontWeight: FontWeight.w700),
                                                overflow: TextOverflow.ellipsis,
                                              ),
                                            ),
                                            if (a.isDefault) ...[
                                              const SizedBox(width: 6),
                                              Text(
                                                '· default',
                                                style: GoogleFonts.inter(
                                                  fontSize: 11,
                                                  color: AppColors.primary,
                                                  fontWeight: FontWeight.w700,
                                                ),
                                              ),
                                            ],
                                          ],
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          a.summary,
                                          style: GoogleFonts.inter(
                                            fontSize: 12,
                                            color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                                          ),
                                          maxLines: 2,
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}
