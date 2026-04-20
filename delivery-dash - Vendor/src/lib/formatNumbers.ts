// Example: 1260500 => "1,260,500"
export function formatWithCommas(number: number): string {
  if (number == null) return '0';

  return number.toLocaleString();
}

// Example: 1500 => "1.5 K", 1000 => "1 K", 1010 => "1.01 K", 1260500 => "1.26 M"
export function formatCompact(number: number): string {
  if (number == null) return '0';

  if (number >= 1_000_000) {
    return parseFloat((number / 1_000_000).toFixed(2)) + ' M';
  }
  if (number >= 1_000) {
    return parseFloat((number / 1_000).toFixed(2)) + ' K';
  }
  return parseFloat(number.toFixed(2)).toString();
}

// Example: 1500 => "$1,500.00"
export function formatPrice(number: number): string {
  if (number === undefined || number === null) return '$0.00';
  return '$' + number.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
