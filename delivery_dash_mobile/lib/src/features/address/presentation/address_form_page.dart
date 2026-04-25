import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:latlong2/latlong.dart';

import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../data/address_model.dart';
import 'address_notifier.dart';

/// Create/edit page for a single Address.
///
/// - Top half: OpenStreetMap (via `flutter_map`) with a draggable pin marking the
///   selected lat/lng. Pressing the "Use my location" button re-centers the map
///   on the device's GPS after asking for permission.
/// - Bottom half: form fields (type selector + type-specific inputs).
class AddressFormPage extends ConsumerStatefulWidget {
  final Address? address;
  const AddressFormPage({super.key, this.address});

  @override
  ConsumerState<AddressFormPage> createState() => _AddressFormPageState();
}

class _AddressFormPageState extends ConsumerState<AddressFormPage> {
  // Default map center — Baghdad-ish (single-city scope per project).
  static const LatLng _fallbackCenter = LatLng(33.3152, 44.3661);

  final _formKey = GlobalKey<FormState>();
  final _mapController = MapController();

  late AddressType _type;
  late LatLng _pinLocation;
  late TextEditingController _streetCtrl;
  late TextEditingController _phoneCtrl;
  late TextEditingController _buildingCtrl;
  late TextEditingController _floorCtrl;
  late TextEditingController _aptCtrl;
  late TextEditingController _houseNameCtrl;
  late TextEditingController _houseNumberCtrl;
  late TextEditingController _companyCtrl;
  late TextEditingController _directionsCtrl;
  late TextEditingController _labelCtrl;
  bool _isDefault = false;
  bool _saving = false;
  bool _locating = false;

  bool get _isEditing => widget.address != null;

  @override
  void initState() {
    super.initState();
    final a = widget.address;
    _type = a?.type ?? AddressType.apartment;
    _pinLocation = a != null
        ? LatLng(a.latitude, a.longitude)
        : _fallbackCenter;
    _streetCtrl = TextEditingController(text: a?.street ?? '');
    _phoneCtrl = TextEditingController(text: a?.phoneNumber ?? '');
    _buildingCtrl = TextEditingController(text: a?.buildingName ?? '');
    _floorCtrl = TextEditingController(text: a?.floor ?? '');
    _aptCtrl = TextEditingController(text: a?.apartmentNumber ?? '');
    _houseNameCtrl = TextEditingController(text: a?.houseName ?? '');
    _houseNumberCtrl = TextEditingController(text: a?.houseNumber ?? '');
    _companyCtrl = TextEditingController(text: a?.companyName ?? '');
    _directionsCtrl = TextEditingController(text: a?.additionalDirections ?? '');
    _labelCtrl = TextEditingController(text: a?.label ?? '');
    _isDefault = a?.isDefault ?? false;

    // If we're creating a brand-new address, nudge toward the device's location once.
    if (!_isEditing) {
      WidgetsBinding.instance.addPostFrameCallback((_) => _useCurrentLocation(silent: true));
    }
  }

  @override
  void dispose() {
    _streetCtrl.dispose();
    _phoneCtrl.dispose();
    _buildingCtrl.dispose();
    _floorCtrl.dispose();
    _aptCtrl.dispose();
    _houseNameCtrl.dispose();
    _houseNumberCtrl.dispose();
    _companyCtrl.dispose();
    _directionsCtrl.dispose();
    _labelCtrl.dispose();
    super.dispose();
  }

