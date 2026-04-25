import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/loading_indicator.dart';
import '../data/support_ticket_model.dart';
import '../data/support_ticket_repository.dart';

final ticketDetailsProvider = FutureProvider.family<SupportTicket, int>((ref, ticketId) async {
  final repository = ref.watch(supportTicketRepositoryProvider);
  return await repository.getTicketById(ticketId);
});

class SupportTicketDetailsPage extends ConsumerWidget {
  final int ticketId;

  const SupportTicketDetailsPage({super.key, required this.ticketId});

  Color _getStatusColor(BuildContext context, int status) {
    final appTheme = context.appTheme;
    switch (status) {
      case 1:
        return appTheme.warningColor;
      case 2:
        return appTheme.infoColor;
      case 3:
        return appTheme.successColor;
      case 4:
        return appTheme.textTertiary;
      default:
        return appTheme.textTertiary;
    }
  }

  Color _getPriorityColor(BuildContext context, String priority) {
    final appTheme = context.appTheme;
    switch (priority) {
      case 'Low':
        return appTheme.successColor;
      case 'Normal':
        return appTheme.infoColor;
      case 'High':
        return appTheme.warningColor;
      case 'Urgent':
        return appTheme.errorColor;
      default:
        return appTheme.textTertiary;
    }
  }

