import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/status_badge.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/loading_indicator.dart';
import 'support_tickets_notifier.dart';
import 'create_support_ticket_page.dart';
import 'support_ticket_details_page.dart';

class SupportTicketsPage extends ConsumerWidget {
  const SupportTicketsPage({super.key});

  /// Map support ticket status to order status equivalent for color consistency
  int _mapTicketStatusToColor(int status) {
    switch (status) {
      case 1: return 1; // Open -> Pending (orange)
      case 2: return 2; // In Progress -> Confirmed (blue)
      case 3: return 5; // Resolved -> Delivered (green)
      case 4: return 6; // Closed -> Cancelled (grey, using default)
      default: return 0;
    }
  }

  Color _statusAccentColor(int status, CustomThemeExtension appTheme) {
    return appTheme.getOrderStatusColor(_mapTicketStatusToColor(status));
  }

  IconData _statusIcon(int status) {
    switch (status) {
      case 1: return Icons.fiber_new_rounded;
      case 2: return Icons.autorenew_rounded;
      case 3: return Icons.check_circle_rounded;
      case 4: return Icons.archive_rounded;
      default: return Icons.help_outline_rounded;
    }
  }

  Color _priorityColor(String priority) {
    switch (priority.toLowerCase()) {
      case 'urgent': return AppColors.error;
      case 'high': return AppColors.warning;
      case 'normal': return AppColors.info;
      case 'low': return AppColors.success;
      default: return AppColors.info;
    }
  }

  IconData _priorityIcon(String priority) {
    switch (priority.toLowerCase()) {
      case 'urgent': return Icons.error_rounded;
      case 'high': return Icons.priority_high_rounded;
      case 'normal': return Icons.remove_rounded;
      case 'low': return Icons.arrow_downward_rounded;
      default: return Icons.remove_rounded;
    }
  }

  String _formatDateTime(DateTime dateTime) {
    final months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    return '${months[dateTime.month - 1]} ${dateTime.day}, ${dateTime.year}';
  }

