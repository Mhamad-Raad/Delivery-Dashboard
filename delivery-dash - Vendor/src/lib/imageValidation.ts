// Shared image upload constraints — must match backend FileStorage config in appsettings.json.
export const ALLOWED_IMAGE_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.gif', '.webp'] as const;
export const MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB

// HTML `accept` attribute — use on every <input type="file"> that uploads an image.
export const IMAGE_ACCEPT_ATTR =
  '.jpg,.jpeg,.png,.gif,.webp,image/jpeg,image/png,image/gif,image/webp';

export type ImageValidationError =
  | { kind: 'extension'; allowed: string[] }
  | { kind: 'size'; maxBytes: number };

export function validateImageFile(file: File): ImageValidationError | null {
  const lower = file.name.toLowerCase();
  const allowed = ALLOWED_IMAGE_EXTENSIONS;
  if (!allowed.some((ext) => lower.endsWith(ext))) {
    return { kind: 'extension', allowed: [...allowed] };
  }
  if (file.size > MAX_IMAGE_SIZE_BYTES) {
    return { kind: 'size', maxBytes: MAX_IMAGE_SIZE_BYTES };
  }
  return null;
}

export function imageValidationMessage(err: ImageValidationError): string {
  if (err.kind === 'extension') {
    return `Only ${err.allowed.join(', ')} images are allowed.`;
  }
  const mb = Math.round(err.maxBytes / (1024 * 1024));
  return `Image must be smaller than ${mb} MB.`;
}
