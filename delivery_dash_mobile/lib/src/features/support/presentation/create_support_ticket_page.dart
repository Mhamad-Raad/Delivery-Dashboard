import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../data/support_ticket_model.dart';
import '../data/support_ticket_repository.dart';

class CreateSupportTicketPage extends ConsumerStatefulWidget {
  const CreateSupportTicketPage({super.key});

  @override
  ConsumerState<CreateSupportTicketPage> createState() => _CreateSupportTicketPageState();
}

class _CreateSupportTicketPageState extends ConsumerState<CreateSupportTicketPage> {
  final _formKey = GlobalKey<FormState>();
  final _subjectController = TextEditingController();
  final _descriptionController = TextEditingController();
  String _selectedPriority = 'Normal';
  bool _isSubmitting = false;

  final List<String> _priorities = ['Low', 'Normal', 'High', 'Urgent'];

  @override
  void dispose() {
    _subjectController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _submitTicket() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final request = CreateSupportTicketRequest(
        subject: _subjectController.text.trim(),
        description: _descriptionController.text.trim(),
        priority: _selectedPriority,
      );

      final repository = ref.read(supportTicketRepositoryProvider);
      await repository.createTicket(request);

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('Support ticket created successfully'),
          backgroundColor: context.appTheme.successColor,
        ),
      );

      Navigator.pop(context, true);
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(e.toString().replaceAll('Exception: ', '')),
          backgroundColor: context.appTheme.errorColor,
          duration: const Duration(seconds: 4),
        ),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  Color _getPriorityColorFromTheme(BuildContext context, String priority) {
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

  IconData _getPriorityIcon(String priority) {
    switch (priority) {
      case 'Low':
        return Icons.arrow_downward_rounded;
      case 'Normal':
        return Icons.remove_rounded;
      case 'High':
        return Icons.arrow_upward_rounded;
      case 'Urgent':
        return Icons.priority_high_rounded;
      default:
        return Icons.remove_rounded;
    }
  }

  @override
  Widget build(BuildContext context) {
    final appTheme = context.appTheme;
    final isDark = context.isDarkMode;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        leading: Padding(
          padding: const EdgeInsets.all(8.0),
          child: GestureDetector(
            onTap: () => Navigator.pop(context),
            child: Container(
              decoration: BoxDecoration(
                color: (isDark ? Colors.white : AppColors.primary).withValues(alpha: 0.1),
                borderRadius: AppRadius.radiusPill,
              ),
              child: Icon(
                Icons.arrow_back_rounded,
                color: isDark ? Colors.white : AppColors.primary,
                size: 20,
              ),
            ),
          ),
        ),
        title: Text(
          'New Ticket',
          style: GoogleFonts.inter(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            color: isDark ? Colors.white : AppColors.textPrimary,
          ),
        ),
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 32),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Header illustration
              Container(
                padding: AppSpacing.allLg,
                decoration: BoxDecoration(
                  gradient: isDark
                      ? const LinearGradient(
                          colors: [Color(0xFF1E1E2D), Color(0xFF2A2A3A)],
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                        )
                      : const LinearGradient(
                          colors: [Color(0xFFF0EDFF), Color(0xFFE8F4FD)],
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                        ),
                  borderRadius: AppRadius.radiusXl,
                ),
                child: Row(
                  children: [
                    Container(
                      width: 52,
                      height: 52,
                      decoration: BoxDecoration(
                        gradient: AppColors.primaryGradient,
                        borderRadius: AppRadius.radiusMd,
                        boxShadow: AppShadows.coloredShadow(AppColors.primary),
                      ),
                      child: const Icon(
                        Icons.support_agent_rounded,
                        color: Colors.white,
                        size: 26,
                      ),
                    ),
                    AppSpacing.horizontalGapMd,
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'How can we help?',
                            style: GoogleFonts.inter(
                              fontSize: 17,
                              fontWeight: FontWeight.w700,
                              color: isDark ? Colors.white : AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            'Describe your issue and we\'ll get back to you shortly.',
                            style: GoogleFonts.inter(
                              fontSize: 13,
                              color: appTheme.textSecondary,
                              height: 1.4,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ).animate().fadeIn(duration: 400.ms).slideY(begin: -0.1, end: 0),

              const SizedBox(height: 28),

              // Subject field
              _buildSectionLabel('Subject', Icons.title_rounded, context),
              const SizedBox(height: 10),
              TextFormField(
                controller: _subjectController,
                style: GoogleFonts.inter(
                  fontSize: 15,
                  fontWeight: FontWeight.w500,
                  color: isDark ? Colors.white : AppColors.textPrimary,
                ),
                decoration: _buildInputDecoration(
                  context,
                  hintText: 'Brief description of the issue',
                  prefixIcon: Icons.subject_rounded,
                ),
                validator: (value) {
                  if (value == null || value.trim().isEmpty) {
                    return 'Please enter a subject';
                  }
                  if (value.trim().length < 5) {
                    return 'Subject must be at least 5 characters';
                  }
                  return null;
                },
                maxLength: 200,
              ).animate().fadeIn(duration: 400.ms, delay: 100.ms).slideX(begin: -0.05, end: 0),

              const SizedBox(height: 20),

              // Priority selector
              _buildSectionLabel('Priority', Icons.flag_rounded, context),
              const SizedBox(height: 10),
              Wrap(
                spacing: 10,
                runSpacing: 10,
                children: _priorities.map((priority) {
                  final isSelected = _selectedPriority == priority;
                  final color = _getPriorityColorFromTheme(context, priority);
                  return GestureDetector(
                    onTap: () => setState(() => _selectedPriority = priority),
                    child: AnimatedContainer(
                      duration: 250.ms,
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                      decoration: BoxDecoration(
                        color: isSelected ? color.withValues(alpha: 0.15) : (isDark ? AppColors.surfaceVariantDark : Colors.white),
                        borderRadius: AppRadius.radiusPill,
                        border: Border.all(
                          color: isSelected ? color : (isDark ? AppColors.outlineDark : AppColors.outlineVariant),
                          width: isSelected ? 1.8 : 1,
                        ),
                        boxShadow: isSelected ? AppShadows.coloredShadow(color, blur: 12, spread: -6) : AppShadows.none,
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            _getPriorityIcon(priority),
                            size: 16,
                            color: isSelected ? color : appTheme.textTertiary,
                          ),
                          const SizedBox(width: 6),
                          Text(
                            priority,
                            style: GoogleFonts.inter(
                              fontSize: 13,
                              fontWeight: isSelected ? FontWeight.w700 : FontWeight.w500,
                              color: isSelected ? color : appTheme.textSecondary,
                            ),
                          ),
                        ],
                      ),
                    ),
                  );
                }).toList(),
              ).animate().fadeIn(duration: 400.ms, delay: 200.ms).slideX(begin: -0.05, end: 0),

              const SizedBox(height: 20),

              // Description field
              _buildSectionLabel('Description', Icons.description_rounded, context),
              const SizedBox(height: 10),
              TextFormField(
                controller: _descriptionController,
                style: GoogleFonts.inter(
                  fontSize: 15,
                  fontWeight: FontWeight.w500,
                  color: isDark ? Colors.white : AppColors.textPrimary,
                ),
                decoration: _buildInputDecoration(
                  context,
                  hintText: 'Provide detailed information about your issue...',
                  prefixIcon: null,
                  alignLabelWithHint: true,
                ),
                maxLines: 7,
                maxLength: 1000,
                validator: (value) {
                  if (value == null || value.trim().isEmpty) {
                    return 'Please enter a description';
                  }
                  if (value.trim().length < 20) {
                    return 'Description must be at least 20 characters';
                  }
                  return null;
                },
              ).animate().fadeIn(duration: 400.ms, delay: 300.ms).slideX(begin: -0.05, end: 0),

              const SizedBox(height: 20),

              // Info Card
              Container(
                padding: AppSpacing.allMd,
                decoration: BoxDecoration(
                  color: appTheme.infoColor.withValues(alpha: 0.08),
                  borderRadius: AppRadius.radiusLg,
                  border: Border.all(color: appTheme.infoColor.withValues(alpha: 0.2)),
                ),
                child: Row(
                  children: [
                    Container(
                      width: 36,
                      height: 36,
                      decoration: BoxDecoration(
                        color: appTheme.infoColor.withValues(alpha: 0.15),
                        borderRadius: AppRadius.radiusSm,
                      ),
                      child: Icon(Icons.info_outline_rounded, color: appTheme.infoColor, size: 18),
                    ),
                    AppSpacing.horizontalGapSm,
                    Expanded(
                      child: Text(
                        'Our support team will review your ticket and respond as soon as possible.',
                        style: GoogleFonts.inter(
                          fontSize: 13,
                          color: appTheme.infoColor,
                          height: 1.5,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                  ],
                ),
              ).animate().fadeIn(duration: 400.ms, delay: 400.ms),

              const SizedBox(height: 28),

              // Submit Button
              Container(
                height: 56,
                decoration: BoxDecoration(
                  gradient: _isSubmitting ? null : AppColors.primaryGradient,
                  color: _isSubmitting ? (isDark ? AppColors.surfaceVariantDark : AppColors.outline) : null,
                  borderRadius: AppRadius.radiusLg,
                  boxShadow: _isSubmitting ? AppShadows.none : AppShadows.coloredShadow(AppColors.primary),
                ),
                child: Material(
                  color: Colors.transparent,
                  child: InkWell(
                    onTap: _isSubmitting ? null : _submitTicket,
                    borderRadius: AppRadius.radiusLg,
                    child: Center(
                      child: _isSubmitting
                          ? const SizedBox(
                              width: 22,
                              height: 22,
                              child: CircularProgressIndicator(
                                color: Colors.white,
                                strokeWidth: 2.5,
                              ),
                            )
                          : Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const Icon(Icons.send_rounded, color: Colors.white, size: 20),
                                const SizedBox(width: 10),
                                Text(
                                  'Submit Ticket',
                                  style: GoogleFonts.inter(
                                    fontSize: 16,
                                    fontWeight: FontWeight.w700,
                                    color: Colors.white,
                                  ),
                                ),
                              ],
                            ),
                    ),
                  ),
                ),
              ).animate().fadeIn(duration: 400.ms, delay: 500.ms).slideY(begin: 0.1, end: 0),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSectionLabel(String label, IconData icon, BuildContext context) {
    final isDark = context.isDarkMode;
    return Row(
      children: [
        Icon(icon, size: 16, color: isDark ? AppColors.accentLight : AppColors.primary),
        const SizedBox(width: 8),
        Text(
          label,
          style: GoogleFonts.inter(
            fontSize: 14,
            fontWeight: FontWeight.w700,
            color: isDark ? Colors.white : AppColors.textPrimary,
            letterSpacing: -0.2,
          ),
        ),
      ],
    );
  }

  InputDecoration _buildInputDecoration(
    BuildContext context, {
    required String hintText,
    IconData? prefixIcon,
    bool alignLabelWithHint = false,
  }) {
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;
    return InputDecoration(
      hintText: hintText,
      hintStyle: GoogleFonts.inter(
        fontSize: 14,
        color: appTheme.textTertiary,
        fontWeight: FontWeight.w400,
      ),
      prefixIcon: prefixIcon != null
          ? Icon(prefixIcon, color: appTheme.textTertiary, size: 20)
          : null,
      alignLabelWithHint: alignLabelWithHint,
      filled: true,
      fillColor: isDark ? AppColors.surfaceVariantDark : Colors.white,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      border: OutlineInputBorder(
        borderRadius: AppRadius.radiusMd,
        borderSide: BorderSide(color: isDark ? AppColors.outlineDark : AppColors.outlineVariant),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: AppRadius.radiusMd,
        borderSide: BorderSide(color: isDark ? AppColors.outlineDark : AppColors.outlineVariant),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: AppRadius.radiusMd,
        borderSide: BorderSide(color: AppColors.accent, width: 1.8),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: AppRadius.radiusMd,
        borderSide: BorderSide(color: appTheme.errorColor),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: AppRadius.radiusMd,
        borderSide: BorderSide(color: appTheme.errorColor, width: 1.8),
      ),
      counterStyle: GoogleFonts.inter(fontSize: 11, color: appTheme.textTertiary),
    );
  }
}