  String _formatTime(DateTime dateTime) {
    return '${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final ticketsState = ref.watch(supportTicketsProvider);
    final appTheme = context.appTheme;
    final isDark = context.isDarkMode;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        leading: Padding(
          padding: const EdgeInsets.only(left: 12),
          child: Center(
            child: GestureDetector(
              onTap: () => Navigator.pop(context),
              child: Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: (isDark ? Colors.white : Colors.black)
                      .withAlpha(isDark ? 25 : 15),
                  borderRadius: AppRadius.radiusPill,
                  border: Border.all(
                    color: (isDark ? Colors.white : Colors.black).withAlpha(10),
                  ),
                ),
                child: ClipRRect(
                  borderRadius: AppRadius.radiusPill,
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                    child: Icon(Icons.arrow_back_rounded,
                        size: 20,
                        color: isDark ? Colors.white : Colors.black87),
                  ),
                ),
              ),
            ),
          ),
        ),
        title: Text(
          'Support Tickets',
          style: GoogleFonts.inter(
            fontSize: 17,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
          ),
        ),
        centerTitle: true,
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 12),
            child: Center(
              child: GestureDetector(
                onTap: () => ref.read(supportTicketsProvider.notifier).refresh(),
                child: Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: (isDark ? Colors.white : Colors.black)
                        .withAlpha(isDark ? 25 : 15),
                    borderRadius: AppRadius.radiusPill,
                    border: Border.all(
                      color: (isDark ? Colors.white : Colors.black).withAlpha(10),
                    ),
                  ),
                  child: ClipRRect(
                    borderRadius: AppRadius.radiusPill,
                    child: BackdropFilter(
                      filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                      child: Icon(Icons.refresh_rounded,
                          size: 20,
                          color: isDark ? Colors.white : Colors.black87),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
      body: ticketsState.when(
        data: (tickets) {
          if (tickets.isEmpty) {
            return _buildEmptyState(isDark, appTheme);
          }
          return RefreshIndicator(
            color: AppColors.accent,
            onRefresh: () => ref.read(supportTicketsProvider.notifier).refresh(),
            child: ListView.builder(
              physics: const AlwaysScrollableScrollPhysics(
                  parent: BouncingScrollPhysics()),
              padding: EdgeInsets.only(
                top: MediaQuery.of(context).padding.top + kToolbarHeight + 12,
                left: 16,
                right: 16,
                bottom: 100,
              ),
              itemCount: tickets.length,
              itemBuilder: (context, index) {
                final ticket = tickets[index];
                return _buildTicketCard(context, ticket, isDark, appTheme)
                    .animate()
                    .fadeIn(
                      duration: 500.ms,
                      delay: Duration(milliseconds: 80 * index),
                    )
                    .slideY(begin: 0.08, end: 0);
              },
            ),
          );
        },
        loading: () => const LoadingIndicator(),
        error: (error, stack) => ErrorState(
          title: 'Error loading tickets',
          message: error.toString().replaceAll('Exception: ', ''),
          onRetry: () => ref.read(supportTicketsProvider.notifier).refresh(),
        ),
      ),
      floatingActionButton: _buildFab(context, ref, isDark),
    );
  }

  //  Empty State 

  Widget _buildEmptyState(bool isDark, CustomThemeExtension appTheme) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 100,
            height: 100,
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: isDark
                    ? [AppColors.surfaceVariantDark, AppColors.surfaceDark]
                    : [AppColors.surfaceVariant, AppColors.surface],
              ),
              borderRadius: AppRadius.radiusCircle,
            ),
            child: Icon(Icons.support_agent_rounded,
                size: 44, color: appTheme.textTertiary),
          )
              .animate()
              .scale(
                begin: const Offset(0.8, 0.8),
                end: const Offset(1, 1),
                duration: 600.ms,
                curve: Curves.elasticOut,
              )
              .fadeIn(duration: 400.ms),
          AppSpacing.verticalGapLg,
          Text(
            'No support tickets yet',
            style: GoogleFonts.inter(
              fontSize: 20,
              fontWeight: FontWeight.w800,
              letterSpacing: -0.3,
              color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
            ),
          ).animate().fadeIn(delay: 200.ms, duration: 400.ms),
          AppSpacing.verticalGapXs,
          Text(
            'Tap + to create a new ticket',
            style: GoogleFonts.inter(
              fontSize: 14,
              color: appTheme.textSecondary,
            ),
          ).animate().fadeIn(delay: 300.ms, duration: 400.ms),
        ],
      ),
    );
  }

  //  Ticket Card 

  Widget _buildTicketCard(
    BuildContext context,
    dynamic ticket,
    bool isDark,
    CustomThemeExtension appTheme,
  ) {
    final accentColor = _statusAccentColor(ticket.status, appTheme);
    final pColor = _priorityColor(ticket.priority);

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GestureDetector(
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) =>
                  SupportTicketDetailsPage(ticketId: ticket.id),
            ),
          );
        },
        child: Container(
          decoration: BoxDecoration(
            color: isDark ? AppColors.cardBackgroundDark : AppColors.cardBackground,
            borderRadius: AppRadius.radiusXl,
            boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
            border: Border.all(
              color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
            ),
          ),
          child: ClipRRect(
            borderRadius: AppRadius.radiusXl,
            child: IntrinsicHeight(
              child: Row(
                children: [
                  // Left accent strip
                  Container(
                    width: 4,
                    decoration: BoxDecoration(
                      color: accentColor,
                      borderRadius: const BorderRadius.only(
                        topLeft: Radius.circular(AppRadius.xl),
                        bottomLeft: Radius.circular(AppRadius.xl),
                      ),
                    ),
                  ),
                  // Content
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(14, 16, 16, 16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Top row: subject + status
                          Row(
                            children: [
                              Container(
                                width: 36,
                                height: 36,
                                decoration: BoxDecoration(
                                  color: accentColor.withAlpha(20),
                                  borderRadius: AppRadius.radiusMd,
                                ),
                                child: Icon(
                                  _statusIcon(ticket.status),
                                  size: 18,
                                  color: accentColor,
                                ),
                              ),
                              const SizedBox(width: 10),
                              Expanded(
                                child: Text(
                                  ticket.subject,
                                  style: GoogleFonts.inter(
                                    fontSize: 15,
                                    fontWeight: FontWeight.w700,
                                    color: isDark
                                        ? AppColors.textPrimaryDark
                                        : AppColors.textPrimary,
                                    letterSpacing: -0.2,
                                  ),
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),
                              const SizedBox(width: 8),
                              StatusBadge(
                                label: ticket.statusName,
                                statusCode: _mapTicketStatusToColor(ticket.status),
                              ),
                            ],
                          ),
                          const SizedBox(height: 10),
                          // Description
                          Text(
                            ticket.description,
                            style: GoogleFonts.inter(
                              fontSize: 13,
                              color: appTheme.textSecondary,
                              height: 1.4,
                            ),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 12),
                          // Bottom row: priority + date
                          Row(
                            children: [
                              // Priority badge
                              Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 10, vertical: 4),
                                decoration: BoxDecoration(
                                  color: pColor.withAlpha(18),
                                  borderRadius: AppRadius.radiusPill,
                                  border: Border.all(
                                      color: pColor.withAlpha(40)),
                                ),
                                child: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Icon(_priorityIcon(ticket.priority),
                                        size: 12, color: pColor),
                                    const SizedBox(width: 4),
                                    Text(
                                      ticket.priority,
                                      style: GoogleFonts.inter(
                                        fontSize: 11,
                                        fontWeight: FontWeight.w700,
                                        color: pColor,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const Spacer(),
                              Icon(Icons.calendar_today_rounded,
                                  size: 12, color: appTheme.textTertiary),
                              const SizedBox(width: 4),
                              Text(
                                _formatDateTime(ticket.createdAt),
                                style: GoogleFonts.inter(
                                  fontSize: 11,
                                  fontWeight: FontWeight.w500,
                                  color: appTheme.textTertiary,
                                ),
                              ),
                              const SizedBox(width: 10),
                              Icon(Icons.schedule_rounded,
                                  size: 12, color: appTheme.textTertiary),
                              const SizedBox(width: 4),
                              Text(
                                _formatTime(ticket.createdAt),
                                style: GoogleFonts.inter(
                                  fontSize: 11,
                                  fontWeight: FontWeight.w500,
                                  color: appTheme.textTertiary,
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  //  FAB 

  Widget _buildFab(BuildContext context, WidgetRef ref, bool isDark) {
    return GestureDetector(
      onTap: () async {
        final created = await Navigator.push<bool>(
          context,
          MaterialPageRoute(
            builder: (context) => const CreateSupportTicketPage(),
          ),
        );

        if (created == true) {
          ref.read(supportTicketsProvider.notifier).refresh();
        }
      },
      child: Container(
        width: 58,
        height: 58,
        decoration: BoxDecoration(
          gradient: AppColors.primaryGradient,
          borderRadius: AppRadius.radiusPill,
          boxShadow: [
            BoxShadow(
              color: AppColors.primary.withAlpha(80),
              blurRadius: 20,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: const Icon(Icons.add_rounded, color: Colors.white, size: 28),
      ),
    )
        .animate()
        .scale(
          begin: const Offset(0, 0),
          end: const Offset(1, 1),
          duration: 500.ms,
          delay: 300.ms,
          curve: Curves.elasticOut,
        );
  }
}
