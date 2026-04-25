import 'dart:async';
import 'dart:developer' as developer;
import 'dart:io' show Platform;

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:latlong2/latlong.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../core/design/design_system.dart';

class DriverDeliveryMap extends ConsumerStatefulWidget {
  final double destinationLat;
  final double destinationLng;
  final String? destinationLabel;

  const DriverDeliveryMap({
    super.key,
    required this.destinationLat,
    required this.destinationLng,
    this.destinationLabel,
  });

  @override
  ConsumerState<DriverDeliveryMap> createState() => _DriverDeliveryMapState();
}

class _DriverDeliveryMapState extends ConsumerState<DriverDeliveryMap> {
  static const _initialZoom = 14.0;

  final MapController _mapController = MapController();
  StreamSubscription<Position>? _positionSub;
  Position? _driverPosition;
  bool _loading = true;
  bool _permissionDenied = false;
  bool _hasFitBounds = false;

  LatLng get _destination => LatLng(widget.destinationLat, widget.destinationLng);
  LatLng? get _driverLatLng => _driverPosition == null
      ? null
      : LatLng(_driverPosition!.latitude, _driverPosition!.longitude);

  @override
  void initState() {
    super.initState();
    _startPositionStream();
  }

  Future<void> _startPositionStream() async {
    try {
      if (!await Geolocator.isLocationServiceEnabled()) {
        if (mounted) setState(() => _loading = false);
        return;
      }
      var perm = await Geolocator.checkPermission();
      if (perm == LocationPermission.denied) {
        perm = await Geolocator.requestPermission();
      }
      if (perm == LocationPermission.denied ||
          perm == LocationPermission.deniedForever) {
        if (mounted) {
          setState(() {
            _permissionDenied = true;
            _loading = false;
          });
        }
        return;
      }

      try {
        final initial = await Geolocator.getCurrentPosition(
          desiredAccuracy: LocationAccuracy.high,
        );
        if (!mounted) return;
        setState(() {
          _driverPosition = initial;
          _loading = false;
        });
        _tryFitBounds();
      } catch (_) {
        if (mounted) setState(() => _loading = false);
      }

      _positionSub = Geolocator.getPositionStream(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.high,
          distanceFilter: 5,
        ),
      ).listen((pos) {
        if (!mounted) return;
        setState(() => _driverPosition = pos);
        _tryFitBounds();
      }, onError: (error) {
        developer.log('Driver map position stream error',
            name: 'DriverDeliveryMap', error: error);
      });
    } catch (e) {
      developer.log('Driver map init error',
          name: 'DriverDeliveryMap', error: e);
      if (mounted) setState(() => _loading = false);
    }
  }

  void _tryFitBounds() {
    if (_hasFitBounds) return;
    final driver = _driverLatLng;
    if (driver == null) return;
    _hasFitBounds = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _mapController.fitCamera(
        CameraFit.bounds(
          bounds: LatLngBounds.fromPoints([driver, _destination]),
          padding: const EdgeInsets.all(56),
        ),
      );
    });
  }

  void _recenterOnDriver() {
    final driver = _driverLatLng;
    if (driver != null) {
      _mapController.move(driver, 16);
    }
  }

  Future<void> _openNavigation() async {
    final lat = widget.destinationLat;
    final lng = widget.destinationLng;
    final label = widget.destinationLabel ?? 'Delivery destination';

    // Try platform-preferred URLs first; fall back to the universal web URL.
    final candidates = <Uri>[];
    if (!kIsWeb) {
      if (Platform.isAndroid) {
        candidates.add(Uri.parse('google.navigation:q=$lat,$lng&mode=d'));
        candidates.add(Uri.parse('geo:$lat,$lng?q=$lat,$lng(${Uri.encodeComponent(label)})'));
      } else if (Platform.isIOS) {
        candidates.add(Uri.parse('http://maps.apple.com/?daddr=$lat,$lng&dirflg=d'));
        candidates.add(Uri.parse('comgooglemaps://?daddr=$lat,$lng&directionsmode=driving'));
      }
    }
    candidates.add(Uri.parse(
        'https://www.google.com/maps/dir/?api=1&destination=$lat,$lng&travelmode=driving'));

    for (final uri in candidates) {
      try {
        if (await canLaunchUrl(uri)) {
          final launched =
              await launchUrl(uri, mode: LaunchMode.externalApplication);
          if (launched) return;
        }
      } catch (_) {
        // try next
      }
    }

    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Could not open a maps app',
            style: GoogleFonts.inter(fontSize: 13)),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  @override
  void dispose() {
    _positionSub?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final initialCenter = _driverLatLng ?? _destination;

    return ClipRRect(
      borderRadius: AppRadius.radiusXl,
      child: SizedBox(
        height: 300,
        child: Stack(
          children: [
            FlutterMap(
              mapController: _mapController,
              options: MapOptions(
                initialCenter: initialCenter,
                initialZoom: _initialZoom,
                interactionOptions: const InteractionOptions(
                  flags: InteractiveFlag.pinchZoom |
                      InteractiveFlag.drag |
                      InteractiveFlag.doubleTapZoom,
                ),
              ),
              children: [
                TileLayer(
                  urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                  userAgentPackageName: 'com.deliverydash.mobile',
                ),
                if (_driverLatLng != null)
                  PolylineLayer(
                    polylines: [
                      Polyline(
                        points: [_driverLatLng!, _destination],
                        color: AppColors.accent,
                        strokeWidth: 4,
                        borderColor: Colors.white,
                        borderStrokeWidth: 1,
                      ),
                    ],
                  ),
                MarkerLayer(
                  markers: [
                    Marker(
                      point: _destination,
                      width: 44,
                      height: 44,
                      child: _DestinationPin(),
                    ),
                    if (_driverLatLng != null)
                      Marker(
                        point: _driverLatLng!,
                        width: 40,
                        height: 40,
                        child: _DriverPin(heading: _driverPosition?.heading),
                      ),
                  ],
                ),
              ],
            ),
            if (_loading)
              const Positioned.fill(
                child: ColoredBox(
                  color: Color(0x33000000),
                  child: Center(
                    child: SizedBox(
                      width: 28,
                      height: 28,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    ),
                  ),
                ),
              ),
            if (_permissionDenied)
              Positioned(
                left: 12,
                right: 12,
                top: 12,
                child: _HintBanner(
                  icon: Icons.location_off_rounded,
                  text: 'Enable location to see your position',
                ),
              ),
            Positioned(
              right: 12,
              bottom: 12,
              child: _MyLocationFab(onTap: _recenterOnDriver)
                  .animate()
                  .fadeIn(duration: 300.ms)
                  .slideY(begin: 0.2, end: 0),
            ),
            Positioned(
              left: 12,
              bottom: 12,
              child: _NavigateButton(onTap: _openNavigation)
                  .animate()
                  .fadeIn(duration: 300.ms, delay: 100.ms)
                  .slideY(begin: 0.2, end: 0),
            ),
          ],
        ),
      ),
    );
  }
}