  IconData _getStatusIcon(int status) {
    switch (status) {
      case 1:
        return Icons.schedule_rounded;
      case 2:
        return Icons.sync_rounded;
      case 3:
        return Icons.check_circle_rounded;
      case 4:
        return Icons.lock_rounded;
      default:
        return Icons.help_outline_rounded;
    }
  }

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour}:${dateTime.minute.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final appTheme = context.appTheme;
    final isDark = context.isDarkMode;
    final ticketState = ref.watch(ticketDetailsProvider(ticketId));

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      body: ticketState.when(
        data: (ticket) {
          return RefreshIndicator(
            onRefresh: () => ref.refresh(ticketDetailsProvider(ticketId).future),
            child: CustomScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              slivers: [
                // Gradient App Bar with ticket info
                SliverAppBar(
                  expandedHeight: 180,
                  pinned: true,
                  backgroundColor: isDark ? AppColors.primaryDark : AppColors.primary,
                  leading: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: GestureDetector(
                      onTap: () => Navigator.pop(context),
                      child: Container(
                        decoration: BoxDecoration(
                          color: Colors.white.withValues(alpha: 0.15),
                          borderRadius: AppRadius.radiusPill,
                        ),
                        child: const Icon(Icons.arrow_back_rounded, color: Colors.white, size: 20),
                      ),
                    ),
                  ),
                  flexibleSpace: FlexibleSpaceBar(
                    background: Container(
                      decoration: const BoxDecoration(
                        gradient: AppColors.primaryGradient,
                      ),
                      child: SafeArea(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(20, 56, 20, 20),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            mainAxisAlignment: MainAxisAlignment.end,
                            children: [
                              Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                                    decoration: BoxDecoration(
                                      color: Colors.white.withValues(alpha: 0.15),
                                      borderRadius: AppRadius.radiusPill,
                                    ),
                                    child: Text(
                                      'Ticket #${ticket.id}',
                                      style: GoogleFonts.inter(
                                        fontSize: 12,
                                        fontWeight: FontWeight.w600,
                                        color: AppColors.accentLight,
                                      ),
                                    ),
                                  ),
                                  const SizedBox(width: 10),
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                                    decoration: BoxDecoration(
                                      color: _getStatusColor(context, ticket.status).withValues(alpha: 0.25),
                                      borderRadius: AppRadius.radiusPill,
                                      border: Border.all(
                                        color: _getStatusColor(context, ticket.status).withValues(alpha: 0.4),
                                      ),
                                    ),
                                    child: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        Icon(
                                          _getStatusIcon(ticket.status),
                                          size: 13,
                                          color: _getStatusColor(context, ticket.status),
                                        ),
                                        const SizedBox(width: 5),
                                        Text(
                                          ticket.statusName,
                                          style: GoogleFonts.inter(
                                            fontSize: 12,
                                            fontWeight: FontWeight.w700,
                                            color: _getStatusColor(context, ticket.status),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 12),
                              Text(
                                ticket.subject,
                                style: GoogleFonts.inter(
                                  fontSize: 20,
                                  fontWeight: FontWeight.w800,
                                  color: Colors.white,
                                  height: 1.3,
                                ),
                                maxLines: 2,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
                ),

                // Body content
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Ticket Info Row
                        Container(
                          padding: AppSpacing.allMd,
                          decoration: BoxDecoration(
                            color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                            borderRadius: AppRadius.radiusXl,
                            boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                          ),
                          child: Row(
                            children: [
                              _buildInfoChip(
                                context,
                                icon: Icons.flag_rounded,
                                label: 'Priority',
                                value: ticket.priority,
                                color: _getPriorityColor(context, ticket.priority),
                              ),
                              Container(
                                width: 1,
                                height: 36,
                                color: appTheme.dividerColor,
                              ),
                              _buildInfoChip(
                                context,
                                icon: Icons.calendar_today_rounded,
                                label: 'Created',
                                value: '${ticket.createdAt.day}/${ticket.createdAt.month}/${ticket.createdAt.year}',
                                color: appTheme.textSecondary,
                              ),
                            ],
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: 100.ms).slideY(begin: 0.05, end: 0),

                        const SizedBox(height: 24),

                        // Description Section
                        _buildSectionHeader(context, 'Description', Icons.description_rounded),
                        const SizedBox(height: 12),
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(18),
                          decoration: BoxDecoration(
                            color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                            borderRadius: AppRadius.radiusLg,
                            boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                          ),
                          child: Text(
                            ticket.description,
                            style: GoogleFonts.inter(
                              fontSize: 15,
                              height: 1.7,
                              color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                            ),
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: 200.ms).slideY(begin: 0.05, end: 0),

                        const SizedBox(height: 24),

                        // Admin Notes - message timeline
                        if (ticket.adminNotes != null && ticket.adminNotes!.isNotEmpty) ...[
                          _buildSectionHeader(context, 'Support Response', Icons.support_agent_rounded),
                          const SizedBox(height: 12),

                          // User message bubble
                          _buildMessageBubble(
                            context,
                            isSupport: false,
                            message: ticket.description,
                            label: 'You',
                            icon: Icons.person_rounded,
                          ).animate().fadeIn(duration: 400.ms, delay: 300.ms).slideX(begin: -0.05, end: 0),

                          const SizedBox(height: 12),

                          // Timeline connector
                          Padding(
                            padding: const EdgeInsets.only(left: 24),
                            child: Container(
                              width: 2,
                              height: 24,
                              decoration: BoxDecoration(
                                gradient: LinearGradient(
                                  begin: Alignment.topCenter,
                                  end: Alignment.bottomCenter,
                                  colors: [
                                    appTheme.infoColor.withValues(alpha: 0.3),
                                    appTheme.infoColor.withValues(alpha: 0.1),
                                  ],
                                ),
                                borderRadius: AppRadius.radiusPill,
                              ),
                            ),
                          ),

                          const SizedBox(height: 4),

                          // Support response bubble
                          _buildMessageBubble(
                            context,
                            isSupport: true,
                            message: ticket.adminNotes!,
                            label: 'Support Team',
                            icon: Icons.support_agent_rounded,
                          ).animate().fadeIn(duration: 400.ms, delay: 400.ms).slideX(begin: 0.05, end: 0),

                          const SizedBox(height: 24),
                        ],

                        // Timestamps card
                        _buildSectionHeader(context, 'Timeline', Icons.timeline_rounded),
                        const SizedBox(height: 12),
                        Container(
                          padding: const EdgeInsets.all(18),
                          decoration: BoxDecoration(
                            color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                            borderRadius: AppRadius.radiusXl,
                            boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                          ),
                          child: Column(
                            children: [
                              _buildTimelineRow(
                                context,
                                icon: Icons.add_circle_outline_rounded,
                                label: 'Created',
                                value: _formatDateTime(ticket.createdAt),
                                color: appTheme.infoColor,
                              ),
                              if (ticket.updatedAt != null) ...[
                                Padding(
                                  padding: const EdgeInsets.only(left: 15),
                                  child: Container(
                                    width: 2,
                                    height: 20,
                                    color: appTheme.dividerColor,
                                  ),
                                ),
                                _buildTimelineRow(
                                  context,
                                  icon: Icons.update_rounded,
                                  label: 'Last Updated',
                                  value: _formatDateTime(ticket.updatedAt!),
                                  color: appTheme.successColor,
                                ),
                              ],
                            ],
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: 500.ms).slideY(begin: 0.05, end: 0),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        },
        loading: () => const LoadingIndicator(),
        error: (error, stack) => ErrorState(
          title: 'Error loading ticket',
          message: error.toString().replaceAll('Exception: ', ''),
          onRetry: () => ref.invalidate(ticketDetailsProvider(ticketId)),
        ),
      ),
    );
  }

  Widget _buildSectionHeader(BuildContext context, String title, IconData icon) {
    final isDark = context.isDarkMode;
    return Row(
      children: [
        Icon(icon, size: 18, color: isDark ? AppColors.accentLight : AppColors.primary),
        const SizedBox(width: 8),
        Text(
          title,
          style: GoogleFonts.inter(
            fontSize: 16,
            fontWeight: FontWeight.w800,
            color: isDark ? Colors.white : AppColors.textPrimary,
            letterSpacing: -0.3,
          ),
        ),
      ],
    );
  }

  Widget _buildInfoChip(
    BuildContext context, {
    required IconData icon,
    required String label,
    required String value,
    required Color color,
  }) {
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;
    return Expanded(
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.12),
              borderRadius: AppRadius.radiusSm,
            ),
            child: Icon(icon, size: 16, color: color),
          ),
          const SizedBox(width: 10),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: GoogleFonts.inter(
                  fontSize: 11,
                  fontWeight: FontWeight.w500,
                  color: appTheme.textTertiary,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: GoogleFonts.inter(
                  fontSize: 14,
                  fontWeight: FontWeight.w700,
                  color: isDark ? Colors.white : AppColors.textPrimary,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildMessageBubble(
    BuildContext context, {
    required bool isSupport,
    required String message,
    required String label,
    required IconData icon,
  }) {
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;
    final bubbleColor = isSupport
        ? appTheme.infoColor.withValues(alpha: 0.08)
        : (isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant);
    final borderColor = isSupport
        ? appTheme.infoColor.withValues(alpha: 0.2)
        : (isDark ? AppColors.outlineDark : AppColors.outlineVariant);
    final iconColor = isSupport ? appTheme.infoColor : AppColors.accent;

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: bubbleColor,
        borderRadius: BorderRadius.only(
          topLeft: const Radius.circular(18),
          topRight: const Radius.circular(18),
          bottomLeft: Radius.circular(isSupport ? 18 : 4),
          bottomRight: Radius.circular(isSupport ? 4 : 18),
        ),
        border: Border.all(color: borderColor),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 28,
                height: 28,
                decoration: BoxDecoration(
                  color: iconColor.withValues(alpha: 0.15),
                  borderRadius: AppRadius.radiusSm,
                ),
                child: Icon(icon, size: 15, color: iconColor),
              ),
              const SizedBox(width: 8),
              Text(
                label,
                style: GoogleFonts.inter(
                  fontSize: 13,
                  fontWeight: FontWeight.w700,
                  color: isDark ? Colors.white : AppColors.textPrimary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Text(
            message,
            style: GoogleFonts.inter(
              fontSize: 14,
              height: 1.6,
              color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTimelineRow(
    BuildContext context, {
    required IconData icon,
    required String label,
    required String value,
    required Color color,
  }) {
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;
    return Row(
      children: [
        Container(
          width: 32,
          height: 32,
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.12),
            shape: BoxShape.circle,
          ),
          child: Icon(icon, size: 16, color: color),
        ),
        const SizedBox(width: 14),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: GoogleFonts.inter(
                  fontSize: 12,
                  fontWeight: FontWeight.w500,
                  color: appTheme.textTertiary,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: GoogleFonts.inter(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: isDark ? Colors.white : AppColors.textPrimary,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
