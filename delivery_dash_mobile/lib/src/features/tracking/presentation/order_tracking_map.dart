import 'dart:async';
import 'dart:developer' as developer;

import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:latlong2/latlong.dart';
import 'package:signalr_core/signalr_core.dart';

import '../../../core/design/design_system.dart';
import '../../../core/network/dio_provider.dart';
import '../../../core/storage/token_storage_service.dart';
import '../data/driver_location_model.dart';
import '../data/order_tracking_repository.dart';

class OrderTrackingMap extends ConsumerStatefulWidget {
  final int orderId;
  final double? destinationLat;
  final double? destinationLng;

  const OrderTrackingMap({
    super.key,
    required this.orderId,
    required this.destinationLat,
    required this.destinationLng,
  });

  @override
  ConsumerState<OrderTrackingMap> createState() => _OrderTrackingMapState();
}

class _OrderTrackingMapState extends ConsumerState<OrderTrackingMap> {
  static const _defaultCenter = LatLng(33.3152, 44.3661); // Baghdad
  static const _initialZoom = 14.0;

  final MapController _mapController = MapController();
  HubConnection? _connection;
  DriverLocation? _driverLocation;
  bool _hasFitBounds = false;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _init();
  }

  Future<void> _init() async {
    try {
      final repo = ref.read(orderTrackingRepositoryProvider);
      final initial = await repo.getDriverLocation(widget.orderId);
      if (!mounted) return;
      setState(() {
        _driverLocation = initial;
        _loading = false;
      });
      _tryFitBounds();
      await _connectHub();
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString();
        _loading = false;
      });
    }
  }

  Future<void> _connectHub() async {
    try {
      final dio = ref.read(dioProvider);
      final tokenStorage = ref.read(tokenStorageServiceProvider);
      final baseUri = Uri.parse(dio.options.baseUrl);
      final hubUri = baseUri.replace(path: '/hubs/tracking');

      final connection = HubConnectionBuilder()
          .withUrl(
            hubUri.toString(),
            HttpConnectionOptions(
              accessTokenFactory: () async =>
                  (await tokenStorage.getAccessToken()) ?? '',
            ),
          )
          .withAutomaticReconnect()
          .build();

      connection.on('DriverLocationUpdated', (arguments) {
        if (!mounted || arguments == null || arguments.isEmpty) return;
        final payload = arguments.first;
        if (payload is Map<String, dynamic>) {
          try {
            final loc = DriverLocation.fromJson(payload);
            if (loc.orderId != widget.orderId) return;
            setState(() => _driverLocation = loc);
            _tryFitBounds();
          } catch (e) {
            developer.log('Bad location payload',
                name: 'OrderTrackingMap', error: e);
          }
        }
      });

      await connection.start();
      await connection.invoke('SubscribeToOrder', args: [widget.orderId]);
      _connection = connection;
    } catch (e) {
      developer.log('Tracking hub connect failed',
          name: 'OrderTrackingMap', error: e);
    }
  }

  void _tryFitBounds() {
    if (_hasFitBounds) return;
    final dest = _destination;
    final driver = _driverLatLng;
    if (dest != null && driver != null) {
      _hasFitBounds = true;
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (!mounted) return;
        final bounds = LatLngBounds.fromPoints([dest, driver]);
        _mapController.fitCamera(
          CameraFit.bounds(
            bounds: bounds,
            padding: const EdgeInsets.all(48),
          ),
        );
      });
    } else if (driver != null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (!mounted) return;
        _mapController.move(driver, 15);
      });
    }
  }

  Future<void> _recenterOnMe() async {
    try {
      if (!await Geolocator.isLocationServiceEnabled()) {
        _snack('Location services are off');
        return;
      }
      var permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
      }
      if (permission == LocationPermission.denied ||
          permission == LocationPermission.deniedForever) {
        _snack('Location permission denied');
        return;
      }
      final pos = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );
      if (!mounted) return;
      _mapController.move(LatLng(pos.latitude, pos.longitude), 16);
    } catch (e) {
      _snack('Could not get current location');
    }
  }

  void _snack(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message, style: GoogleFonts.inter(fontSize: 13)),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  LatLng? get _destination {
    if (widget.destinationLat == null || widget.destinationLng == null) {
      return null;
    }
    return LatLng(widget.destinationLat!, widget.destinationLng!);
  }

  LatLng? get _driverLatLng =>
      _driverLocation == null ? null : LatLng(_driverLocation!.lat, _driverLocation!.lng);

  @override
  void dispose() {
    final conn = _connection;
    _connection = null;
    if (conn != null) {
      // Best-effort unsubscribe + stop
      conn.invoke('UnsubscribeFromOrder', args: [widget.orderId]).catchError((_) => null);
      conn.stop();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final initialCenter = _driverLatLng ?? _destination ?? _defaultCenter;

    return ClipRRect(
      borderRadius: AppRadius.radiusXl,
      child: SizedBox(
        height: 280,
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
                MarkerLayer(
                  markers: [
                    if (_destination != null)
                      Marker(
                        point: _destination!,
                        width: 44,
                        height: 44,
                        child: _DestinationPin(),
                      ),
                    if (_driverLatLng != null)
                      Marker(
                        point: _driverLatLng!,
                        width: 44,
                        height: 44,
                        child: _DriverPin(heading: _driverLocation!.heading),
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
            if (!_loading && _driverLocation == null && _error == null)
              Positioned(
                left: 12,
                right: 12,
                top: 12,
                child: _HintBanner(
                  icon: Icons.delivery_dining_rounded,
                  text: 'Waiting for driver location…',
                ),
              ),
            Positioned(
              right: 12,
              bottom: 12,
              child: _MyLocationFab(onTap: _recenterOnMe)
                  .animate()
                  .fadeIn(duration: 300.ms)
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
        child: const Icon(Icons.delivery_dining_rounded,
            color: Colors.white, size: 22),
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
