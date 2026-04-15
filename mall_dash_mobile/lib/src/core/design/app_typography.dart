import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'app_colors.dart';

/// Premium typography system using Google Fonts Inter.
abstract final class AppTypography {
  // ==========================================================================
  // FONT FAMILY
  // ==========================================================================
  
  static String get fontFamily => GoogleFonts.inter().fontFamily!;
  
  static TextStyle get _baseStyle => GoogleFonts.inter();
  
  // ==========================================================================
  // FONT WEIGHTS
  // ==========================================================================
  
  static const FontWeight thin = FontWeight.w100;
  static const FontWeight extraLight = FontWeight.w200;
  static const FontWeight light = FontWeight.w300;
  static const FontWeight regular = FontWeight.w400;
  static const FontWeight medium = FontWeight.w500;
  static const FontWeight semiBold = FontWeight.w600;
  static const FontWeight bold = FontWeight.w700;
  static const FontWeight extraBold = FontWeight.w800;
  static const FontWeight black = FontWeight.w900;
  
  // ==========================================================================
  // FONT SIZES
  // ==========================================================================
  
  static const double sizeCaption = 10.0;
  static const double sizeLabelSm = 11.0;
  static const double sizeLabel = 12.0;
  static const double sizeBodySm = 13.0;
  static const double sizeBody = 14.0;
  static const double sizeBodyLg = 16.0;
  static const double sizeTitleSm = 18.0;
  static const double sizeTitle = 20.0;
  static const double sizeTitleLg = 22.0;
  static const double sizeHeadlineSm = 24.0;
  static const double sizeHeadline = 28.0;
  static const double sizeHeadlineLg = 32.0;
  static const double sizeDisplaySm = 36.0;
  static const double sizeDisplay = 45.0;
  
  // ==========================================================================
  // LETTER SPACING
  // ==========================================================================
  
  static const double trackingTight = -0.5;
  static const double trackingNormal = 0.0;
  static const double trackingWide = 0.5;
  static const double trackingExtraWide = 1.5;
  
  // ==========================================================================
  // LINE HEIGHTS
  // ==========================================================================
  
  static const double heightTight = 1.2;
  static const double heightNormal = 1.5;
  static const double heightRelaxed = 1.75;
  
  // ==========================================================================
  // TEXT STYLES - Light Theme
  // ==========================================================================
  
  static TextStyle get caption => _baseStyle.copyWith(
    fontSize: sizeCaption,
    fontWeight: regular,
    color: AppColors.textTertiary,
    letterSpacing: trackingWide,
  );
  
  static TextStyle get label => _baseStyle.copyWith(
    fontSize: sizeLabel,
    fontWeight: medium,
    color: AppColors.textSecondary,
    letterSpacing: trackingWide,
  );
  
  static TextStyle get labelBold => _baseStyle.copyWith(
    fontSize: sizeLabel,
    fontWeight: bold,
    color: AppColors.textSecondary,
    letterSpacing: trackingWide,
  );
  
  static TextStyle get bodySmall => _baseStyle.copyWith(
    fontSize: sizeBodySm,
    fontWeight: regular,
    color: AppColors.textSecondary,
  );
  
  static TextStyle get body => _baseStyle.copyWith(
    fontSize: sizeBody,
    fontWeight: regular,
    color: AppColors.textPrimary,
  );
  
  static TextStyle get bodyBold => _baseStyle.copyWith(
    fontSize: sizeBody,
    fontWeight: semiBold,
    color: AppColors.textPrimary,
  );
  
  static TextStyle get bodyLarge => _baseStyle.copyWith(
    fontSize: sizeBodyLg,
    fontWeight: regular,
    color: AppColors.textPrimary,
  );
  
  static TextStyle get bodyLargeBold => _baseStyle.copyWith(
    fontSize: sizeBodyLg,
    fontWeight: semiBold,
    color: AppColors.textPrimary,
  );
  
  static TextStyle get titleSmall => _baseStyle.copyWith(
    fontSize: sizeTitleSm,
    fontWeight: semiBold,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
  );
  
  static TextStyle get title => _baseStyle.copyWith(
    fontSize: sizeTitle,
    fontWeight: semiBold,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
  );
  
  static TextStyle get titleLarge => _baseStyle.copyWith(
    fontSize: sizeTitleLg,
    fontWeight: bold,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
  );
  
  static TextStyle get headlineSmall => _baseStyle.copyWith(
    fontSize: sizeHeadlineSm,
    fontWeight: bold,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
  );
  
  static TextStyle get headline => _baseStyle.copyWith(
    fontSize: sizeHeadline,
    fontWeight: bold,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
  );
  
  static TextStyle get headlineLarge => _baseStyle.copyWith(
    fontSize: sizeHeadlineLg,
    fontWeight: extraBold,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
  );
  
  static TextStyle get display => _baseStyle.copyWith(
    fontSize: sizeDisplay,
    fontWeight: black,
    color: AppColors.textPrimary,
    letterSpacing: trackingTight,
    height: heightTight,
  );
  
  // ==========================================================================
  // HELPER METHODS
  // ==========================================================================
  
  static TextStyle primary(TextStyle base) => base.copyWith(color: AppColors.primary);
  static TextStyle secondary(TextStyle base) => base.copyWith(color: AppColors.textSecondary);
  static TextStyle tertiary(TextStyle base) => base.copyWith(color: AppColors.textTertiary);
  static TextStyle success(TextStyle base) => base.copyWith(color: AppColors.success);
  static TextStyle error(TextStyle base) => base.copyWith(color: AppColors.error);
  static TextStyle warning(TextStyle base) => base.copyWith(color: AppColors.warning);
  static TextStyle gold(TextStyle base) => base.copyWith(color: AppColors.accent);
}
