namespace DeliveryDash.Application.Options
{
    public class FirebaseOptions
    {
        public const string SectionName = "Firebase";

        /// <summary>
        /// Absolute path to the service-account JSON file generated in Firebase Console.
        /// If null/empty, push notifications are disabled (service becomes a no-op).
        /// </summary>
        public string? ServiceAccountPath { get; set; }
    }
}