  Future<void> _useCurrentLocation({bool silent = false}) async {
    setState(() => _locating = true);
    try {
      final enabled = await Geolocator.isLocationServiceEnabled();
      if (!enabled) {
        if (!silent && mounted) _snack('Turn on location services to use GPS.', error: true);
        return;
      }
      var permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
      }
      if (permission == LocationPermission.denied ||
          permission == LocationPermission.deniedForever) {
        if (!silent && mounted) _snack('Location permission is required.', error: true);
        return;
      }

      final position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );
      final next = LatLng(position.latitude, position.longitude);
      if (!mounted) return;
      setState(() => _pinLocation = next);
      _mapController.move(next, 16);
    } catch (e) {
      if (!silent && mounted) _snack('Could not fetch your location.', error: true);
    } finally {
      if (mounted) setState(() => _locating = false);
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    if (_pinLocation.latitude == 0 && _pinLocation.longitude == 0) {
      _snack('Drop a pin on the map first.', error: true);
      return;
    }

    setState(() => _saving = true);

    final request = AddressRequest(
      type: _type,
      latitude: _pinLocation.latitude,
      longitude: _pinLocation.longitude,
      phoneNumber: _phoneCtrl.text.trim(),
      street: _streetCtrl.text.trim(),
      buildingName: _maybe(_buildingCtrl),
      floor: _maybe(_floorCtrl),
      apartmentNumber: _maybe(_aptCtrl),
      houseName: _maybe(_houseNameCtrl),
      houseNumber: _maybe(_houseNumberCtrl),
      companyName: _maybe(_companyCtrl),
      additionalDirections: _maybe(_directionsCtrl),
      label: _maybe(_labelCtrl),
      isDefault: _isDefault,
    );

    try {
      final notifier = ref.read(addressesProvider.notifier);
      if (_isEditing) {
        await notifier.updateAddress(widget.address!.id, request);
      } else {
        await notifier.create(request);
      }
      if (!mounted) return;
      _snack(_isEditing ? 'Address updated' : 'Address saved');
      Navigator.pop(context);
    } catch (e) {
      if (!mounted) return;
      _snack(e.toString().replaceAll('Exception: ', ''), error: true);
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  String? _maybe(TextEditingController c) {
    final v = c.text.trim();
    return v.isEmpty ? null : v;
  }

  void _snack(String message, {bool error = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: error ? AppColors.error : AppColors.success,
        behavior: SnackBarBehavior.floating,
        shape: AppRadius.pillButtonShape,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      appBar: AppBar(
        title: Text(
          _isEditing ? 'Edit address' : 'New address',
          style: GoogleFonts.inter(fontWeight: FontWeight.w700),
        ),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.only(bottom: 32),
          children: [
            _buildMap(isDark),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _buildTypeSelector(isDark),
                  const SizedBox(height: 16),
                  _field(_streetCtrl, 'Street', icon: Icons.signpost_outlined, required: true, max: 255),
                  _field(_phoneCtrl, 'Phone', icon: Icons.phone_outlined, required: true, max: 30, keyboard: TextInputType.phone),
                  ..._typeSpecificFields(),
                  _field(_directionsCtrl, 'Additional directions (optional)', icon: Icons.map_outlined, max: 500, multiline: true),
                  _field(_labelCtrl, 'Label (e.g. Home, Work)', icon: Icons.bookmark_border_rounded, max: 50),
                  const SizedBox(height: 8),
                  SwitchListTile.adaptive(
                    contentPadding: EdgeInsets.zero,
                    value: _isDefault,
                    onChanged: (v) => setState(() => _isDefault = v),
                    title: Text('Set as default',
                        style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
                    subtitle: Text(
                      'We\'ll use this address for new orders.',
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    height: 52,
                    child: ElevatedButton(
                      onPressed: _saving ? null : _save,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                        shape: AppRadius.pillButtonShape,
                      ),
                      child: _saving
                          ? const SizedBox(
                              width: 22,
                              height: 22,
                              child: CircularProgressIndicator(
                                color: Colors.white,
                                strokeWidth: 2.5,
                              ),
                            )
                          : Text(
                              _isEditing ? 'Save changes' : 'Save address',
                              style: GoogleFonts.inter(fontWeight: FontWeight.w700),
                            ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMap(bool isDark) {
    return SizedBox(
      height: 280,
      child: Stack(
        children: [
          FlutterMap(
            mapController: _mapController,
            options: MapOptions(
              initialCenter: _pinLocation,
              initialZoom: 15,
              onTap: (_, point) => setState(() => _pinLocation = point),
            ),
            children: [
              TileLayer(
                urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                userAgentPackageName: 'com.deliverydash.mobile',
                maxNativeZoom: 19,
              ),
              MarkerLayer(
                markers: [
                  Marker(
                    point: _pinLocation,
                    width: 44,
                    height: 44,
                    alignment: Alignment.topCenter,
                    child: const Icon(
                      Icons.location_pin,
                      size: 44,
                      color: AppColors.primary,
                    ),
                  ),
                ],
              ),
            ],
          ),
          Positioned(
            right: 12,
            bottom: 12,
            child: FloatingActionButton.small(
              heroTag: 'gps-btn',
              onPressed: _locating ? null : () => _useCurrentLocation(),
              backgroundColor: isDark ? AppColors.cardBackgroundDark : Colors.white,
              foregroundColor: AppColors.primary,
              child: _locating
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2.4),
                    )
                  : const Icon(Icons.my_location_rounded),
            ),
          ),
          Positioned(
            left: 12,
            bottom: 12,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
              decoration: BoxDecoration(
                color: (isDark ? Colors.black : Colors.white).withAlpha(210),
                borderRadius: AppRadius.radiusPill,
              ),
              child: Text(
                'Tap the map to drop a pin',
                style: GoogleFonts.inter(
                  fontSize: 11,
                  fontWeight: FontWeight.w600,
                  color: isDark ? Colors.white : Colors.black87,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTypeSelector(bool isDark) {
    return SegmentedButton<AddressType>(
      segments: const [
        ButtonSegment(
          value: AddressType.apartment,
          label: Text('Apartment'),
          icon: Icon(Icons.apartment_rounded),
        ),
        ButtonSegment(
          value: AddressType.house,
          label: Text('House'),
          icon: Icon(Icons.home_rounded),
        ),
        ButtonSegment(
          value: AddressType.office,
          label: Text('Office'),
          icon: Icon(Icons.business_rounded),
        ),
      ],
      selected: {_type},
      onSelectionChanged: (sel) => setState(() => _type = sel.first),
    );
  }

  List<Widget> _typeSpecificFields() {
    switch (_type) {
      case AddressType.apartment:
        return [
          _field(_buildingCtrl, 'Building name', icon: Icons.apartment_rounded, required: true),
          _field(_floorCtrl, 'Floor', icon: Icons.layers_outlined, required: true),
          _field(_aptCtrl, 'Apartment number', icon: Icons.door_front_door_outlined, required: true),
        ];
      case AddressType.house:
        return [
          _field(_houseNameCtrl, 'House name (optional)', icon: Icons.home_outlined),
          _field(_houseNumberCtrl, 'House number', icon: Icons.pin_outlined,
              helper: 'House name or number required'),
          _field(_floorCtrl, 'Floor (optional)', icon: Icons.layers_outlined),
        ];
      case AddressType.office:
        return [
          _field(_companyCtrl, 'Company name', icon: Icons.business_center_outlined, required: true),
          _field(_buildingCtrl, 'Building name', icon: Icons.apartment_rounded, required: true),
          _field(_floorCtrl, 'Floor', icon: Icons.layers_outlined, required: true),
        ];
    }
  }

  Widget _field(
    TextEditingController controller,
    String label, {
    IconData? icon,
    bool required = false,
    int? max,
    bool multiline = false,
    TextInputType? keyboard,
    String? helper,
  }) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: TextFormField(
        controller: controller,
        keyboardType: multiline
            ? TextInputType.multiline
            : keyboard ?? TextInputType.text,
        maxLength: max,
        maxLines: multiline ? 3 : 1,
        decoration: InputDecoration(
          labelText: required ? '$label *' : label,
          helperText: helper,
          prefixIcon: icon != null ? Icon(icon) : null,
          border: OutlineInputBorder(borderRadius: AppRadius.radiusMd),
          counterText: '',
        ),
        validator: required
            ? (v) => (v == null || v.trim().isEmpty) ? 'Required' : null
            : null,
      ),
    );
  }
}
