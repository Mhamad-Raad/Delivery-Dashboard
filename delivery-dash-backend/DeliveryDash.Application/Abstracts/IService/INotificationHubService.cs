namespace DeliveryDash.Application.Abstracts.IService
{
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(Guid userId, object notification);
    }
}