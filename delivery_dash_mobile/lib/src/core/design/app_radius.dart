import 'package:flutter/material.dart';

/// Premium border radius constants with modern, rounded feel.
abstract final class AppRadius {
  static const double none = 0.0;
  static const double xs = 4.0;
  static const double sm = 8.0;
  static const double md = 12.0;
  static const double lg = 16.0;
  static const double xl = 20.0;
  static const double xxl = 24.0;
  static const double pill = 100.0;
  static const double circle = 999.0;

  static const BorderRadius radiusNone = BorderRadius.zero;
  static const BorderRadius radiusXs = BorderRadius.all(Radius.circular(xs));
  static const BorderRadius radiusSm = BorderRadius.all(Radius.circular(sm));
  static const BorderRadius radiusMd = BorderRadius.all(Radius.circular(md));
  static const BorderRadius radiusLg = BorderRadius.all(Radius.circular(lg));
  static const BorderRadius radiusXl = BorderRadius.all(Radius.circular(xl));
  static const BorderRadius radiusXxl = BorderRadius.all(Radius.circular(xxl));
  static const BorderRadius radiusPill = BorderRadius.all(Radius.circular(pill));
  static const BorderRadius radiusCircle = BorderRadius.all(Radius.circular(circle));

  static const BorderRadius radiusTopMd = BorderRadius.only(
    topLeft: Radius.circular(md), topRight: Radius.circular(md),
  );
  static const BorderRadius radiusTopLg = BorderRadius.only(
    topLeft: Radius.circular(lg), topRight: Radius.circular(lg),
  );
  static const BorderRadius radiusTopXl = BorderRadius.only(
    topLeft: Radius.circular(xl), topRight: Radius.circular(xl),
  );
  static const BorderRadius radiusTopXxl = BorderRadius.only(
    topLeft: Radius.circular(xxl), topRight: Radius.circular(xxl),
  );
  static const BorderRadius radiusBottomMd = BorderRadius.only(
    bottomLeft: Radius.circular(md), bottomRight: Radius.circular(md),
  );

  static const RoundedRectangleBorder cardShape = RoundedRectangleBorder(borderRadius: radiusXl);
  static const RoundedRectangleBorder buttonShape = RoundedRectangleBorder(borderRadius: radiusMd);
  static const RoundedRectangleBorder pillButtonShape = RoundedRectangleBorder(borderRadius: radiusPill);
  static const RoundedRectangleBorder dialogShape = RoundedRectangleBorder(borderRadius: radiusXxl);
  static const RoundedRectangleBorder bottomSheetShape = RoundedRectangleBorder(borderRadius: radiusTopXxl);
  static const RoundedRectangleBorder badgeShape = RoundedRectangleBorder(borderRadius: radiusPill);
  static const RoundedRectangleBorder chipShape = RoundedRectangleBorder(borderRadius: radiusPill);
}
