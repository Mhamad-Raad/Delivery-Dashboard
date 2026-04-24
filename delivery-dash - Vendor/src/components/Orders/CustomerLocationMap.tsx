import L from 'leaflet';
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet';

// react-leaflet v4+ doesn't bundle marker icon assets; point Leaflet at CDN URLs.
// Without this, markers show as broken images.
const markerIcon = new L.Icon({
  iconUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
  iconRetinaUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
  shadowUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

interface CustomerLocationMapProps {
  latitude: number;
  longitude: number;
  label?: string | null;
  addressLine?: string | null;
  heightClassName?: string;
}

export function CustomerLocationMap({
  latitude,
  longitude,
  label,
  addressLine,
  heightClassName = 'h-72',
}: CustomerLocationMapProps) {
  const position: [number, number] = [latitude, longitude];

  return (
    <div
      className={`${heightClassName} w-full overflow-hidden rounded-lg border`}
    >
      <MapContainer
        center={position}
        zoom={16}
        scrollWheelZoom={false}
        style={{ height: '100%', width: '100%' }}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url='https://tile.openstreetmap.org/{z}/{x}/{y}.png'
        />
        <Marker position={position} icon={markerIcon}>
          {(label || addressLine) && (
            <Popup>
              {label && <div className='font-semibold'>{label}</div>}
              {addressLine && (
                <div className='text-muted-foreground'>{addressLine}</div>
              )}
              <div className='text-muted-foreground mt-1 text-xs'>
                {latitude.toFixed(5)}, {longitude.toFixed(5)}
              </div>
            </Popup>
          )}
        </Marker>
      </MapContainer>
    </div>
  );
}
