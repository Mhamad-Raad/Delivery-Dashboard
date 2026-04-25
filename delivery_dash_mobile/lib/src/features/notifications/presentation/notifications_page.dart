import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/empty_state.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/loading_indicator.dart';
import '../data/customer_notification_repository.dart';

final _notificationsProvider =
    FutureProvider.autoDispose<List<CustomerNotification>>((ref) async {
  final repo = ref.watch(customerNotificationRepositoryProvider);
  return repo.list();
});

class NotificationsPage extends ConsumerWidget {
  const NotificationsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(_notificationsProvider);
    final isDark = context.isDarkMode;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      appBar: AppBar(
        title: Text(
          'Notifications',
          style: GoogleFonts.inter(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
          ),
        ),
        actions: [
          async.maybeWhen(
            data: (items) => items.any((n) => !n.isRead)
                ? TextButton(
                    onPressed: () async {
                      await ref
                          .read(customerNotificationRepositoryProvider)
                          .markAllAsRead();
                      ref.invalidate(_notificationsProvider);
                    },
                    child: Text(
                      'Mark all read',
                      style: GoogleFonts.inter(
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                        color: AppColors.primary,
                      ),
                    ),
                  )
                : const SizedBox.shrink(),
            orElse: () => const SizedBox.shrink(),
          ),
        ],
      ),
      body: RefreshIndicator(
        color: AppColors.accent,
        onRefresh: () async {
          ref.invalidate(_notificationsProvider);
          await ref.read(_notificationsProvider.future);
        },
        child: async.when(
          data: (items) {
            if (items.isEmpty) {
              return const SingleChildScrollView(
                physics: AlwaysScrollableScrollPhysics(),
                child: SizedBox(
                  height: 600,
                  child: EmptyState(
                    icon: Icons.notifications_none_rounded,
                    title: 'No notifications',
                    subtitle: 'You\'re all caught up.',
                  ),
                ),
              );
            }
            return ListView.separated(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.symmetric(
                  horizontal: 16, vertical: 12),
              itemCount: items.length,
              separatorBuilder: (_, __) => const SizedBox(height: 10),
              itemBuilder: (context, index) {
                final n = items[index];
                return _NotificationCard(
                  n: n,
                  isDark: isDark,
                  onTap: () async {
                    if (!n.isRead) {
                      await ref
                          .read(customerNotificationRepositoryProvider)
                          .markAsRead(n.id);
                      ref.invalidate(_notificationsProvider);
                    }
                  },
                )
                    .animate(delay: Duration(milliseconds: index * 35))
                    .fadeIn(duration: 300.ms)
                    .slideY(begin: 0.05, end: 0);
              },
            );
          },
          loading: () => const LoadingIndicator(),
          error: (error, _) => ErrorState(
            message: error.toString(),
            onRetry: () => ref.invalidate(_notificationsProvider),
          ),
        ),
      ),
    );
  }
}

class _NotificationCard extends StatelessWidget {
  final CustomerNotification n;
  final bool isDark;
  final VoidCallback onTap;

  const _NotificationCard({
    required this.n,
    required this.isDark,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final appTheme = context.appTheme;

    return Material(
      color: isDark ? AppColors.cardBackgroundDark : AppColors.cardBackground,
      borderRadius: AppRadius.radiusXl,
      child: InkWell(
        onTap: onTap,
        borderRadius: AppRadius.radiusXl,
        child: Ink(
          decoration: BoxDecoration(
            borderRadius: AppRadius.radiusXl,
            border: Border.all(
              color: isDark
                  ? Colors.white.withAlpha(8)
                  : Colors.black.withAlpha(5),
            ),
          ),
          child: Padding(
            padding: const EdgeInsets.all(12),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (n.imageUrl != null && n.imageUrl!.isNotEmpty)
                  ClipRRect(
                    borderRadius: AppRadius.radiusMd,
                    child: CachedNetworkImage(
                      imageUrl: n.imageUrl!,
                      width: 64,
                      height: 64,
                      fit: BoxFit.cover,
                      placeholder: (_, __) => Container(
                        width: 64,
                        height: 64,
                        color: appTheme.surfaceVariant,
                      ),
                      errorWidget: (_, __, ___) => Container(
                        width: 64,
                        height: 64,
                        color: appTheme.surfaceVariant,
                        child: Icon(Icons.image_not_supported_outlined,
                            color: appTheme.textTertiary),
                      ),
                    ),
                  )
                else
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.primary.withAlpha(20),
                      borderRadius: AppRadius.radiusMd,
                    ),
                    child: Icon(Icons.notifications_rounded,
                        color: AppColors.primary, size: 22),
                  ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              n.title,
                              style: GoogleFonts.inter(
                                fontSize: 14,
                                fontWeight: FontWeight.w700,
                                color: isDark
                                    ? AppColors.textPrimaryDark
                                    : AppColors.textPrimary,
                              ),
                            ),
                          ),
                          if (!n.isRead) ...[
                            const SizedBox(width: 8),
                            Container(
                              width: 8,
                              height: 8,
                              decoration: const BoxDecoration(
                                color: AppColors.accent,
                                shape: BoxShape.circle,
                              ),
                            ),
                          ],
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(
                        n.message,
                        style: GoogleFonts.inter(
                          fontSize: 13,
                          color: appTheme.textSecondary,
                          height: 1.4,
                        ),
                        maxLines: 3,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 6),
                      Text(
                        _timeAgo(n.createdAt),
                        style: GoogleFonts.inter(
                          fontSize: 11,
                          color: appTheme.textTertiary,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _timeAgo(DateTime d) {
    final diff = DateTime.now().difference(d);
    if (diff.inSeconds < 60) return 'Just now';
    if (diff.inMinutes < 60) return '${diff.inMinutes}m ago';
    if (diff.inHours < 24) return '${diff.inHours}h ago';
    if (diff.inDays < 7) return '${diff.inDays}d ago';
    return '${d.day}/${d.month}/${d.year}';
  }
}