class _DestinationPin extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.primary,
        shape: BoxShape.circle,
        border: Border.all(color: Colors.white, width: 3),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(40),
            blurRadius: 8,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: const Icon(Icons.home_rounded, color: Colors.white, size: 22),
    );
  }
}

class _DriverPin extends StatelessWidget {
  final double? heading;
  const _DriverPin({this.heading});

  @override
  Widget build(BuildContext context) {
    final rotation = heading == null ? 0.0 : (heading! * 3.14159265 / 180.0);
    return Container(
      decoration: BoxDecoration(
        color: AppColors.accent,
        shape: BoxShape.circle,
        border: Border.all(color: Colors.white, width: 3),
        boxShadow: [
          BoxShadow(
            color: AppColors.accent.withAlpha(120),
            blurRadius: 12,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Transform.rotate(
        angle: rotation,
        child: const Icon(Icons.navigation_rounded,
            color: Colors.white, size: 20),
      ),
    );
  }
}

class _HintBanner extends StatelessWidget {
  final IconData icon;
  final String text;
  const _HintBanner({required this.icon, required this.text});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.white.withAlpha(230),
        borderRadius: AppRadius.radiusPill,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(20),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: AppColors.primary),
          const SizedBox(width: 8),
          Flexible(
            child: Text(
              text,
              style: GoogleFonts.inter(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                color: AppColors.textPrimary,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _MyLocationFab extends StatelessWidget {
  final VoidCallback onTap;
  const _MyLocationFab({required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      shape: const CircleBorder(),
      elevation: 4,
      child: InkWell(
        onTap: onTap,
        customBorder: const CircleBorder(),
        child: const SizedBox(
          width: 44,
          height: 44,
          child: Icon(Icons.my_location_rounded,
              color: AppColors.primary, size: 22),
        ),
      ),
    );
  }
}

class _NavigateButton extends StatelessWidget {
  final VoidCallback onTap;
  const _NavigateButton({required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColors.primary,
      borderRadius: AppRadius.radiusPill,
      elevation: 4,
      child: InkWell(
        onTap: onTap,
        borderRadius: AppRadius.radiusPill,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.directions_rounded,
                  color: Colors.white, size: 18),
              const SizedBox(width: 8),
              Text(
                'Navigate',
                style: GoogleFonts.inter(
                  color: Colors.white,
                  fontSize: 13,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.2,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
